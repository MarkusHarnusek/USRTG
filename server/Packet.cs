using System.Runtime.Intrinsics.X86;

namespace USRTG
{
    public class Packet
    {
        public int id { get; }

        #region Input

        public float gas { get;}
        public float brake { get; }
        public float steerAngle { get; }

        #endregion

        #region General

        public float speedKmh { get; }
        public int gear { get; }
        public int rpms { get; }
        public float turboBoost { get; }
        public float ballast { get; }

        #endregion

        #region Aids

        public float drs { get; }
        public float tc { get; }
        public float abs { get; }
        public float kersCharge { get; }
        public float kersInput { get; }

        #endregion

        #region Car

        public float heading { get; }
        public float pitch { get; }
        public float roll { get; }
        public float fuel { get; }
        public float maxFuel { get; }
        public float maxRpm { get; }

        #endregion

        #region Session

        public int gameState { get; }
        public int sessionType { get; }
        public int flag { get; }
        public float sessionTimeLeft { get; }
        public int numCars { get; }
        public int sectorCount { get; }

        #endregion

        #region Environment

        public float airTemp { get; }
        public float roadTemp { get; }
        public float surfaceGrip { get; }

        #endregion

        #region Driver

        public int normalizedCarPosition { get; }
        public int completedLaps { get; }
        public int position { get; }
        public int currentTime { get; }
        public int lastTime { get; }
        public int bestTime { get; }
        public float distanceTraveled { get; }
        public int isInPit { get; }
        public int isInPitLane { get; }
        public int currentSectorIndex { get; }
        public int lastSectorTime { get; }
        public string? tyreCompound { get; }

        #endregion

        #region Tyres 

        public int[] tyreWear { get; } = [0, 0, 0, 0];

        #endregion

        /// <summary>
        /// Empty packet
        /// </summary>
        public Packet()
        {
        }

        /// <summary>
        /// Assetto corsa telemetry packet
        /// </summary>
        public Packet(int id, float gas, float brake, float steerAngle, float speedKmh, int gear, int rpms, float fuel, float maxFuel, float maxRpm, float turboBoost, float ballast, float drs, float tc, float abs, float kersCharge, float kersInput, float heading, float pitch, float roll, int gameState, int sessionType, int flag, float sessionTimeLeft, int numCars, int sectorCount, float airTemp, float roadTemp, float surfaceGrip, int normalizedCarPosition, int completedLaps, int position, int currentTime, int lastTime, int bestTime, float distanceTraveled, int isInPit, int isInPitLane, int currentSectorIndex, int lastSectorTime, string? tyreCompound)
        {
            this.id = id;
            this.gas = gas;
            this.brake = brake;
            this.steerAngle = steerAngle;
            this.speedKmh = speedKmh;
            this.gear = gear;
            this.rpms = rpms;
            this.fuel = fuel;
            this.maxFuel = maxFuel;
            this.maxRpm = maxRpm;
            this.turboBoost = turboBoost;
            this.ballast = ballast;
            this.drs = drs;
            this.tc = tc;
            this.abs = abs;
            this.kersCharge = kersCharge;
            this.kersInput = kersInput;
            this.heading = heading;
            this.pitch = pitch;
            this.roll = roll;
            this.gameState = gameState;
            this.sessionType = sessionType;
            this.flag = flag;
            this.sessionTimeLeft = sessionTimeLeft;
            this.numCars = numCars;
            this.sectorCount = sectorCount;
            this.airTemp = airTemp;
            this.roadTemp = roadTemp;
            this.surfaceGrip = surfaceGrip;
            this.normalizedCarPosition = normalizedCarPosition;
            this.completedLaps = completedLaps;
            this.position = position;
            this.currentTime = currentTime;
            this.lastTime = lastTime;
            this.bestTime = bestTime;
            this.distanceTraveled = distanceTraveled;
            this.isInPit = isInPit;
            this.isInPitLane = isInPitLane;
            this.currentSectorIndex = currentSectorIndex;
            this.lastSectorTime = lastSectorTime;
            this.tyreCompound = tyreCompound;
            this.maxRpm = maxRpm;
        }
    }
}
