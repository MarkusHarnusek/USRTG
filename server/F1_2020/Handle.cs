using System.Net;
using System.Net.Sockets;
using USRTG.Telemetry;

namespace USRTG.F1_2020
{
    /// <summary>
    /// Telemetry source for F1 2020. The game pushes UDP datagrams (default port 20777, "UDP format 2020")
    /// at the rate chosen in the game's telemetry settings. The listener keeps the latest decoded data of
    /// each packet type for the player's car and assembles a <see cref="Packet"/> on demand.
    ///
    /// Enable it in game: Settings → Telemetry Settings → UDP Telemetry: On, UDP Format: 2020, port 20777.
    /// </summary>
    public sealed class Handle : ITelemetrySource
    {
        /// <summary>No packet for this long means the game is gone (the slowest packet type arrives every 5 s).</summary>
        private static readonly TimeSpan StaleAfter = TimeSpan.FromSeconds(6);

        private readonly int _port;
        private readonly CancellationTokenSource _cts = new();
        private UdpClient? _udp;
        private Task? _receiveTask;
        private bool _warnedFormat;

        // Latest data per packet type. Records are immutable so a reference swap is an atomic publish.
        private volatile SessionData? _session;
        private volatile LapData? _lap;
        private volatile CarTelemetryData? _telemetry;
        private volatile CarStatusData? _status;
        private volatile MotionData? _motion;
        private volatile ParticipantsData? _participants;
        private volatile uint _lastFrameId;
        private long _lastReceivedTicks;

        // Sector bookkeeping: the spec only sends S1/S2 of the current lap, so S3 is derived at lap change.
        private byte _lapNum;
        private int _s1, _s2;
        private volatile int _lastSectorTime; // written on the receive thread, read on the poll thread

        public Handle(int port = Spec.DefaultPort)
        {
            _port = port;
        }

        public string Name => "F1 2020";

        public bool IsListening => _udp != null;

        public void Start()
        {
            try
            {
                _udp = new UdpClient(AddressFamily.InterNetwork);
                _udp.Client.ReceiveBufferSize = 1 << 20; // the game can send > 1 MB/s at 60 Hz
                _udp.Client.Bind(new IPEndPoint(IPAddress.Any, _port));
            }
            catch (SocketException ex)
            {
                Console.WriteLine($"[F1 2020] Cannot listen on UDP port {_port} ({ex.SocketErrorCode}); F1 2020 support disabled. " +
                                  "Another telemetry tool may already own the port.");
                _udp?.Dispose();
                _udp = null;
                return;
            }

            _receiveTask = Task.Run(() => ReceiveLoop(_cts.Token));
            Console.WriteLine($"[F1 2020] Listening for UDP telemetry on port {_port}");
        }

