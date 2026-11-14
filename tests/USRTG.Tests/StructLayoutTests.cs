using System.Runtime.InteropServices;
using Xunit;

namespace USRTG.Tests
{
    /// <summary>
    /// The shared-memory structs must match the C++ layouts byte for byte. The expected sizes are the
    /// well-known sizes of the game structs; a mismatch means a field was added, removed or reordered.
    /// </summary>
    public class StructLayoutTests
    {
        [Fact]
        public void AssettoCorsa_PageSizes_MatchGame()
        {
            Assert.Equal(580, Marshal.SizeOf<AC.Structs.SPageFilePhysics>());
            Assert.Equal(296, Marshal.SizeOf<AC.Structs.SPageFileGraphic>());
            Assert.Equal(688, Marshal.SizeOf<AC.Structs.SPageFileStatic>());
        }

        [Fact]
        public void AssettoCorsaCompetizione_PageSizes_MatchGame()
        {
            Assert.Equal(800, Marshal.SizeOf<ACC.Structs.SPageFilePhysics>());
            Assert.Equal(1588, Marshal.SizeOf<ACC.Structs.SPageFileGraphic>());
            Assert.Equal(820, Marshal.SizeOf<ACC.Structs.SPageFileStatic>());
        }

        [Fact]
        public void AssettoCorsa_PhysicsFieldOffsets_MatchOfficialDocumentation()
        {
            // Regression for the bug where tyre temperature arrays were inserted mid-struct and shifted everything after them.
            Assert.Equal(200, Marshal.OffsetOf<AC.Structs.SPageFilePhysics>("drs").ToInt32());
            Assert.Equal(204, Marshal.OffsetOf<AC.Structs.SPageFilePhysics>("tc").ToInt32());
            Assert.Equal(252, Marshal.OffsetOf<AC.Structs.SPageFilePhysics>("abs").ToInt32());
            Assert.Equal(288, Marshal.OffsetOf<AC.Structs.SPageFilePhysics>("airTemp").ToInt32());
            Assert.Equal(348, Marshal.OffsetOf<AC.Structs.SPageFilePhysics>("brakeTemp").ToInt32());
            Assert.Equal(368, Marshal.OffsetOf<AC.Structs.SPageFilePhysics>("tyreTempI").ToInt32());
        }

        [Fact]
        public void AccPhysics_IsAnExtensionOfAcPhysics()
        {
            // Every AC field must sit at the same offset in the ACC struct (ACC only appends).
            foreach (var field in typeof(AC.Structs.SPageFilePhysics).GetFields())
            {
                var accOffset = Marshal.OffsetOf<ACC.Structs.SPageFilePhysics>(field.Name);
                var acOffset = Marshal.OffsetOf<AC.Structs.SPageFilePhysics>(field.Name);
                Assert.True(acOffset == accOffset, $"{field.Name}: AC {acOffset} vs ACC {accOffset}");
            }
        }

        [Fact]
        public void AccGraphics_PlayerFieldsSitAfterMultiCarArrays()
        {
            Assert.Equal(252, Marshal.OffsetOf<ACC.Structs.SPageFileGraphic>("activeCars").ToInt32());
            Assert.Equal(1268, Marshal.OffsetOf<ACC.Structs.SPageFileGraphic>("TC").ToInt32());
            Assert.Equal(1280, Marshal.OffsetOf<ACC.Structs.SPageFileGraphic>("ABS").ToInt32());
            Assert.Equal(1556, Marshal.OffsetOf<ACC.Structs.SPageFileGraphic>("trackGripStatus").ToInt32());
        }
    }
}
