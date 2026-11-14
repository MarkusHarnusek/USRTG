using USRTG.F1_2020;
using Xunit;

namespace USRTG.Tests
{
    public class F1HandleTests
    {
        private static Handle FedHandle()
        {
            var h = new Handle();
            h.Process(F1PacketBuilder.Session());
            h.Process(F1PacketBuilder.LapData());
            h.Process(F1PacketBuilder.CarStatus());
            h.Process(F1PacketBuilder.Motion());
            h.Process(F1PacketBuilder.Participants());
            h.Process(F1PacketBuilder.CarTelemetry()); // last: its frame id (777) becomes the packet id
            return h;
        }

        [Fact]
        public void NoData_YieldsNoPacket()
        {
            using var h = new Handle();
            Assert.Null(h.TryRead());
        }

        [Fact]
        public void IncompleteData_YieldsNoPacket()
        {
            using var h = new Handle();
            h.Process(F1PacketBuilder.CarTelemetry());
            Assert.Null(h.TryRead());
        }

        [Fact]
        public void WrongPacketFormat_IsIgnored()
        {
            using var h = new Handle();
            var foreign = new F1PacketBuilder(Spec.SessionSize, Spec.SessionId, 0, format: 2021).Bytes;
            h.Process(foreign);
            h.Process(F1PacketBuilder.LapData());
            h.Process(F1PacketBuilder.CarTelemetry());
            h.Process(F1PacketBuilder.CarStatus());
            Assert.Null(h.TryRead()); // session packet was not accepted
        }

        [Fact]
        public void Packet_IsBuiltWithNormalisedValues()
        {
            using var h = FedHandle();
            var p = h.TryRead();
            Assert.NotNull(p);

            Assert.Equal(Enums.Game.F1_2020, p.Game);
            Assert.Equal(777, p.Id);
            Assert.Equal(0.75f, p.Throttle);
            Assert.Equal(0.1f, p.Brake);
            Assert.Equal(-0.25f, p.Steer);
            Assert.Equal(0.5f, p.Clutch);               // 50 % -> 0.5
            Assert.Equal(287f, p.SpeedKmh);
            Assert.Equal(7, p.Gear);
            Assert.Equal(11500, p.Rpms);
            Assert.Equal(12000, p.MaxRpm);
            Assert.Equal(42.5f, p.Fuel);
            Assert.Equal(110f, p.MaxFuel);
            Assert.True(p.PitLimiterOn);
            Assert.Equal(2, p.Tc);
            Assert.Equal(1, p.Abs);
            Assert.Equal(1f, p.Drs);

            Assert.Equal(0.5f, p.Heading);
            Assert.Equal(new[] { 1.5f, -0.8f, 1.0f }, p.AccG);

            Assert.Equal(Enums.GameState.Live, p.GameState);
            Assert.Equal(Enums.SessionType.Race, p.SessionType);
            Assert.Equal(Enums.Flag.Yellow, p.Flag);
            Assert.Equal(3_456_000f, p.SessionTimeLeft);  // seconds -> ms
            Assert.Equal(20, p.NumCars);
            Assert.Equal(3, p.SectorCount);
            Assert.Equal(52, p.TotalLaps);
            Assert.Equal(21f, p.AirTemp);
            Assert.Equal(33f, p.RoadTemp);

            Assert.Equal("Ferrari", p.CarModel);
            Assert.Equal("Silverstone", p.Track);
            Assert.Equal("HAMILTON", p.PlayerName);

            Assert.Equal(1472.75f / 5891f, p.NormalizedCarPosition, 4);
            Assert.Equal(5, p.CompletedLaps);              // lap 6 in progress
            Assert.Equal(4, p.Position);
            Assert.Equal(30500, p.CurrentTime);
            Assert.Equal(91234, p.LastTime);
            Assert.Equal(90001, p.BestTime);
            Assert.False(p.IsInPit);
            Assert.False(p.IsInPitLane);
            Assert.Equal(1, p.CurrentSectorIndex);
            Assert.Equal(28100, p.LastSectorTime);         // in S2, last completed sector is S1
            Assert.Equal(5f, p.PenaltyTime);

            Assert.Equal("C3", p.TyreCompound);
            Assert.Equal("Medium", p.VisualTyreCompound);
            // Wheel order normalised from RL, RR, FL, FR to FL, FR, RL, RR.
            Assert.Equal(new[] { 103f, 104f, 101f, 102f }, p.TyreCoreTemp);
            Assert.Equal(new[] { 403f, 404f, 401f, 402f }, p.BrakeTemp);
            Assert.Equal(new[] { 21.3f, 21.4f, 21.1f, 21.2f }, p.TyrePressure);
            Assert.Equal(new[] { 0.3f, 0.4f, 0.1f, 0.2f }, p.WheelSlip);
            Assert.Equal(new[] { 13f, 14f, 11f, 12f }, p.TyreWear);
        }

        [Fact]
        public void PitStatus_MapsToPitFlags()
        {
            using var h = FedHandle();
            h.Process(F1PacketBuilder.LapData(pitStatus: 1));
            var p = h.TryRead()!;
            Assert.True(p.IsInPitLane);
            Assert.False(p.IsInPit);

            h.Process(F1PacketBuilder.LapData(pitStatus: 2));
            p = h.TryRead()!;
            Assert.True(p.IsInPitLane);
            Assert.True(p.IsInPit);
        }

        [Fact]
        public void SafetyCar_OverridesVehicleFlag()
        {
            using var h = FedHandle();
            h.Process(F1PacketBuilder.Session(safetyCar: 2));
            Assert.Equal(Enums.Flag.VirtualSafetyCar, h.TryRead()!.Flag);
            h.Process(F1PacketBuilder.Session(paused: true));
            Assert.Equal(Enums.GameState.Paused, h.TryRead()!.GameState);
        }

        [Fact]
        public void ThirdSectorTime_IsDerivedAtLapChange()
        {
            using var h = FedHandle();
            // Lap 6: S1 = 28.1 s, S2 = 30.0 s, then lap 7 starts with last lap = 91.234 s -> S3 = 33.134 s.
            h.Process(F1PacketBuilder.LapData(lapNum: 6, sector: 2, s1: 28100, s2: 30000));
            Assert.Equal(30000, h.TryRead()!.LastSectorTime);
            h.Process(F1PacketBuilder.LapData(lapNum: 7, sector: 0, s1: 0, s2: 0, lastLap: 91.234f));
            Assert.Equal(33134, h.TryRead()!.LastSectorTime);
        }
    }
}
