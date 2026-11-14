using Xunit;

namespace USRTG.Tests
{
    /// <summary>Checks the translation from the raw shared memory structs into the packet.</summary>
    public class KunosBuildTests
    {
        [Fact]
        public void AssettoCorsa_Build_NormalisesValues()
        {
            var p = new AC.Structs.SPageFilePhysics
            {
                packetId = 10, gas = 0.5f, brake = 0.2f, steerAngle = -0.1f, clutch = 1f, speedKmh = 100f, gear = 3, rpms = 5000,
                fuel = 20f, accG = [0.5f, 1.0f, -0.8f], tc = 0.3f, abs = 0.0f, heading = 1f, pitch = 0f, roll = 0f,
                airTemp = 25f, roadTemp = 35f, pitLimiterOn = 1,
                tyreCoreTemperature = [70, 71, 72, 73], wheelsPressure = [26, 26, 26, 26], brakeTemp = [300, 300, 300, 300],
                wheelSlip = [0, 0, 0, 0], tyreWear = [99, 99, 99, 99], tyreTempI = [1, 2, 3, 4], tyreTempM = [5, 6, 7, 8],
                tyreTempO = [9, 10, 11, 12], velocity = [1, 2, 3], localAngularVel = [0, 0, 0], carDamage = [0, 0, 0, 0, 0],
                tyreDirtyLevel = [0, 0, 0, 0], wheelLoad = [0, 0, 0, 0], wheelAngularSpeed = [0, 0, 0, 0],
                suspensionTravel = [0, 0, 0, 0], rideHeight = [0, 0]
            };
            var g = new AC.Structs.SPageFileGraphic
            {
                status = 2, session = 1, flag = 1, sessionTimeLeft = 60000f, normalizedCarPosition = 0.25f, completedLaps = 2,
                position = 3, iCurrentTime = 1000, iLastTime = 2000, iBestTime = 1500, isInPit = 0, isInPitLane = 1,
                currentSectorIndex = 2, lastSectorTime = 500, tyreCompound = "Soft", carCoordinates = [0, 0, 0], numberOfLaps = 10
            };
            var s = new AC.Structs.SPageFileStatic
            {
                carModel = "ks_ferrari_488_gt3", track = "monza", playerName = "Max", playerSurname = "Mustermann",
                maxRpm = 8000, maxFuel = 80f, numCars = 16, sectorCount = 3
            };

            var packet = new AC.Handle().Build(p, g, s);

            Assert.Equal(Enums.Game.AssettoCorsa, packet.Game);
            Assert.Equal(2, packet.Gear);                       // 3 -> 2nd gear
            Assert.Equal(new[] { 0.5f, -0.8f, 1.0f }, packet.AccG); // [x, y, z] -> [lat, lon, vert]
            Assert.Equal(25f, packet.FuelPercent);
            Assert.Equal(1, packet.Tc);                         // first discovered level
            Assert.Equal(0, packet.Abs);
            Assert.Equal(Enums.GameState.Live, packet.GameState);
            Assert.Equal(Enums.SessionType.Qualify, packet.SessionType);
            Assert.Equal(Enums.Flag.Blue, packet.Flag);
            Assert.Equal(0.25f, packet.NormalizedCarPosition);  // was truncated to int before
            Assert.True(packet.IsInPitLane);
            Assert.False(packet.IsInPit);
            Assert.True(packet.PitLimiterOn);
            Assert.Equal("Max Mustermann", packet.PlayerName);
            Assert.Equal("Soft", packet.TyreCompound);
            Assert.Equal(new[] { 1f, 2f, 3f, 4f }, packet.TyreTempInner);
            Assert.Equal(10, packet.TotalLaps);
            Assert.Null(packet.TrackGripStatus);                // ACC only
        }

        [Fact]
        public void AssettoCorsaCompetizione_Build_UsesGraphicsAidLevels()
        {
            var p = new ACC.Structs.SPageFilePhysics
            {
                packetId = 7, gear = 1, accG = [0, 0, 0], tc = 0.123f, abs = 0.456f, fuel = 30f,
                tyreCoreTemperature = [80, 80, 80, 80], wheelsPressure = [27, 27, 27, 27], brakeTemp = [500, 500, 500, 500],
                wheelSlip = [0, 0, 0, 0], velocity = [0, 0, 0], localAngularVel = [0, 0, 0], carDamage = [0, 0, 0, 0, 0],
                wheelAngularSpeed = [0, 0, 0, 0], slipRatio = [0, 0, 0, 0], slipAngle = [0, 0, 0, 0], suspensionTravel = [0, 0, 0, 0],
                brakePressure = [0, 0, 0, 0], padLife = [0, 0, 0, 0], discLife = [0, 0, 0, 0], waterTemp = 90f, isEngineRunning = 1
            };
            var g = new ACC.Structs.SPageFileGraphic
            {
                status = 3, session = 7, flag = 8, TC = 5, ABS = 4, TCCut = 2, EngineMap = 3, activeCars = 2,
                carID = new int[60], carCoordinates = new float[180], playerCarID = 1000, tyreCompound = "dry_compound",
                trackGripStatus = 2, rainIntensity = 1, isValidLap = 1, iDeltaLapTime = -250, gapAhead = 1200, gapBehind = 800
            };
            g.carID[1] = 1000;
            g.carCoordinates[3] = 1f; g.carCoordinates[4] = 2f; g.carCoordinates[5] = 3f;
            var s = new ACC.Structs.SPageFileStatic { carModel = "porsche_992_gt3_r", track = "nurburgring", maxRpm = 9000, maxFuel = 120f, numCars = 30, sectorCount = 3 };

            var packet = ACC.Handle.Build(p, g, s);

            Assert.Equal(Enums.Game.AssettoCorsaCompetizione, packet.Game);
            Assert.Equal(0, packet.Gear);                       // neutral
            Assert.Equal(5, packet.Tc);                         // from graphics page, not inferred
            Assert.Equal(4, packet.Abs);
            Assert.Equal(2, packet.TcCut);
            Assert.Equal(Enums.GameState.Paused, packet.GameState);
            Assert.Equal(Enums.SessionType.Hotstint, packet.SessionType);
            Assert.Equal(Enums.Flag.OrangeBlack, packet.Flag);
            Assert.Equal(2, packet.NumCars);                    // activeCars preferred over static numCars
            Assert.Equal(new[] { 1f, 2f, 3f }, packet.CarCoordinates);
            Assert.Equal(2, packet.TrackGripStatus);
            Assert.True(packet.IsLapValid);
            Assert.Equal(-250, packet.DeltaLapTime);
            Assert.Equal(1200, packet.GapAhead);
            Assert.Equal(90f, packet.WaterTemp);
            Assert.True(packet.IsEngineRunning);
            Assert.Null(packet.TyreWear);                       // not available in ACC
            Assert.Null(packet.Drs);
        }
    }
}