        private async Task ReceiveLoop(CancellationToken token)
        {
            var udp = _udp!;
            while (!token.IsCancellationRequested)
            {
                UdpReceiveResult result;
                try
                {
                    result = await udp.ReceiveAsync(token);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (ObjectDisposedException)
                {
                    break;
                }
                catch (SocketException)
                {
                    // ICMP "port unreachable" from a previous send shows up here on Windows; just keep receiving.
                    continue;
                }

                try
                {
                    Process(result.Buffer);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[F1 2020] Failed to decode packet: {ex.Message}");
                }
            }
        }

        /// <summary>Decodes one datagram. Internal so the parser pipeline can be unit tested without a socket.</summary>
        internal void Process(ReadOnlySpan<byte> data)
        {
            if (!Parser.TryParseHeader(data, out var header))
            {
                return;
            }

            if (header.PacketFormat != Spec.PacketFormat)
            {
                if (!_warnedFormat)
                {
                    Console.WriteLine($"[F1 2020] Ignoring packets with UDP format {header.PacketFormat}; set the game's UDP format to 2020.");
                    _warnedFormat = true;
                }
                return;
            }

            int car = header.PlayerCarIndex;
            switch (header.PacketId)
            {
                case Spec.MotionId:
                    _motion = Parser.ParseMotion(data, car) ?? _motion;
                    break;
                case Spec.SessionId:
                    _session = Parser.ParseSession(data) ?? _session;
                    break;
                case Spec.LapDataId:
                    var lap = Parser.ParseLapData(data, car);
                    if (lap != null)
                    {
                        UpdateSectors(lap);
                        _lap = lap;
                    }
                    break;
                case Spec.ParticipantsId:
                    _participants = Parser.ParseParticipants(data, car) ?? _participants;
                    break;
                case Spec.CarTelemetryId:
                    _telemetry = Parser.ParseCarTelemetry(data, car) ?? _telemetry;
                    break;
                case Spec.CarStatusId:
                    _status = Parser.ParseCarStatus(data, car) ?? _status;
                    break;
                default:
                    return; // event, setups, final classification, lobby: not needed
            }

            _lastFrameId = header.FrameIdentifier;
            Interlocked.Exchange(ref _lastReceivedTicks, Environment.TickCount64);
        }

        private void UpdateSectors(LapData lap)
        {
            if (lap.CurrentLapNum != _lapNum)
            {
                // New lap: the previous lap's S3 is its total minus the two sectors we saw.
                int lastLapMs = Units.SecondsToMs(lap.LastLapTime);
                if (_lapNum != 0 && lastLapMs > 0 && _s1 > 0 && _s2 > 0 && lastLapMs > _s1 + _s2)
                {
                    _lastSectorTime = lastLapMs - _s1 - _s2;
                }
                _lapNum = lap.CurrentLapNum;
                _s1 = 0;
                _s2 = 0;
            }

            if (lap.Sector1TimeMs > 0)
            {
                _s1 = lap.Sector1TimeMs;
            }
            if (lap.Sector2TimeMs > 0)
            {
                _s2 = lap.Sector2TimeMs;
            }

            _lastSectorTime = lap.Sector switch
            {
                1 when _s1 > 0 => _s1,
                2 when _s2 > 0 => _s2,
                _ => _lastSectorTime
            };
        }

        public Packet? TryRead()
        {
            long last = Interlocked.Read(ref _lastReceivedTicks);
            if (last == 0 || Environment.TickCount64 - last > StaleAfter.TotalMilliseconds)
            {
                return null;
            }

            var session = _session;
            var lap = _lap;
            var telemetry = _telemetry;
            var status = _status;
            if (session == null || lap == null || telemetry == null || status == null)
            {
                return null; // still collecting the first packets of each type
            }

            return Build(_lastFrameId, session, lap, telemetry, status, _motion, _participants, _lastSectorTime);
        }

        internal static Packet Build(uint frameIdentifier, SessionData session, LapData lap, CarTelemetryData telemetry, CarStatusData status,
                                     MotionData? motion, ParticipantsData? participants, int lastSectorTime)
        {
            float normalizedPosition = session.TrackLength > 0
                ? Math.Clamp(lap.LapDistance / session.TrackLength, 0f, 1f)
                : 0f;

            return new Packet
            {
                Game = USRTG.Enums.Game.F1_2020,
                Id = unchecked((int)frameIdentifier),
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),

                Throttle = telemetry.Throttle,
                Brake = telemetry.Brake,
                Steer = telemetry.Steer,
                Clutch = telemetry.Clutch / 100f,

                SpeedKmh = telemetry.Speed,
                Gear = telemetry.Gear,
                Rpms = telemetry.EngineRpm,
                MaxRpm = status.MaxRpm,
                Fuel = status.FuelInTank,
                MaxFuel = status.FuelCapacity,
                FuelPercent = Units.Percent(status.FuelInTank, status.FuelCapacity),
                PitLimiterOn = status.PitLimiterStatus != 0,
                EngineTemp = telemetry.EngineTemperature,
                IsAiControlled = participants?.AiControlled,

                Tc = status.TractionControl,
                Abs = status.AntiLockBrakes,
                Drs = telemetry.Drs,
                DrsAllowed = status.DrsAllowed != 0,
                ErsStoreEnergy = status.ErsStoreEnergy,
                ErsDeployMode = status.ErsDeployMode,
                BrakeBias = status.FrontBrakeBias / 100f,

                Heading = motion?.Yaw ?? 0f,
                Pitch = motion?.Pitch ?? 0f,
                Roll = motion?.Roll ?? 0f,
                AccG = motion != null ? [motion.GForceLateral, motion.GForceLongitudinal, motion.GForceVertical] : new float[3],
                Velocity = motion != null ? [motion.WorldVelocityX, motion.WorldVelocityY, motion.WorldVelocityZ] : null,
                LocalAngularVelocity = motion?.AngularVelocity,
                CarCoordinates = motion != null ? [motion.WorldPositionX, motion.WorldPositionY, motion.WorldPositionZ] : null,

                GameState = Mapping.MapF1GameState(session.GamePaused),
                SessionType = Mapping.MapF1SessionType(session.SessionType),
                Flag = Mapping.MapF1Flag(status.VehicleFiaFlags, session.SafetyCarStatus),
                SessionTimeLeft = session.SessionTimeLeft * 1000f,
                NumCars = participants?.NumActiveCars ?? 0,
                SectorCount = 3,
                TotalLaps = session.TotalLaps,
                SafetyCarStatus = session.SafetyCarStatus,
                IsOnline = session.NetworkGame,

                AirTemp = session.AirTemperature,
                RoadTemp = session.TrackTemperature,
                Weather = session.Weather,

                CarModel = participants != null ? Lookups.Team(participants.TeamId) : "",
                Track = Lookups.Track(session.TrackId),
                PlayerName = participants?.Name ?? "",

                NormalizedCarPosition = normalizedPosition,
                CompletedLaps = Math.Max(0, lap.CurrentLapNum - 1),
                Position = lap.CarPosition,
                CurrentTime = Units.SecondsToMs(lap.CurrentLapTime),
                LastTime = Units.SecondsToMs(lap.LastLapTime),
                BestTime = Units.SecondsToMs(lap.BestLapTime),
                DistanceTraveled = lap.TotalDistance,
                IsInPit = lap.PitStatus == 2,
                IsInPitLane = lap.PitStatus >= 1,
                CurrentSectorIndex = lap.Sector,
                LastSectorTime = lastSectorTime,
                PenaltyTime = lap.Penalties,
                IsLapValid = !lap.CurrentLapInvalid,
                Sector1Time = lap.Sector1TimeMs,
                Sector2Time = lap.Sector2TimeMs,
                GridPosition = lap.GridPosition,
                DriverStatus = lap.DriverStatus,
                FuelEstimatedLaps = status.FuelRemainingLaps,

                TyreCompound = Lookups.ActualCompound(status.ActualTyreCompound),
                VisualTyreCompound = Lookups.VisualCompound(status.VisualTyreCompound),
                // F1 has no "core" temperature; the inner (carcass) temperature is the closest equivalent.
                TyreCoreTemp = Units.F1Wheels(telemetry.TyresInnerTemperature),
                TyreSurfaceTemp = Units.F1Wheels(telemetry.TyresSurfaceTemperature),
                TyreTempInner = Units.F1Wheels(telemetry.TyresInnerTemperature),
                TyrePressure = Units.F1Wheels(telemetry.TyresPressure),
                BrakeTemp = Units.F1Wheels(telemetry.BrakesTemperature),
                WheelSlip = motion != null ? Units.F1Wheels(motion.WheelSlip) : new float[4],
                TyreWear = Units.F1Wheels(status.TyresWear),
                TyresAgeLaps = status.TyresAgeLaps,

                CarDamage = [status.FrontLeftWingDamage, status.FrontRightWingDamage, status.RearWingDamage, status.EngineDamage, status.GearBoxDamage],
                TyreDamage = Units.F1Wheels(status.TyresDamage)
            };
        }

        public void Dispose()
        {
            _cts.Cancel();
            _udp?.Dispose();
            try
            {
                _receiveTask?.Wait(TimeSpan.FromSeconds(1));
            }
            catch (AggregateException)
            {
                // cancellation
            }
            _cts.Dispose();
        }
    }
}
