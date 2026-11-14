using System.Runtime.InteropServices;

namespace USRTG.ACC
{
    /// <summary>
    /// Assetto Corsa Competizione shared memory layout (shared memory version 1.9+, ACC 1.9/1.10).
    /// The physics page starts with the same fields as Assetto Corsa and appends ACC specific data;
    /// the graphics page inserts the multi-car arrays after normalizedCarPosition and is therefore
    /// NOT compatible with the AC layout; the static page is the AC layout plus two tyre names.
    ///
    /// Fields marked "not used" are written by the game but always zero in ACC.
    /// Wheel arrays are [FL, FR, RL, RR]. Strings are UTF-16.
    /// </summary>
    public static class Structs
    {
        public const string PhysicsMapName = @"Local\acpmf_physics";
        public const string GraphicsMapName = @"Local\acpmf_graphics";
        public const string StaticMapName = @"Local\acpmf_static";

        public const int MaxCars = 60;

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
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public float[] wheelLoad;            // not used
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public float[] wheelsPressure;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public float[] wheelAngularSpeed;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public float[] tyreWear;             // not used
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public float[] tyreDirtyLevel;       // not used
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public float[] tyreCoreTemperature;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public float[] camberRAD;            // not used
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public float[] suspensionTravel;
            public float drs;                                                                           // not used
            public float tc;
            public float heading;
            public float pitch;
            public float roll;
            public float cgHeight;                                                                      // not used
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)] public float[] carDamage;
            public int numberOfTyresOut;                                                                // not used
            public int pitLimiterOn;
            public float abs;
            public float kersCharge;                                                                    // not used
            public float kersInput;                                                                     // not used
            public int autoShifterOn;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)] public float[] rideHeight;           // not used
            public float turboBoost;
            public float ballast;                                                                       // not used
            public float airDensity;                                                                    // not used
            public float airTemp;
            public float roadTemp;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)] public float[] localAngularVel;
            public float finalFF;
            public float performanceMeter;                                                              // not used
            public int engineBrake;                                                                     // not used
            public int ersRecoveryLevel;                                                                // not used
            public int ersPowerLevel;                                                                   // not used
            public int ersHeatCharging;                                                                 // not used
            public int ersIsCharging;                                                                   // not used
            public float kersCurrentKJ;                                                                 // not used
            public int drsAvailable;                                                                    // not used
            public int drsEnabled;                                                                      // not used
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public float[] brakeTemp;
            public float clutch;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public float[] tyreTempI;            // not used
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public float[] tyreTempM;            // not used
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public float[] tyreTempO;            // not used
            public int isAIControlled;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)] public float[] tyreContactPoint;    // [4][3]
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)] public float[] tyreContactNormal;   // [4][3]
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)] public float[] tyreContactHeading;  // [4][3]
            public float brakeBias;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)] public float[] localVelocity;
            public int P2PActivations;                                                                  // not used
            public int P2PStatus;                                                                       // not used
            public int currentMaxRpm;                                                                   // not used
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public float[] mz;                   // not used
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public float[] fx;                   // not used
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public float[] fy;                   // not used
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public float[] slipRatio;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public float[] slipAngle;
            public int tcInAction;                                                                      // not used
            public int absInAction;                                                                     // not used
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public float[] suspensionDamage;     // not used
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public float[] tyreTemp;             // not used
            public float waterTemp;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public float[] brakePressure;
            public int frontBrakeCompound;
            public int rearBrakeCompound;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public float[] padLife;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public float[] discLife;
            public int ignitionOn;
            public int starterEngineOn;
            public int isEngineRunning;
            public float kerbVibration;
            public float slipVibrations;
            public float gVibrations;
            public float absVibrations;
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
            public float replayTimeMultiplier;                                                          // not used
            public float normalizedCarPosition;
            public int activeCars;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxCars * 3)] public float[] carCoordinates; // [60][3]
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxCars)] public int[] carID;
            public int playerCarID;
            public float penaltyTime;
            public int flag;
            public int penalty;
            public int idealLineOn;
            public int isInPitLane;
            public float surfaceGrip;
            public int mandatoryPitDone;
            public float windSpeed;
            public float windDirection;
            public int isSetupMenuVisible;
            public int mainDisplayIndex;
            public int secondaryDisplayIndex;
            public int TC;
            public int TCCut;
            public int EngineMap;
            public int ABS;
            public float fuelXLap;
            public int rainLights;
            public int flashingLights;
            public int lightsStage;
            public float exhaustTemperature;
            public int wiperLV;
            public int DriverStintTotalTimeLeft;
            public int DriverStintTimeLeft;
            public int rainTyres;
            public int sessionIndex;
            public float usedFuel;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 15)] public string deltaLapTime;
            public int iDeltaLapTime;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 15)] public string estimatedLapTime;
            public int iEstimatedLapTime;
            public int isDeltaPositive;
            public int iSplit;
            public int isValidLap;
            public float fuelEstimatedLaps;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 33)] public string trackStatus;
            public int missingMandatoryPits;
            public float Clock;
            public int directionLightsLeft;
            public int directionLightsRight;
            public int GlobalYellow;
            public int GlobalYellow1;
            public int GlobalYellow2;
            public int GlobalYellow3;
            public int GlobalWhite;
            public int GlobalGreen;
            public int GlobalChequered;
            public int GlobalRed;
            public int mfdTyreSet;
            public float mfdFuelToAdd;
            public float mfdTyrePressureLF;
            public float mfdTyrePressureRF;
            public float mfdTyrePressureLR;
            public float mfdTyrePressureRR;
            public int trackGripStatus;
            public int rainIntensity;
            public int rainIntensityIn10min;
            public int rainIntensityIn30min;
            public int currentTyreSet;
            public int strategyTyreSet;
            public int gapAhead;
            public int gapBehind;
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
            public float maxTorque;                                                                     // not used
            public float maxPower;                                                                      // not used
            public int maxRpm;
            public float maxFuel;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public float[] suspensionMaxTravel;  // not used
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public float[] tyreRadius;           // not used
            public float maxTurboBoost;                                                                 // not used
            public float deprecated_1;
            public float deprecated_2;
            public int penaltiesEnabled;
            public float aidFuelRate;
            public float aidTireRate;
            public float aidMechanicalDamage;
            public int aidAllowTyreBlankets;
            public float aidStability;
            public int aidAutoClutch;
            public int aidAutoBlip;
            public int hasDRS;                                                                          // not used
            public int hasERS;                                                                          // not used
            public int hasKERS;                                                                         // not used
            public float kersMaxJ;                                                                      // not used
            public int engineBrakeSettingsCount;                                                        // not used
            public int ersPowerControllerCount;                                                         // not used
            public float trackSPlineLength;                                                             // not used
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 33)] public string trackConfiguration;    // not used
            public float ersMaxJ;                                                                       // not used
            public int isTimedRace;                                                                     // not used
            public int hasExtraLap;                                                                     // not used
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 33)] public string carSkin;               // not used
            public int reversedGridPositions;                                                           // not used
            public int PitWindowStart;
            public int PitWindowEnd;
            public int isOnline;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 33)] public string dryTyresName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 33)] public string wetTyresName;
        }
    }
}
