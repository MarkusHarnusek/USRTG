using USRTG.F1_2020;
using Xunit;

namespace USRTG.Tests
{
    public class F1ParserTests
    {
        [Fact]
        public void Header_IsDecoded()
        {
            Assert.True(Parser.TryParseHeader(F1PacketBuilder.CarTelemetry(), out var header));
            Assert.Equal(Spec.PacketFormat, header.PacketFormat);
            Assert.Equal(Spec.CarTelemetryId, header.PacketId);
            Assert.Equal(777u, header.FrameIdentifier);
            Assert.Equal(F1PacketBuilder.Player, header.PlayerCarIndex);
            Assert.Equal(Spec.NoSecondaryPlayer, header.SecondaryPlayerCarIndex);
        }

        [Fact]
        public void TruncatedPackets_AreRejectedWithoutThrowing()
        {
            Assert.False(Parser.TryParseHeader(new byte[10], out _));
            Assert.Null(Parser.ParseSession(new byte[Spec.SessionSize - 1]));
            Assert.Null(Parser.ParseLapData(new byte[Spec.LapDataSize - 1], 0));
            Assert.Null(Parser.ParseCarTelemetry(new byte[Spec.CarTelemetrySize - 1], 0));
            Assert.Null(Parser.ParseCarStatus(new byte[Spec.CarStatusSize - 1], 0));
            Assert.Null(Parser.ParseMotion(new byte[Spec.MotionSize - 1], 0));
            Assert.Null(Parser.ParseParticipants(new byte[Spec.ParticipantsSize - 1], 0));
            Assert.Null(Parser.ParseLapData(F1PacketBuilder.LapData(), Spec.MaxCars));
        }

        [Fact]
        public void Session_IsDecoded()
        {
            var s = Parser.ParseSession(F1PacketBuilder.Session(sessionType: 10, trackId: 7, totalLaps: 52, trackLength: 5891, airTemp: 21, trackTemp: 33, timeLeft: 3456, safetyCar: 2));
            Assert.NotNull(s);
            Assert.Equal(10, s.SessionType);
            Assert.Equal(7, s.TrackId);
            Assert.Equal(52, s.TotalLaps);
            Assert.Equal(5891, s.TrackLength);
            Assert.Equal(21, s.AirTemperature);
            Assert.Equal(33, s.TrackTemperature);
            Assert.Equal(3456, s.SessionTimeLeft);
            Assert.Equal(2, s.SafetyCarStatus);
            Assert.True(s.NetworkGame);
            Assert.False(s.GamePaused);
        }

        [Fact]
        public void LapData_IsDecodedForPlayerCar()
        {
            var lap = Parser.ParseLapData(F1PacketBuilder.LapData(), F1PacketBuilder.Player);
            Assert.NotNull(lap);
            Assert.Equal(91.234f, lap.LastLapTime, 3);
            Assert.Equal(30.5f, lap.CurrentLapTime, 3);
            Assert.Equal(28100, lap.Sector1TimeMs);
            Assert.Equal(90.001f, lap.BestLapTime, 3);
            Assert.Equal(1472.75f, lap.LapDistance, 2);
            Assert.Equal(25000f, lap.TotalDistance, 1);
            Assert.Equal(4, lap.CarPosition);
            Assert.Equal(6, lap.CurrentLapNum);
            Assert.Equal(1, lap.Sector);
            Assert.Equal(5, lap.Penalties);
            Assert.Equal(9, lap.GridPosition);
            Assert.Equal(4, lap.DriverStatus);

            // Another car's slot is all zero.
            var other = Parser.ParseLapData(F1PacketBuilder.LapData(), 0);
            Assert.NotNull(other);
            Assert.Equal(0, other.CurrentLapNum);
        }

        [Fact]
        public void CarTelemetry_IsDecodedIncludingTrailingFields()
        {
            var t = Parser.ParseCarTelemetry(F1PacketBuilder.CarTelemetry(), F1PacketBuilder.Player);
            Assert.NotNull(t);
            Assert.Equal(287, t.Speed);
            Assert.Equal(0.75f, t.Throttle);
            Assert.Equal(-0.25f, t.Steer);
            Assert.Equal(0.1f, t.Brake);
            Assert.Equal(50, t.Clutch);
            Assert.Equal(7, t.Gear);
            Assert.Equal(11500, t.EngineRpm);
            Assert.Equal(1, t.Drs);
            Assert.Equal(new ushort[] { 401, 402, 403, 404 }, t.BrakesTemperature);
            Assert.Equal(new byte[] { 101, 102, 103, 104 }, t.TyresInnerTemperature);
            Assert.Equal(105, t.EngineTemperature);
            Assert.Equal(new[] { 21.1f, 21.2f, 21.3f, 21.4f }, t.TyresPressure);
            Assert.Equal(8, t.SuggestedGear);
        }

        [Fact]
        public void CarStatus_IsDecoded()
        {
            var s = Parser.ParseCarStatus(F1PacketBuilder.CarStatus(), F1PacketBuilder.Player);
            Assert.NotNull(s);
            Assert.Equal(2, s.TractionControl);
            Assert.Equal(1, s.AntiLockBrakes);
            Assert.Equal(56, s.FrontBrakeBias);
            Assert.Equal(1, s.PitLimiterStatus);
            Assert.Equal(42.5f, s.FuelInTank);
            Assert.Equal(110f, s.FuelCapacity);
            Assert.Equal(12000, s.MaxRpm);
            Assert.Equal(new byte[] { 11, 12, 13, 14 }, s.TyresWear);
            Assert.Equal(18, s.ActualTyreCompound);
            Assert.Equal(17, s.VisualTyreCompound);
            Assert.Equal(3, s.VehicleFiaFlags);
            Assert.Equal(3_000_000f, s.ErsStoreEnergy);
            Assert.Equal(2, s.ErsDeployMode);
            Assert.Equal(3f, s.ErsDeployedThisLap);
        }

        [Fact]
        public void Motion_IsDecodedIncludingPlayerExtras()
        {
            var m = Parser.ParseMotion(F1PacketBuilder.Motion(), F1PacketBuilder.Player);
            Assert.NotNull(m);
            Assert.Equal(100f, m.WorldPositionX);
            Assert.Equal(1.5f, m.GForceLateral);
            Assert.Equal(-0.8f, m.GForceLongitudinal);
            Assert.Equal(0.5f, m.Yaw);
            Assert.Equal(new[] { 0.1f, 0.2f, 0.3f, 0.4f }, m.WheelSlip);
            Assert.Equal(new[] { 0.01f, 0.02f, 0.03f }, m.AngularVelocity);
            Assert.Equal(0.2f, m.FrontWheelsAngle);
        }

        [Fact]
        public void Participants_IsDecoded()
        {
            var p = Parser.ParseParticipants(F1PacketBuilder.Participants(numActive: 20, teamId: 1, name: "HAMILTON"), F1PacketBuilder.Player);
            Assert.NotNull(p);
            Assert.Equal(20, p.NumActiveCars);
            Assert.Equal(1, p.TeamId);
            Assert.Equal(44, p.RaceNumber);
            Assert.Equal("HAMILTON", p.Name);
            Assert.True(p.YourTelemetry);
        }
    }
}
