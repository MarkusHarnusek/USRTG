using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit;

namespace USRTG.Tests
{
    public class PacketTests
    {
        private static readonly JsonSerializerOptions Json = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        [Fact]
        public void CommonPacket_IsASubsetOfPacket_AndFromCopiesEveryField()
        {
            var full = PopulatedPacket();
            var common = CommonPacket.From(full);

            foreach (var prop in typeof(CommonPacket).GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                var source = typeof(Packet).GetProperty(prop.Name);
                Assert.True(source != null, $"Packet has no property {prop.Name}");
                Assert.True(source!.PropertyType == prop.PropertyType, $"{prop.Name} has a different type");
                Assert.Equal(source.GetValue(full), prop.GetValue(common));
            }
        }

        [Fact]
        public void CommonPacket_ContainsOnlyNonNullableFields()
        {
            var context = new NullabilityInfoContext();
            foreach (var prop in typeof(CommonPacket).GetProperties())
            {
                var info = context.Create(prop);
                Assert.True(info.ReadState == NullabilityState.NotNull, $"{prop.Name} is nullable; common fields must exist in every game");
            }
        }

        [Fact]
        public void CommonPacket_SerialisesWithCamelCaseKeys()
        {
            var json = JsonSerializer.Serialize(CommonPacket.From(PopulatedPacket()), Json);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            Assert.Equal(2, root.GetProperty("game").GetInt32());
            Assert.Equal("AssettoCorsaCompetizione", root.GetProperty("gameName").GetString());
            Assert.Equal(211.5f, root.GetProperty("speedKmh").GetSingle());
            Assert.Equal(4, root.GetProperty("tyreCoreTemp").GetArrayLength());
            Assert.False(root.TryGetProperty("tyreWear", out _)); // not part of the common packet
        }

        [Fact]
        public void FullPacket_OmitsNullGameSpecificFields()
        {
            var json = JsonSerializer.Serialize(new Packet { Game = Enums.Game.F1_2020, Drs = 1f }, Json);
            using var doc = JsonDocument.Parse(json);
            Assert.True(doc.RootElement.TryGetProperty("drs", out _));
            Assert.False(doc.RootElement.TryGetProperty("turboBoost", out _));
        }

        private static Packet PopulatedPacket() => new()
        {
            Game = Enums.Game.AssettoCorsaCompetizione,
            Id = 42,
            Timestamp = 1700000000000,
            Throttle = 0.9f,
            Brake = 0.1f,
            Steer = -0.3f,
            Clutch = 1f,
            SpeedKmh = 211.5f,
            Gear = 4,
            Rpms = 6500,
            MaxRpm = 7500,
            Fuel = 55f,
            MaxFuel = 120f,
            FuelPercent = 45.8f,
            PitLimiterOn = false,
            Tc = 3,
            Abs = 2,
            Heading = 1f,
            Pitch = 0.01f,
            Roll = -0.02f,
            AccG = [1.1f, -0.5f, 1f],
            GameState = Enums.GameState.Live,
            SessionType = Enums.SessionType.Race,
            Flag = Enums.Flag.Blue,
            SessionTimeLeft = 123456f,
            NumCars = 24,
            SectorCount = 3,
            TotalLaps = 0,
            AirTemp = 22f,
            RoadTemp = 31f,
            CarModel = "porsche_991ii_gt3_r",
            Track = "spa",
            PlayerName = "Test Driver",
            NormalizedCarPosition = 0.42f,
            CompletedLaps = 3,
            Position = 7,
            CurrentTime = 61000,
            LastTime = 138000,
            BestTime = 137500,
            DistanceTraveled = 24000f,
            IsInPit = false,
            IsInPitLane = false,
            CurrentSectorIndex = 1,
            LastSectorTime = 45000,
            PenaltyTime = 0f,
            TyreCompound = "dry_compound",
            TyreCoreTemp = [80f, 81f, 82f, 83f],
            TyrePressure = [27.1f, 27.2f, 27.3f, 27.4f],
            BrakeTemp = [400f, 410f, 380f, 390f],
            WheelSlip = [0.1f, 0.1f, 0.2f, 0.2f],
            TyreWear = [0.1f, 0.1f, 0.1f, 0.1f]
        };
    }
}
