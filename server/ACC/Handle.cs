using System.Runtime.Versioning;
using USRTG.Kunos;

namespace USRTG.ACC
{
    /// <summary>Builds a <see cref="Packet"/> from the Assetto Corsa Competizione shared memory.</summary>
    [SupportedOSPlatform("windows")]
    public sealed class Handle : IDisposable
    {
        private readonly SharedMemoryPage<Structs.SPageFilePhysics> _physics = new(Structs.PhysicsMapName);
        private readonly SharedMemoryPage<Structs.SPageFileGraphic> _graphics = new(Structs.GraphicsMapName);
        private readonly SharedMemoryPage<Structs.SPageFileStatic> _static = new(Structs.StaticMapName);

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
        internal static Packet Build(in Structs.SPageFilePhysics p, in Structs.SPageFileGraphic g, in Structs.SPageFileStatic s)
        {
            return new Packet
            {
                Game = USRTG.Enums.Game.AssettoCorsaCompetizione,
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
                EngineMap = g.EngineMap,
                WaterTemp = p.waterTemp,
                IgnitionOn = p.ignitionOn != 0,
                StarterEngineOn = p.starterEngineOn != 0,
                IsEngineRunning = p.isEngineRunning != 0,
                AutoShifterOn = p.autoShifterOn != 0,
                IsAiControlled = p.isAIControlled != 0,

                // ACC publishes the actual TC/ABS setting on the graphics page; no inference needed.
                Tc = g.TC,
                Abs = g.ABS,
                TcCut = g.TCCut,
                TcInAction = p.tcInAction != 0,
                AbsInAction = p.absInAction != 0,
                BrakeBias = p.brakeBias,

                Heading = p.heading,
                Pitch = p.pitch,
                Roll = p.roll,
                AccG = Units.KunosAccG(p.accG),
                Velocity = p.velocity,
                LocalAngularVelocity = p.localAngularVel,
                CarCoordinates = Units.PlayerCoordinates(g.carCoordinates, g.carID, g.playerCarID, g.activeCars),

                GameState = Mapping.MapKunosStatus(g.status),
                SessionType = Mapping.MapKunosSessionType(g.session),
                Flag = Mapping.MapAccFlag(g.flag),
                SessionTimeLeft = g.sessionTimeLeft,
                NumCars = g.activeCars > 0 ? g.activeCars : s.numCars,
                SectorCount = s.sectorCount,
                TotalLaps = g.numberOfLaps,
                SessionIndex = g.sessionIndex,
                PitWindowStart = s.PitWindowStart,
                PitWindowEnd = s.PitWindowEnd,
                IsOnline = s.isOnline != 0,
                MandatoryPitDone = g.mandatoryPitDone != 0,
                MissingMandatoryPits = g.missingMandatoryPits,
                DriverStintTotalTimeLeft = g.DriverStintTotalTimeLeft,
                DriverStintTimeLeft = g.DriverStintTimeLeft,
                GlobalFlags = [g.GlobalYellow, g.GlobalYellow1, g.GlobalYellow2, g.GlobalYellow3, g.GlobalWhite, g.GlobalGreen, g.GlobalChequered, g.GlobalRed],
                Penalty = g.penalty,

                AirTemp = p.airTemp,
                RoadTemp = p.roadTemp,
                SurfaceGrip = g.surfaceGrip,
                WindSpeed = g.windSpeed,
                WindDirection = g.windDirection,
                TrackGripStatus = g.trackGripStatus,
                RainIntensity = g.rainIntensity,
                RainIntensityIn10Min = g.rainIntensityIn10min,
                RainIntensityIn30Min = g.rainIntensityIn30min,
                Clock = g.Clock,

                CarModel = s.carModel ?? "",
                Track = s.track ?? "",
                PlayerName = Units.JoinName(s.playerName, s.playerSurname),

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
                IsLapValid = g.isValidLap != 0,
                DeltaLapTime = g.iDeltaLapTime,
                EstimatedLapTime = g.iEstimatedLapTime,
                IsDeltaPositive = g.isDeltaPositive != 0,
                Split = g.iSplit,
                GapAhead = g.gapAhead,
                GapBehind = g.gapBehind,
                FuelPerLap = g.fuelXLap,
                FuelEstimatedLaps = g.fuelEstimatedLaps,
                UsedFuel = g.usedFuel,
                IdealLineOn = g.idealLineOn != 0,

                TyreCompound = g.tyreCompound ?? "",
                TyreCoreTemp = p.tyreCoreTemperature,
                TyrePressure = p.wheelsPressure,
                BrakeTemp = p.brakeTemp,
                WheelSlip = p.wheelSlip,
                WheelAngularSpeed = p.wheelAngularSpeed,
                SlipRatio = p.slipRatio,
                SlipAngle = p.slipAngle,
                SuspensionTravel = p.suspensionTravel,
                BrakePressure = p.brakePressure,
                PadLife = p.padLife,
                DiscLife = p.discLife,
                FrontBrakeCompound = p.frontBrakeCompound,
                RearBrakeCompound = p.rearBrakeCompound,
                CurrentTyreSet = g.currentTyreSet,
                StrategyTyreSet = g.strategyTyreSet,
                RainTyres = g.rainTyres != 0,

                CarDamage = p.carDamage,

                RainLights = g.rainLights != 0,
                FlashingLights = g.flashingLights != 0,
                LightsStage = g.lightsStage,
                WiperLevel = g.wiperLV,
                DirectionLightsLeft = g.directionLightsLeft != 0,
                DirectionLightsRight = g.directionLightsRight != 0,
                ExhaustTemperature = g.exhaustTemperature,
                MfdTyreSet = g.mfdTyreSet,
                MfdFuelToAdd = g.mfdFuelToAdd,
                MfdTyrePressure = [g.mfdTyrePressureLF, g.mfdTyrePressureRF, g.mfdTyrePressureLR, g.mfdTyrePressureRR],
                KerbVibration = p.kerbVibration,
                SlipVibration = p.slipVibrations,
                GVibration = p.gVibrations,
                AbsVibration = p.absVibrations,
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
