namespace USRTG.ACC
{
    /// <summary>Raw Assetto Corsa Competizione enumerations as found in the shared memory.</summary>
    public static class Enums
    {
        public enum ACC_STATUS { ACC_OFF = 0, ACC_REPLAY = 1, ACC_LIVE = 2, ACC_PAUSE = 3 }

        public enum ACC_SESSION_TYPE
        {
            ACC_UNKNOWN = -1, ACC_PRACTICE = 0, ACC_QUALIFY = 1, ACC_RACE = 2, ACC_HOTLAP = 3, ACC_TIME_ATTACK = 4,
            ACC_DRIFT = 5, ACC_DRAG = 6, ACC_HOTSTINT = 7, ACC_HOTLAPSUPERPOLE = 8
        }

        public enum ACC_FLAG_TYPE
        {
            ACC_NO_FLAG = 0, ACC_BLUE_FLAG = 1, ACC_YELLOW_FLAG = 2, ACC_BLACK_FLAG = 3, ACC_WHITE_FLAG = 4,
            ACC_CHECKERED_FLAG = 5, ACC_PENALTY_FLAG = 6, ACC_GREEN_FLAG = 7, ACC_ORANGE_FLAG = 8
        }

        public enum ACC_PENALTY_TYPE
        {
            None = 0,
            DriveThrough_Cutting = 1,
            StopAndGo_10_Cutting = 2,
            StopAndGo_20_Cutting = 3,
            StopAndGo_30_Cutting = 4,
            Disqualified_Cutting = 5,
            RemoveBestLaptime_Cutting = 6,
            DriveThrough_PitSpeeding = 7,
            StopAndGo_10_PitSpeeding = 8,
            StopAndGo_20_PitSpeeding = 9,
            StopAndGo_30_PitSpeeding = 10,
            Disqualified_PitSpeeding = 11,
            RemoveBestLaptime_PitSpeeding = 12,
            Disqualified_IgnoredMandatoryPit = 13,
            PostRaceTime = 14,
            Disqualified_Trolling = 15,
            Disqualified_PitEntry = 16,
            Disqualified_PitExit = 17,
            Disqualified_Wrongway = 18,
            DriveThrough_IgnoredDriverStint = 19,
            Disqualified_IgnoredDriverStint = 20,
            Disqualified_ExceededDriverStintLimit = 21
        }

        public enum ACC_TRACK_GRIP_STATUS { ACC_GREEN = 0, ACC_FAST = 1, ACC_OPTIMUM = 2, ACC_GREASY = 3, ACC_DAMP = 4, ACC_WET = 5, ACC_FLOODED = 6 }

        public enum ACC_RAIN_INTENSITY { ACC_NO_RAIN = 0, ACC_DRIZZLE = 1, ACC_LIGHT_RAIN = 2, ACC_MEDIUM_RAIN = 3, ACC_HEAVY_RAIN = 4, ACC_THUNDERSTORM = 5 }
    }
}
