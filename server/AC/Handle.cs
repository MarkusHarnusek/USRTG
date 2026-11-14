using System.Runtime.Versioning;
using USRTG.Kunos;

namespace USRTG.AC
{
    /// <summary>Builds a <see cref="Packet"/> from the Assetto Corsa shared memory.</summary>
    [SupportedOSPlatform("windows")]
    public sealed class Handle : IDisposable
    {
        private readonly SharedMemoryPage<Structs.SPageFilePhysics> _physics = new(Structs.PhysicsMapName);
        private readonly SharedMemoryPage<Structs.SPageFileGraphic> _graphics = new(Structs.GraphicsMapName);
        private readonly SharedMemoryPage<Structs.SPageFileStatic> _static = new(Structs.StaticMapName);
        private readonly LevelMapping _tc = new();
        private readonly LevelMapping _abs = new();

        public Packet? TryRead()
        {
            if (!_static.TryRead(out var s) || !_graphics.TryRead(out var g) || !_physics.TryRead(out var p))
            {
                Close();
                return null;
            }

            return Build(p, g, s);
        }

        /// <summary>Pure translation of the three pages into a packet; separated so it can be unit tested.</summary>
        internal Packet Build(in Structs.SPageFilePhysics p, in Structs.SPageFileGraphic g, in Structs.SPageFileStatic s)
        {
            return new Packet
            {
                Game = USRTG.Enums.Game.AssettoCorsa,
                Id = p.packetId,
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),

                Throttle = p.gas,
                Brake = p.brake,
                Steer = p.steerAngle,
                Clutch = p.clutch,

                SpeedKmh = p.speedKmh,
                Gear = Mapping.MapKunosGear(p.gear),
                Rpms = p.rpms,
                MaxRpm = s.maxRpm,
                Fuel = p.fuel,
                MaxFuel = s.maxFuel,
                FuelPercent = Units.Percent(p.fuel, s.maxFuel),
                PitLimiterOn = p.pitLimiterOn != 0,
                TurboBoost = p.turboBoost,
                Ballast = p.ballast,
                AutoShifterOn = p.autoShifterOn != 0,
                IsAiControlled = p.isAIControlled != 0,

                Tc = _tc.Update(s.carModel, p.tc),
                Abs = _abs.Update(s.carModel, p.abs),
                Drs = p.drs,
                DrsAllowed = p.drsAvailable != 0,
                KersCharge = p.kersCharge,
                KersInput = p.kersInput,
                BrakeBias = p.brakeBias,

                Heading = p.heading,
                Pitch = p.pitch,
                Roll = p.roll,
                AccG = Units.KunosAccG(p.accG),
                Velocity = p.velocity,
                LocalAngularVelocity = p.localAngularVel,
                CarCoordinates = g.carCoordinates,

                GameState = Mapping.MapKunosStatus(g.status),
                SessionType = Mapping.MapKunosSessionType(g.session),
                Flag = Mapping.MapAcFlag(g.flag),
                SessionTimeLeft = g.sessionTimeLeft,
                NumCars = s.numCars,
                SectorCount = s.sectorCount,
                TotalLaps = g.numberOfLaps,
                PitWindowStart = s.pitWindowStart,
                PitWindowEnd = s.pitWindowEnd,
                IsOnline = s.isOnline != 0,
                MandatoryPitDone = g.mandatoryPitDone != 0,

                AirTemp = p.airTemp,
                RoadTemp = p.roadTemp,
                SurfaceGrip = g.surfaceGrip,
                WindSpeed = g.windSpeed,
                WindDirection = g.windDirection,
                AirDensity = p.airDensity,

                CarModel = s.carModel ?? "",
                Track = s.track ?? "",
                PlayerName = Units.JoinName(s.playerName, s.playerSurname),
                TrackConfiguration = s.trackConfiguration,
                CarSkin = s.carSkin,

                NormalizedCarPosition = g.normalizedCarPosition,
                CompletedLaps = g.completedLaps,
                Position = g.position,
                CurrentTime = g.iCurrentTime,
                LastTime = g.iLastTime,
                BestTime = g.iBestTime,
                DistanceTraveled = g.distanceTraveled,
                IsInPit = g.isInPit != 0,
                IsInPitLane = g.isInPitLane != 0,
                CurrentSectorIndex = g.currentSectorIndex,
                LastSectorTime = g.lastSectorTime,
                PenaltyTime = g.penaltyTime,
                ReplayTimeMultiplier = g.replayTimeMultiplier,
                IdealLineOn = g.idealLineOn != 0,

                TyreCompound = g.tyreCompound ?? "",
                TyreCoreTemp = p.tyreCoreTemperature,
                TyrePressure = p.wheelsPressure,
                BrakeTemp = p.brakeTemp,
                WheelSlip = p.wheelSlip,
                TyreWear = p.tyreWear,
                TyreTempInner = p.tyreTempI,
                TyreTempMiddle = p.tyreTempM,
                TyreTempOuter = p.tyreTempO,
                TyreDirtyLevel = p.tyreDirtyLevel,
                WheelLoad = p.wheelLoad,
                WheelAngularSpeed = p.wheelAngularSpeed,
                SuspensionTravel = p.suspensionTravel,
                RideHeight = p.rideHeight,

                CarDamage = p.carDamage,
                NumberOfTyresOut = p.numberOfTyresOut,
                FinalFF = p.finalFF
            };
        }

        public void Close()
        {
            _physics.Close();
            _graphics.Close();
            _static.Close();
        }

        public void Dispose()
        {
            _physics.Dispose();
            _graphics.Dispose();
            _static.Dispose();
        }
    }
}
