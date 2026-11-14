using USRTG.Kunos;
using Xunit;

namespace USRTG.Tests
{
    public class MappingTests
    {
        [Theory]
        [InlineData(0, Enums.GameState.Off)]
        [InlineData(1, Enums.GameState.Replay)]
        [InlineData(2, Enums.GameState.Live)]
        [InlineData(3, Enums.GameState.Paused)]
        [InlineData(99, Enums.GameState.Off)]
        public void KunosStatus(int raw, Enums.GameState expected) => Assert.Equal(expected, Mapping.MapKunosStatus(raw));

        [Theory]
        [InlineData(-1, Enums.SessionType.Unknown)]
        [InlineData(2, Enums.SessionType.Race)]
        [InlineData(8, Enums.SessionType.Superpole)]
        [InlineData(42, Enums.SessionType.Unknown)]
        public void KunosSessionType(int raw, Enums.SessionType expected) => Assert.Equal(expected, Mapping.MapKunosSessionType(raw));

        [Theory]
        [InlineData(0, Enums.SessionType.Unknown)]
        [InlineData(1, Enums.SessionType.Practice)]
        [InlineData(4, Enums.SessionType.Practice)]
        [InlineData(5, Enums.SessionType.Qualify)]
        [InlineData(9, Enums.SessionType.Qualify)]
        [InlineData(10, Enums.SessionType.Race)]
        [InlineData(11, Enums.SessionType.Race)]
        [InlineData(12, Enums.SessionType.TimeAttack)]
        public void F1SessionType(int raw, Enums.SessionType expected) => Assert.Equal(expected, Mapping.MapF1SessionType(raw));

        [Theory]
        [InlineData(1, Enums.Flag.Blue)]
        [InlineData(2, Enums.Flag.Yellow)]
        [InlineData(5, Enums.Flag.Checkered)]
        [InlineData(6, Enums.Flag.Black)]
        [InlineData(7, Enums.Flag.NoFlag)] // AC has no value 7
        public void AcFlags(int raw, Enums.Flag expected) => Assert.Equal(expected, Mapping.MapAcFlag(raw));

        [Theory]
        [InlineData(7, Enums.Flag.Green)]
        [InlineData(8, Enums.Flag.OrangeBlack)]
        public void AccFlags(int raw, Enums.Flag expected) => Assert.Equal(expected, Mapping.MapAccFlag(raw));

        [Theory]
        [InlineData(0, 0, Enums.Flag.NoFlag)]
        [InlineData(-1, 0, Enums.Flag.NoFlag)]
        [InlineData(1, 0, Enums.Flag.Green)]
        [InlineData(2, 0, Enums.Flag.Blue)]
        [InlineData(4, 0, Enums.Flag.Red)]
        [InlineData(3, 1, Enums.Flag.SafetyCar)]
        [InlineData(0, 2, Enums.Flag.VirtualSafetyCar)]
        [InlineData(2, 3, Enums.Flag.Blue)] // formation lap is not a flag
        public void F1Flags(int vehicleFlag, int safetyCar, Enums.Flag expected) => Assert.Equal(expected, Mapping.MapF1Flag(vehicleFlag, safetyCar));

        [Fact]
        public void KunosGear_IsShiftedToUnifiedConvention()
        {
            Assert.Equal(-1, Mapping.MapKunosGear(0));
            Assert.Equal(0, Mapping.MapKunosGear(1));
            Assert.Equal(1, Mapping.MapKunosGear(2));
        }

        [Fact]
        public void GameDetector_SharedMemoryVersionFallback()
        {
            Assert.Equal(Enums.Game.AssettoCorsa, GameDetector.DetectBySharedMemoryVersion("1.7"));
            Assert.Equal(Enums.Game.AssettoCorsaCompetizione, GameDetector.DetectBySharedMemoryVersion("1.8"));
            Assert.Equal(Enums.Game.AssettoCorsaCompetizione, GameDetector.DetectBySharedMemoryVersion("1.10"));
            Assert.Equal(Enums.Game.Unknown, GameDetector.DetectBySharedMemoryVersion(""));
            Assert.Equal(Enums.Game.Unknown, GameDetector.DetectBySharedMemoryVersion(null));
        }

        [Fact]
        public void LevelMapping_LearnsLevelsInSortedOrder()
        {
            var m = new LevelMapping();
            Assert.Equal(0, m.Update("car", 0f));          // off
            Assert.Equal(1, m.Update("car", 0.30f));       // first value seen
            Assert.Equal(2, m.Update("car", 0.50f));
            Assert.Equal(1, m.Update("car", 0.10f));       // lower value becomes level 1 ...
            Assert.Equal(3, m.GetLevel("car", 0.50f));     // ... and shifts the others
            Assert.Equal(2, m.GetLevel("car", 0.3001f));   // within epsilon
            Assert.Equal(-1, m.GetLevel("car", 0.9f));     // unseen
            Assert.Equal(-1, m.GetLevel("other", 0.3f));   // unseen car
            Assert.Equal(0, m.Update(null, 0f));           // null car model must not throw
        }

        [Fact]
        public void LevelMapping_IsBounded()
        {
            var m = new LevelMapping();
            for (int i = 1; i <= 100; i++)
            {
                m.Update("car", i * 0.01f);
            }
            Assert.Equal(-1, m.GetLevel("car", 0.99f));
            Assert.Equal(32, m.GetLevel("car", 0.32f));
        }
    }
}
