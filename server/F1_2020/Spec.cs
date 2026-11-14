namespace USRTG.F1_2020
{
    /// <summary>
    /// Constants of the Codemasters F1 2020 UDP telemetry specification (packet format 2020).
    /// All packets are little endian and packed (no padding).
    /// </summary>
    internal static class Spec
    {
        public const ushort PacketFormat = 2020;
        public const int DefaultPort = 20777;
        public const int HeaderSize = 24;
        public const int MaxCars = 22;
        public const byte NoSecondaryPlayer = 255;

        // Packet ids
        public const byte MotionId = 0;
        public const byte SessionId = 1;
        public const byte LapDataId = 2;
        public const byte EventId = 3;
        public const byte ParticipantsId = 4;
        public const byte CarSetupsId = 5;
        public const byte CarTelemetryId = 6;
        public const byte CarStatusId = 7;
        public const byte FinalClassificationId = 8;
        public const byte LobbyInfoId = 9;

        // Whole packet sizes, used to reject truncated or foreign packets.
        public const int MotionSize = 1464;
        public const int SessionSize = 251;
        public const int LapDataSize = 1190;
        public const int EventSize = 35;
        public const int ParticipantsSize = 1213;
        public const int CarSetupsSize = 1102;
        public const int CarTelemetrySize = 1307;
        public const int CarStatusSize = 1344;
        public const int FinalClassificationSize = 839;
        public const int LobbyInfoSize = 1169;

        // Per-car entry sizes
        public const int CarMotionDataSize = 60;
        public const int LapDataEntrySize = 53;
        public const int CarTelemetryDataSize = 58;
        public const int CarStatusDataSize = 60;
        public const int ParticipantDataSize = 54;

        public const int MarshalZoneCount = 21;
        public const int MarshalZoneSize = 5;
        public const int WeatherForecastSampleCount = 20;
        public const int WeatherForecastSampleSize = 5;
        public const int ParticipantNameLength = 48;
    }
}
