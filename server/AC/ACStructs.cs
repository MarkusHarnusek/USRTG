using System.Runtime.InteropServices;

namespace USRTG.AC
{
    public class ACStructs
    {
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
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public float[] camberRad;
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
            public int iCurrentTime; // ms
            public int iLastTime;   // ms
            public int iBestTime;   // ms
            public float sessionTimeLeft; // seconds
            public float distanceTraveled;
            public int isInPit; // Should be int (0 or 1)
            public int currentSectorIndex;
            public int lastSectorTime; // ms
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
            public int mandatoryPitDone; // Added
            public float windSpeed; // Added
            public float windDirection; // Added
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
            // Removed deprecated airTemp and roadTemp
            public int penaltiesEnabled; // Changed to int
            public float aidFuelRate;
            public float aidTireRate;
            public float aidMechanicalDamage;
            public int aidAllowTyreBlankets; // Added
            public float aidStability; // Added
            public int aidAutoClutch; // Added
            public int aidAutoBlip; // Added
            public int hasDRS; // Added
            public int hasERS; // Added
            public int hasKERS; // Added
            public float kersMaxJ; // Added
            public int engineBrakeSettingsCount; // Added
            public int ersRecoveryLevels; // Added
            public int ersPowerLevels; // Added
            public int ersHeatCharging; // Added
            public int ersIsCharging; // Added
            public float kersCurrentKJ; // Added
            public int drsAvailable; // Added
            public int drsEnabled; // Added
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public float[] tyreTemp; // Added
        }
    }
}