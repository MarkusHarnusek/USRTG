namespace USRTG
{
    /// <summary>
    /// Full telemetry packet. It is the union of everything the supported games expose.
    ///
    /// Conventions (every game handler must follow these so the values are comparable):
    ///  - Wheel arrays are ordered [FL, FR, RL, RR].
    ///  - Times are milliseconds, temperatures are °C, pressures are psi, speeds are km/h,
    ///    distances are metres, angles are radians.
    ///  - Gear: -1 = reverse, 0 = neutral, 1.. = forward gears.
    ///  - Pedals/steering are normalised: throttle/brake/clutch 0..1, steer -1..1.
    ///  - Enumerated fields (gameState, sessionType, flag) use the game-agnostic values from <see cref="Enums"/>.
    ///
    /// Fields that are not nullable are provided by every supported game and form the <see cref="CommonPacket"/>.
    /// Nullable fields are game specific and are omitted from the JSON when null. See Packets.md for the
    /// per-game availability table.
    /// </summary>
    public sealed class Packet
    {
        #region Meta

        /// <summary>Source game, see <see cref="Enums.Game"/>.</summary>
        public Enums.Game Game { get; init; }
        public string GameName => Game.ToString();
        /// <summary>Monotonic counter from the game (AC/ACC physics packetId, F1 frame identifier).</summary>
        public int Id { get; init; }
        /// <summary>Unix time in milliseconds at which this packet was assembled by USRTG.</summary>
        public long Timestamp { get; init; }

        #endregion

        #region Input

        public float Throttle { get; init; }
        public float Brake { get; init; }
        public float Steer { get; init; }
        public float Clutch { get; init; }

        #endregion

        #region General

        public float SpeedKmh { get; init; }
        public int Gear { get; init; }
        public int Rpms { get; init; }
        public int MaxRpm { get; init; }
        /// <summary>Fuel in the game's own unit: litres (AC, ACC) or kilograms (F1 2020).</summary>
        public float Fuel { get; init; }
        public float MaxFuel { get; init; }
        /// <summary>Fuel / MaxFuel * 100, unit independent.</summary>
        public float FuelPercent { get; init; }
        public bool PitLimiterOn { get; init; }
        public float? TurboBoost { get; init; }
        public float? Ballast { get; init; }
        public int? EngineMap { get; init; }
        public float? WaterTemp { get; init; }
        public float? EngineTemp { get; init; }
        public bool? IgnitionOn { get; init; }
        public bool? StarterEngineOn { get; init; }
        public bool? IsEngineRunning { get; init; }
        public bool? AutoShifterOn { get; init; }
        public bool? IsAiControlled { get; init; }

        #endregion

        #region Aids

        /// <summary>Traction control level. 0 = off. AC: inferred level index (-1 while unknown), ACC: TC setting, F1: 0..2.</summary>
        public int Tc { get; init; }
        /// <summary>ABS level. 0 = off. AC: inferred level index (-1 while unknown), ACC: ABS setting, F1: 0/1.</summary>
        public int Abs { get; init; }
        public int? TcCut { get; init; }
        public bool? TcInAction { get; init; }
        public bool? AbsInAction { get; init; }
        /// <summary>DRS: AC raw value, F1 0 = closed / 1 = open. Not available in ACC (GT cars).</summary>
        public float? Drs { get; init; }
        public bool? DrsAllowed { get; init; }
        public float? KersCharge { get; init; }
        public float? KersInput { get; init; }
        public float? ErsStoreEnergy { get; init; }
        public int? ErsDeployMode { get; init; }
        public float? BrakeBias { get; init; }

        #endregion

        #region Car motion

        public float Heading { get; init; }
        public float Pitch { get; init; }
        public float Roll { get; init; }
        /// <summary>Acceleration in G: [lateral, longitudinal, vertical].</summary>
        public float[] AccG { get; init; } = new float[3];
        /// <summary>Velocity in m/s in the game's coordinate system [x, y, z].</summary>
        public float[]? Velocity { get; init; }
        public float[]? LocalAngularVelocity { get; init; }
        public float[]? CarCoordinates { get; init; }

        #endregion

        #region Session

        public Enums.GameState GameState { get; init; }
        public Enums.SessionType SessionType { get; init; }
        public Enums.Flag Flag { get; init; }
        /// <summary>Milliseconds.</summary>
        public float SessionTimeLeft { get; init; }
        public int NumCars { get; init; }
        public int SectorCount { get; init; }
        /// <summary>Total laps of the session, 0 for timed sessions.</summary>
        public int TotalLaps { get; init; }
        public int? SessionIndex { get; init; }
        public int? SafetyCarStatus { get; init; }
        public int? PitWindowStart { get; init; }
        public int? PitWindowEnd { get; init; }
        public bool? IsOnline { get; init; }
        public bool? MandatoryPitDone { get; init; }
        public int? MissingMandatoryPits { get; init; }
        public int? DriverStintTotalTimeLeft { get; init; }
        public int? DriverStintTimeLeft { get; init; }
        /// <summary>ACC global flags: [yellow, yellow S1, yellow S2, yellow S3, white, green, chequered, red] as 0/1.</summary>
        public int[]? GlobalFlags { get; init; }
        /// <summary>ACC penalty type, see Mapping.md.</summary>
        public int? Penalty { get; init; }

        #endregion

        #region Environment

        public float AirTemp { get; init; }
        public float RoadTemp { get; init; }
        public float? SurfaceGrip { get; init; }
        public float? WindSpeed { get; init; }
        public float? WindDirection { get; init; }
        public float? AirDensity { get; init; }
        public int? TrackGripStatus { get; init; }
        public int? RainIntensity { get; init; }
        public int? RainIntensityIn10Min { get; init; }
        public int? RainIntensityIn30Min { get; init; }
        /// <summary>F1 2020 weather: 0 clear, 1 light cloud, 2 overcast, 3 light rain, 4 heavy rain, 5 storm.</summary>
        public int? Weather { get; init; }
        /// <summary>ACC in-game clock, seconds since midnight.</summary>
        public float? Clock { get; init; }

        #endregion

        #region Identity

        public string CarModel { get; init; } = "";
        public string Track { get; init; } = "";
        public string PlayerName { get; init; } = "";
        public string? TrackConfiguration { get; init; }
        public string? CarSkin { get; init; }

        #endregion

        #region Driver / lap

        /// <summary>0..1 position along the lap.</summary>
        public float NormalizedCarPosition { get; init; }
        public int CompletedLaps { get; init; }
        public int Position { get; init; }
        public int CurrentTime { get; init; }
        public int LastTime { get; init; }
        public int BestTime { get; init; }
        public float DistanceTraveled { get; init; }
        public bool IsInPit { get; init; }
        public bool IsInPitLane { get; init; }
        public int CurrentSectorIndex { get; init; }
        public int LastSectorTime { get; init; }
        /// <summary>Seconds of penalty time.</summary>
        public float PenaltyTime { get; init; }
        public bool? IsLapValid { get; init; }
        public int? DeltaLapTime { get; init; }
        public int? EstimatedLapTime { get; init; }
        public bool? IsDeltaPositive { get; init; }
        public int? Split { get; init; }
        public int? GapAhead { get; init; }
        public int? GapBehind { get; init; }
        public int? Sector1Time { get; init; }
        public int? Sector2Time { get; init; }
        public int? GridPosition { get; init; }
        public int? DriverStatus { get; init; }
        public float? FuelPerLap { get; init; }
        public float? FuelEstimatedLaps { get; init; }
        public float? UsedFuel { get; init; }
        public float? ReplayTimeMultiplier { get; init; }
        public bool? IdealLineOn { get; init; }

        #endregion

        #region Tyres and brakes

        public string TyreCompound { get; init; } = "";
        public float[] TyreCoreTemp { get; init; } = new float[4];
        public float[] TyrePressure { get; init; } = new float[4];
        public float[] BrakeTemp { get; init; } = new float[4];
        public float[] WheelSlip { get; init; } = new float[4];
        public float[]? TyreWear { get; init; }
        public float[]? TyreTempInner { get; init; }
        public float[]? TyreTempMiddle { get; init; }
        public float[]? TyreTempOuter { get; init; }
        public float[]? TyreSurfaceTemp { get; init; }
        public float[]? TyreDirtyLevel { get; init; }
        public float[]? WheelLoad { get; init; }
        public float[]? WheelAngularSpeed { get; init; }
        public float[]? SlipRatio { get; init; }
        public float[]? SlipAngle { get; init; }
        public float[]? SuspensionTravel { get; init; }
        public float[]? RideHeight { get; init; }
        public float[]? BrakePressure { get; init; }
        public float[]? PadLife { get; init; }
        public float[]? DiscLife { get; init; }
        public int? FrontBrakeCompound { get; init; }
        public int? RearBrakeCompound { get; init; }
        public int? TyresAgeLaps { get; init; }
        public int? CurrentTyreSet { get; init; }
        public int? StrategyTyreSet { get; init; }
        public bool? RainTyres { get; init; }
        public string? VisualTyreCompound { get; init; }

        #endregion

        #region Damage

        /// <summary>AC/ACC: [front, rear, left, right, centre]. F1: [FL wing, FR wing, rear wing, engine, gearbox] in %.</summary>
        public float[]? CarDamage { get; init; }
        public float[]? SuspensionDamage { get; init; }
        public float[]? TyreDamage { get; init; }
        public int? NumberOfTyresOut { get; init; }

        #endregion

        #region Cockpit (ACC)

        public bool? RainLights { get; init; }
        public bool? FlashingLights { get; init; }
        public int? LightsStage { get; init; }
        public int? WiperLevel { get; init; }
        public bool? DirectionLightsLeft { get; init; }
        public bool? DirectionLightsRight { get; init; }
        public float? ExhaustTemperature { get; init; }
        public int? MfdTyreSet { get; init; }
        public float? MfdFuelToAdd { get; init; }
        public float[]? MfdTyrePressure { get; init; }
        public float? KerbVibration { get; init; }
        public float? SlipVibration { get; init; }
        public float? GVibration { get; init; }
        public float? AbsVibration { get; init; }
        public float? FinalFF { get; init; }

        #endregion
    }
}
