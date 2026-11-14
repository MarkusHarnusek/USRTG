using System.Collections.Frozen;

namespace USRTG
{
    /// <summary>
    /// Translates game specific enumerations into the game-agnostic values of <see cref="Enums"/>.
    /// All lookup tables are built once; the mapping functions allocate nothing.
    /// </summary>
    public static class Mapping
    {
        // ---------------------------------------------------------------- game state

        /// <summary>AC and ACC share the same status values (0 off, 1 replay, 2 live, 3 pause).</summary>
        public static Enums.GameState MapKunosStatus(int status) => status switch
        {
            0 => Enums.GameState.Off,
            1 => Enums.GameState.Replay,
            2 => Enums.GameState.Live,
            3 => Enums.GameState.Paused,
            _ => Enums.GameState.Off
        };

        /// <summary>F1 2020 has no explicit status. A packet is only built while data flows, so it is live unless paused.</summary>
        public static Enums.GameState MapF1GameState(bool gamePaused) =>
            gamePaused ? Enums.GameState.Paused : Enums.GameState.Live;

        // ---------------------------------------------------------------- session type

        /// <summary>AC and ACC session values are already the unified values (ACC adds hotstint 7 and superpole 8).</summary>
        public static Enums.SessionType MapKunosSessionType(int session) =>
            session is >= -1 and <= 8 ? (Enums.SessionType)session : Enums.SessionType.Unknown;

        /// <summary>F1 2020 m_sessionType.</summary>
        public static Enums.SessionType MapF1SessionType(int sessionType) => sessionType switch
        {
            1 or 2 or 3 or 4 => Enums.SessionType.Practice,   // P1, P2, P3, short practice
            5 or 6 or 7 or 8 or 9 => Enums.SessionType.Qualify, // Q1, Q2, Q3, short Q, one shot Q
            10 or 11 => Enums.SessionType.Race,               // race, race 2
            12 => Enums.SessionType.TimeAttack,               // time trial
            _ => Enums.SessionType.Unknown
        };

        // ---------------------------------------------------------------- flags

        private static readonly FrozenDictionary<int, Enums.Flag> AcFlags = new Dictionary<int, Enums.Flag>
        {
            { 0, Enums.Flag.NoFlag },
            { 1, Enums.Flag.Blue },
            { 2, Enums.Flag.Yellow },
            { 3, Enums.Flag.Black },
            { 4, Enums.Flag.White },
            { 5, Enums.Flag.Checkered },
            { 6, Enums.Flag.Black }       // AC "penalty flag" is shown as a black flag
        }.ToFrozenDictionary();

        private static readonly FrozenDictionary<int, Enums.Flag> AccFlags = new Dictionary<int, Enums.Flag>
        {
            { 0, Enums.Flag.NoFlag },
            { 1, Enums.Flag.Blue },
            { 2, Enums.Flag.Yellow },
            { 3, Enums.Flag.Black },
            { 4, Enums.Flag.White },
            { 5, Enums.Flag.Checkered },
            { 6, Enums.Flag.Black },      // penalty
            { 7, Enums.Flag.Green },
            { 8, Enums.Flag.OrangeBlack } // orange "meatball" flag
        }.ToFrozenDictionary();

        public static Enums.Flag MapAcFlag(int flag) =>
            AcFlags.TryGetValue(flag, out var mapped) ? mapped : Enums.Flag.NoFlag;

        public static Enums.Flag MapAccFlag(int flag) =>
            AccFlags.TryGetValue(flag, out var mapped) ? mapped : Enums.Flag.NoFlag;

        /// <summary>
        /// F1 2020: m_vehicleFiaFlags (-1 unknown, 0 none, 1 green, 2 blue, 3 yellow, 4 red) combined with
        /// m_safetyCarStatus (0 none, 1 full SC, 2 VSC, 3 formation lap). A safety car period takes precedence
        /// because it is the information a driver acts on; the vehicle flag is used otherwise.
        /// </summary>
        public static Enums.Flag MapF1Flag(int vehicleFiaFlags, int safetyCarStatus)
        {
            switch (safetyCarStatus)
            {
                case 1: return Enums.Flag.SafetyCar;
                case 2: return Enums.Flag.VirtualSafetyCar;
            }

            return vehicleFiaFlags switch
            {
                1 => Enums.Flag.Green,
                2 => Enums.Flag.Blue,
                3 => Enums.Flag.Yellow,
                4 => Enums.Flag.Red,
                _ => Enums.Flag.NoFlag
            };
        }

        // ---------------------------------------------------------------- gears

        /// <summary>AC/ACC gear: 0 = reverse, 1 = neutral, 2 = first... Unified: -1 = reverse, 0 = neutral, 1 = first...</summary>
        public static int MapKunosGear(int gear) => gear - 1;
    }
}
