using System.Runtime.InteropServices;

namespace USRTG.AC
{
    /// <summary>
    /// Assetto Corsa shared memory layout (shared memory version 1.7, AC 1.16).
    /// Field order follows the official "Assetto Corsa Shared Memory" documentation exactly;
    /// any reordering shifts every following field.
    /// Wheel arrays are [FL, FR, RL, RR]. Strings are UTF-16.
    /// </summary>
    public static class Structs
    {
        public const string PhysicsMapName = @"Local\acpmf_physics";
        public const string GraphicsMapName = @"Local\acpmf_graphics";
        public const string StaticMapName = @"Local\acpmf_static";

        [StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Unicode)]
        public struct SPageFilePhysics
        {
            public int packetId;
            public float gas;
            public float brake;
            public float fuel;
            public int gear;
            public int rpms;
            public float steerAngle;
            public float speedKmh;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)] public float[] velocity;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)] public float[] accG;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public float[] wheelSlip;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public float[] wheelLoad;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public float[] wheelsPressure;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public float[] wheelAngularSpeed;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public float[] tyreWear;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public float[] tyreDirtyLevel;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public float[] tyreCoreTemperature;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public float[] camberRAD;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public float[] suspensionTravel;
            public float drs;
            public float tc;
            public float heading;
            public float pitch;
            public float roll;
            public float cgHeight;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)] public float[] carDamage;
            public int numberOfTyresOut;
            public int pitLimiterOn;
            public float abs;
            public float kersCharge;
            public float kersInput;
            public int autoShifterOn;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)] public float[] rideHeight;
            public float turboBoost;
            public float ballast;
            public float airDensity;
            public float airTemp;
            public float roadTemp;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)] public float[] localAngularVel;
            public float finalFF;
            public float performanceMeter;
            public int engineBrake;
            public int ersRecoveryLevel;
            public int ersPowerLevel;
            public int ersHeatCharging;
            public int ersIsCharging;
            public float kersCurrentKJ;
            public int drsAvailable;
            public int drsEnabled;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public float[] brakeTemp;
            public float clutch;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public float[] tyreTempI;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public float[] tyreTempM;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public float[] tyreTempO;
            public int isAIControlled;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)] public float[] tyreContactPoint;   // [4][3]
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)] public float[] tyreContactNormal;  // [4][3]
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)] public float[] tyreContactHeading; // [4][3]
            public float brakeBias;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)] public float[] localVelocity;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Unicode)]
        public struct SPageFileGraphic
        {
            public int packetId;
            public int status;
            public int session;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 15)] public string currentTime;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 15)] public string lastTime;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 15)] public string bestTime;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 15)] public string split;
            public int completedLaps;
            public int position;
            public int iCurrentTime;
            public int iLastTime;
            public int iBestTime;
            public float sessionTimeLeft;
            public float distanceTraveled;
            public int isInPit;
            public int currentSectorIndex;
            public int lastSectorTime;
            public int numberOfLaps;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 33)] public string tyreCompound;
            public float replayTimeMultiplier;
            public float normalizedCarPosition;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)] public float[] carCoordinates;
            public float penaltyTime;
            public int flag;
            public int idealLineOn;
            public int isInPitLane;
            public float surfaceGrip;
            public int mandatoryPitDone;
            public float windSpeed;
            public float windDirection;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Unicode)]
        public struct SPageFileStatic
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 15)] public string smVersion;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 15)] public string acVersion;
            public int numberOfSessions;
            public int numCars;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 33)] public string carModel;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 33)] public string track;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 33)] public string playerName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 33)] public string playerSurname;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 33)] public string playerNick;
            public int sectorCount;
            public float maxTorque;
            public float maxPower;
            public int maxRpm;
            public float maxFuel;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public float[] suspensionMaxTravel;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public float[] tyreRadius;
            public float maxTurboBoost;
            public float deprecated_1;  // was airTemp, moved to physics in 1.6
            public float deprecated_2;  // was roadTemp, moved to physics in 1.6
            public int penaltiesEnabled;
            public float aidFuelRate;
            public float aidTireRate;
            public float aidMechanicalDamage;
            public int aidAllowTyreBlankets;
            public float aidStability;
            public int aidAutoClutch;
            public int aidAutoBlip;
            public int hasDRS;
            public int hasERS;
            public int hasKERS;
            public float kersMaxJ;
            public int engineBrakeSettingsCount;
            public int ersPowerControllerCount;
            public float trackSPlineLength;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 33)] public string trackConfiguration;
            public float ersMaxJ;
            public int isTimedRace;
            public int hasExtraLap;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 33)] public string carSkin;
            public int reversedGridPositions;
            public int pitWindowStart;
            public int pitWindowEnd;
            public int isOnline;
        }
    }
}
