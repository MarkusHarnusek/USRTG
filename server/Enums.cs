namespace USRTG
{
    /// <summary>
    /// Game-agnostic enumerations. Every game-specific value is translated into one of
    /// these by <see cref="Mapping"/> before it is put into a <see cref="Packet"/>.
    /// The numeric values are part of the public JSON contract, see Mapping.md.
    /// </summary>
    public static class Enums
    {
        public enum Game
        {
            Unknown = 0,
            AssettoCorsa = 1,
            AssettoCorsaCompetizione = 2,
            F1_2020 = 3,
            BeamNGDrive = 4,
            RaceRoom = 5
        }

        public enum GameState
        {
            Off = 0,
            Replay = 1,
            Live = 2,
            Paused = 3
        }

        public enum SessionType
        { 
            Unknown = -1,
            Practice = 0,
            Qualify = 1,
            Race = 2,
            Hotlap = 3,
            TimeAttack = 4,
            Drift = 5,
            Drag = 6,
            Hotstint = 7,
            Superpole = 8
        }

        public enum Flag
        {
            NoFlag = 0,
            Green = 1,
            Yellow = 2,
            WavedYellow = 3,
            DoubleYellow = 4,
            White = 5,
            Checkered = 6,
            Blue = 7,
            WavedBlue = 8,
            Black = 9,
            OrangeBlack = 10,
            BlackWhite = 11,
            YellowRed = 12,
            GreenYellow = 13,
            SafetyCar = 14,
            VirtualSafetyCar = 15,
            Code60 = 16,
            FullCourseYellow = 17,
            Red = 18
        }
    }
}
