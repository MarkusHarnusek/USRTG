namespace USRTG
{
    /// <summary>
    /// Game-agnostic telemetry packet. It contains only the data that every supported game
    /// (Assetto Corsa, Assetto Corsa Competizione, F1 2020) can provide, normalised to the
    /// conventions documented on <see cref="Packet"/>. This is the only packet the web server serves.
    ///
    /// See Packets.md for the per-field source in each game.
    /// </summary>
    public sealed class CommonPacket
    {
        // Meta
        public Enums.Game Game { get; init; }
        public string GameName => Game.ToString();
        public int Id { get; init; }
        public long Timestamp { get; init; }

        // Input (normalised: 0..1, steer -1..1)
        public float Throttle { get; init; }
        public float Brake { get; init; }
        public float Steer { get; init; }
        public float Clutch { get; init; }

        // General
        public float SpeedKmh { get; init; }
        public int Gear { get; init; }
        public int Rpms { get; init; }
        public int MaxRpm { get; init; }
        public float Fuel { get; init; }
        public float MaxFuel { get; init; }
        public float FuelPercent { get; init; }
        public bool PitLimiterOn { get; init; }

        // Aids
        public int Tc { get; init; }
        public int Abs { get; init; }

        // Car motion
        public float Heading { get; init; }
        public float Pitch { get; init; }
        public float Roll { get; init; }
        public float[] AccG { get; init; } = new float[3];

        // Session
        public Enums.GameState GameState { get; init; }
        public Enums.SessionType SessionType { get; init; }
        public Enums.Flag Flag { get; init; }
        public float SessionTimeLeft { get; init; }
        public int NumCars { get; init; }
        public int SectorCount { get; init; }
        public int TotalLaps { get; init; }

        // Environment
        public float AirTemp { get; init; }
        public float RoadTemp { get; init; }

        // Identity
        public string CarModel { get; init; } = "";
        public string Track { get; init; } = "";
        public string PlayerName { get; init; } = "";

        // Driver / lap
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
        public float PenaltyTime { get; init; }

        // Tyres and brakes [FL, FR, RL, RR]
        public string TyreCompound { get; init; } = "";
        public float[] TyreCoreTemp { get; init; } = new float[4];
        public float[] TyrePressure { get; init; } = new float[4];
        public float[] BrakeTemp { get; init; } = new float[4];
        public float[] WheelSlip { get; init; } = new float[4];

        /// <summary>Projects the common subset out of a full <see cref="Packet"/>.</summary>
        public static CommonPacket From(Packet p) => new()
        {
            Game = p.Game,
            Id = p.Id,
            Timestamp = p.Timestamp,
            Throttle = p.Throttle,
            Brake = p.Brake,
            Steer = p.Steer,
            Clutch = p.Clutch,
            SpeedKmh = p.SpeedKmh,
            Gear = p.Gear,
            Rpms = p.Rpms,
            MaxRpm = p.MaxRpm,
            Fuel = p.Fuel,
            MaxFuel = p.MaxFuel,
            FuelPercent = p.FuelPercent,
            PitLimiterOn = p.PitLimiterOn,
            Tc = p.Tc,
            Abs = p.Abs,
            Heading = p.Heading,
            Pitch = p.Pitch,
            Roll = p.Roll,
            AccG = p.AccG,
            GameState = p.GameState,
            SessionType = p.SessionType,
            Flag = p.Flag,
            SessionTimeLeft = p.SessionTimeLeft,
            NumCars = p.NumCars,
            SectorCount = p.SectorCount,
            TotalLaps = p.TotalLaps,
            AirTemp = p.AirTemp,
            RoadTemp = p.RoadTemp,
            CarModel = p.CarModel,
            Track = p.Track,
            PlayerName = p.PlayerName,
            NormalizedCarPosition = p.NormalizedCarPosition,
            CompletedLaps = p.CompletedLaps,
            Position = p.Position,
            CurrentTime = p.CurrentTime,
            LastTime = p.LastTime,
            BestTime = p.BestTime,
            DistanceTraveled = p.DistanceTraveled,
            IsInPit = p.IsInPit,
            IsInPitLane = p.IsInPitLane,
            CurrentSectorIndex = p.CurrentSectorIndex,
            LastSectorTime = p.LastSectorTime,
            PenaltyTime = p.PenaltyTime,
            TyreCompound = p.TyreCompound,
            TyreCoreTemp = p.TyreCoreTemp,
            TyrePressure = p.TyrePressure,
            BrakeTemp = p.BrakeTemp,
            WheelSlip = p.WheelSlip
        };
    }
}
