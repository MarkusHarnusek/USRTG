namespace USRTG.F1_2020
{
    internal readonly record struct Header(
        ushort PacketFormat, byte GameMajorVersion, byte GameMinorVersion, byte PacketVersion, byte PacketId,
        ulong SessionUid, float SessionTime, uint FrameIdentifier, byte PlayerCarIndex, byte SecondaryPlayerCarIndex);

    internal sealed record SessionData(
        byte Weather, sbyte TrackTemperature, sbyte AirTemperature, byte TotalLaps, ushort TrackLength,
        byte SessionType, sbyte TrackId, byte Formula, ushort SessionTimeLeft, ushort SessionDuration,
        byte PitSpeedLimit, bool GamePaused, bool IsSpectating, byte SpectatorCarIndex, byte SafetyCarStatus, bool NetworkGame);

    internal sealed record LapData(
        float LastLapTime, float CurrentLapTime, ushort Sector1TimeMs, ushort Sector2TimeMs, float BestLapTime, byte BestLapNum,
        float LapDistance, float TotalDistance, float SafetyCarDelta, byte CarPosition, byte CurrentLapNum, byte PitStatus,
        byte Sector, bool CurrentLapInvalid, byte Penalties, byte GridPosition, byte DriverStatus, byte ResultStatus);

    internal sealed record CarTelemetryData(
        ushort Speed, float Throttle, float Steer, float Brake, byte Clutch, sbyte Gear, ushort EngineRpm, byte Drs, byte RevLightsPercent,
        ushort[] BrakesTemperature, byte[] TyresSurfaceTemperature, byte[] TyresInnerTemperature, ushort EngineTemperature,
        float[] TyresPressure, byte[] SurfaceType, sbyte SuggestedGear);

    internal sealed record CarStatusData(
        byte TractionControl, byte AntiLockBrakes, byte FuelMix, byte FrontBrakeBias, byte PitLimiterStatus, float FuelInTank,
        float FuelCapacity, float FuelRemainingLaps, ushort MaxRpm, ushort IdleRpm, byte MaxGears, byte DrsAllowed,
        ushort DrsActivationDistance, byte[] TyresWear, byte ActualTyreCompound, byte VisualTyreCompound, byte TyresAgeLaps,
        byte[] TyresDamage, byte FrontLeftWingDamage, byte FrontRightWingDamage, byte RearWingDamage, byte DrsFault,
        byte EngineDamage, byte GearBoxDamage, sbyte VehicleFiaFlags, float ErsStoreEnergy, byte ErsDeployMode,
        float ErsHarvestedThisLapMguk, float ErsHarvestedThisLapMguh, float ErsDeployedThisLap);

    internal sealed record MotionData(
        float WorldPositionX, float WorldPositionY, float WorldPositionZ,
        float WorldVelocityX, float WorldVelocityY, float WorldVelocityZ,
        float GForceLateral, float GForceLongitudinal, float GForceVertical,
        float Yaw, float Pitch, float Roll,
        float[] WheelSlip, float[] LocalVelocity, float[] AngularVelocity, float FrontWheelsAngle);

    internal sealed record ParticipantsData(
        byte NumActiveCars, bool AiControlled, byte DriverId, byte TeamId, byte RaceNumber, byte Nationality, string Name, bool YourTelemetry);

    /// <summary>
    /// Decodes F1 2020 UDP packets. Only the player's car entry is decoded from the per-car arrays.
    /// Every method validates the packet length first, so a truncated datagram can never throw.
    /// </summary>
    internal static class Parser
    {
        public static bool TryParseHeader(ReadOnlySpan<byte> data, out Header header)
        {
            if (data.Length < Spec.HeaderSize)
            {
                header = default;
                return false;
            }

            var r = new SpanReader(data);
            header = new Header(
                PacketFormat: r.U16(),
                GameMajorVersion: r.U8(),
                GameMinorVersion: r.U8(),
                PacketVersion: r.U8(),
                PacketId: r.U8(),
                SessionUid: r.U64(),
                SessionTime: r.F32(),
                FrameIdentifier: r.U32(),
                PlayerCarIndex: r.U8(),
                SecondaryPlayerCarIndex: r.U8());
            return true;
        }

        public static SessionData? ParseSession(ReadOnlySpan<byte> data)
        {
            if (data.Length < Spec.SessionSize)
            {
                return null;
            }

            var r = new SpanReader(data, Spec.HeaderSize);
            var weather = r.U8();
            var trackTemperature = r.I8();
            var airTemperature = r.I8();
            var totalLaps = r.U8();
            var trackLength = r.U16();
            var sessionType = r.U8();
            var trackId = r.I8();
            var formula = r.U8();
            var sessionTimeLeft = r.U16();
            var sessionDuration = r.U16();
            var pitSpeedLimit = r.U8();
            var gamePaused = r.U8() != 0;
            var isSpectating = r.U8() != 0;
            var spectatorCarIndex = r.U8();
            r.Skip(1); // sliProNativeSupport
            r.Skip(1); // numMarshalZones
            r.Skip(Spec.MarshalZoneCount * Spec.MarshalZoneSize);
            var safetyCarStatus = r.U8();
            var networkGame = r.U8() != 0;
            // numWeatherForecastSamples + samples follow; not needed.

            return new SessionData(weather, trackTemperature, airTemperature, totalLaps, trackLength, sessionType, trackId, formula,
                sessionTimeLeft, sessionDuration, pitSpeedLimit, gamePaused, isSpectating, spectatorCarIndex, safetyCarStatus, networkGame);
        }

        public static LapData? ParseLapData(ReadOnlySpan<byte> data, int carIndex)
        {
            if (data.Length < Spec.LapDataSize || !ValidCar(carIndex))
            {
                return null;
            }

            var r = new SpanReader(data, Spec.HeaderSize + carIndex * Spec.LapDataEntrySize);
            var lastLapTime = r.F32();
            var currentLapTime = r.F32();
            var sector1 = r.U16();
            var sector2 = r.U16();
            var bestLapTime = r.F32();
            var bestLapNum = r.U8();
            r.Skip(2 + 2 + 2);          // best lap sector 1..3
            r.Skip(2 + 1 + 2 + 1 + 2 + 1); // best overall sector 1..3 + lap numbers
            var lapDistance = r.F32();
            var totalDistance = r.F32();
            var safetyCarDelta = r.F32();
            var carPosition = r.U8();
            var currentLapNum = r.U8();
            var pitStatus = r.U8();
            var sector = r.U8();
            var currentLapInvalid = r.U8() != 0;
            var penalties = r.U8();
            var gridPosition = r.U8();
            var driverStatus = r.U8();
            var resultStatus = r.U8();

            return new LapData(lastLapTime, currentLapTime, sector1, sector2, bestLapTime, bestLapNum, lapDistance, totalDistance,
                safetyCarDelta, carPosition, currentLapNum, pitStatus, sector, currentLapInvalid, penalties, gridPosition, driverStatus, resultStatus);
        }

        public static CarTelemetryData? ParseCarTelemetry(ReadOnlySpan<byte> data, int carIndex)
        {
            if (data.Length < Spec.CarTelemetrySize || !ValidCar(carIndex))
            {
                return null;
            }

            var r = new SpanReader(data, Spec.HeaderSize + carIndex * Spec.CarTelemetryDataSize);
            var speed = r.U16();
            var throttle = r.F32();
            var steer = r.F32();
            var brake = r.F32();
            var clutch = r.U8();
            var gear = r.I8();
            var rpm = r.U16();
            var drs = r.U8();
            var revLights = r.U8();
            var brakesTemperature = r.U16Array(4);
            var tyresSurface = r.U8Array(4);
            var tyresInner = r.U8Array(4);
            var engineTemperature = r.U16();
            var tyresPressure = r.F32Array(4);
            var surfaceType = r.U8Array(4);

            // Trailing fields after the 22 car entries.
            var t = new SpanReader(data, Spec.HeaderSize + Spec.MaxCars * Spec.CarTelemetryDataSize);
            t.Skip(4 + 1 + 1); // buttonStatus, mfdPanelIndex, mfdPanelIndexSecondaryPlayer
            var suggestedGear = t.I8();

            return new CarTelemetryData(speed, throttle, steer, brake, clutch, gear, rpm, drs, revLights, brakesTemperature,
                tyresSurface, tyresInner, engineTemperature, tyresPressure, surfaceType, suggestedGear);
        }

        public static CarStatusData? ParseCarStatus(ReadOnlySpan<byte> data, int carIndex)
        {
            if (data.Length < Spec.CarStatusSize || !ValidCar(carIndex))
            {
                return null;
            }

            var r = new SpanReader(data, Spec.HeaderSize + carIndex * Spec.CarStatusDataSize);
            return new CarStatusData(
                TractionControl: r.U8(),
                AntiLockBrakes: r.U8(),
                FuelMix: r.U8(),
                FrontBrakeBias: r.U8(),
                PitLimiterStatus: r.U8(),
                FuelInTank: r.F32(),
                FuelCapacity: r.F32(),
                FuelRemainingLaps: r.F32(),
                MaxRpm: r.U16(),
                IdleRpm: r.U16(),
                MaxGears: r.U8(),
                DrsAllowed: r.U8(),
                DrsActivationDistance: r.U16(),
                TyresWear: r.U8Array(4),
                ActualTyreCompound: r.U8(),
                VisualTyreCompound: r.U8(),
                TyresAgeLaps: r.U8(),
                TyresDamage: r.U8Array(4),
                FrontLeftWingDamage: r.U8(),
                FrontRightWingDamage: r.U8(),
                RearWingDamage: r.U8(),
                DrsFault: r.U8(),
                EngineDamage: r.U8(),
                GearBoxDamage: r.U8(),
                VehicleFiaFlags: r.I8(),
                ErsStoreEnergy: r.F32(),
                ErsDeployMode: r.U8(),
                ErsHarvestedThisLapMguk: r.F32(),
                ErsHarvestedThisLapMguh: r.F32(),
                ErsDeployedThisLap: r.F32());
        }

        public static MotionData? ParseMotion(ReadOnlySpan<byte> data, int carIndex)
        {
            if (data.Length < Spec.MotionSize || !ValidCar(carIndex))
            {
                return null;
            }

            var r = new SpanReader(data, Spec.HeaderSize + carIndex * Spec.CarMotionDataSize);
            var px = r.F32();
            var py = r.F32();
            var pz = r.F32();
            var vx = r.F32();
            var vy = r.F32();
            var vz = r.F32();
            r.Skip(6 * 2); // forward / right direction vectors (int16)
            var gLat = r.F32();
            var gLon = r.F32();
            var gVert = r.F32();
            var yaw = r.F32();
            var pitch = r.F32();
            var roll = r.F32();

            // Player-car-only extra data after the 22 entries.
            var e = new SpanReader(data, Spec.HeaderSize + Spec.MaxCars * Spec.CarMotionDataSize);
            e.Skip(3 * 4 * 4); // suspension position / velocity / acceleration
            e.Skip(4 * 4);     // wheel speed
            var wheelSlip = e.F32Array(4);
            var localVelocity = e.F32Array(3);
            var angularVelocity = e.F32Array(3);
            e.Skip(3 * 4);     // angular acceleration
            var frontWheelsAngle = e.F32();

            return new MotionData(px, py, pz, vx, vy, vz, gLat, gLon, gVert, yaw, pitch, roll, wheelSlip, localVelocity, angularVelocity, frontWheelsAngle);
        }

        public static ParticipantsData? ParseParticipants(ReadOnlySpan<byte> data, int carIndex)
        {
            if (data.Length < Spec.ParticipantsSize || !ValidCar(carIndex))
            {
                return null;
            }

            var r = new SpanReader(data, Spec.HeaderSize);
            var numActiveCars = r.U8();
            r.Skip(carIndex * Spec.ParticipantDataSize);
            var aiControlled = r.U8() != 0;
            var driverId = r.U8();
            var teamId = r.U8();
            var raceNumber = r.U8();
            var nationality = r.U8();
            var name = r.FixedString(Spec.ParticipantNameLength);
            var yourTelemetry = r.U8() != 0;

            return new ParticipantsData(numActiveCars, aiControlled, driverId, teamId, raceNumber, nationality, name, yourTelemetry);
        }

        private static bool ValidCar(int carIndex) => carIndex >= 0 && carIndex < Spec.MaxCars;
    }
}
