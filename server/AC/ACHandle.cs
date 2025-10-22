using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;

namespace USRTG.AC
{
    public class ACHandle
    {
        public class ACStructs
        {
            public class TCMapping
            {
                private List<float> tcCurve = new List<float>();
                private string lastCarModel = string.Empty;
                private string acInstallPath = Program.acInstallPath;

                public TCMapping(string installPath)
                {
                    acInstallPath = installPath;
                }

                // Call this when car changes (check SPageFileStatic.carModel)
                public void LoadCurveForCar(string carModel)
                {
                    if (carModel == lastCarModel) return;

                    string iniPath = Path.Combine(acInstallPath, "content", "cars", carModel, "data", "electronics.ini");
                    if (!File.Exists(iniPath))
                    {
                        // Handle error: no file, assume no multi-level TC or fallback to tc * 10 or something
                        tcCurve.Clear();
                        lastCarModel = string.Empty;
                        return;
                    }

                    bool inTCSection = false;
                    tcCurve.Clear();

                    foreach (string line in File.ReadLines(iniPath))
                    {
                        string trimmed = line.Trim();
                        if (trimmed.StartsWith("[TRACTION_CONTROL]"))
                        {
                            inTCSection = true;
                            continue;
                        }
                        if (inTCSection && trimmed.StartsWith("["))
                        {
                            inTCSection = false; // End of section
                            break;
                        }
                        if (inTCSection && trimmed.StartsWith("CURVE="))
                        {
                            string curveStr = trimmed.Substring("CURVE=".Length).Trim();
                            if (!string.IsNullOrEmpty(curveStr))
                            {
                                string[] values = curveStr.Split(',');
                                foreach (string val in values)
                                {
                                    if (float.TryParse(val.Trim(), out float slip))
                                    {
                                        tcCurve.Add(slip);
                                    }
                                }
                            }
                            break; // Found CURVE, no need to continue
                        }
                    }

                    lastCarModel = carModel;
                }

                // Call this after reading tc from shared memory
                public int GetTCLevel(float tc)
                {
                    if (tc == 0) return 0; // Off

                    if (tcCurve.Count == 0)
                    {
                        return -1; // Error, no curve loaded
                    }

                    for (int i = 0; i < tcCurve.Count; i++)
                    {
                        if (Math.Abs(tcCurve[i] - tc) < 0.001f)
                        {
                            return i + 1; // 1-based level
                        }
                    }

                    return -1; // No match, possible error
                }
            }
        }

        private const string PhysicsMapName = @"Local\acpmf_physics";
        private const string GraphicsMapName = @"Local\acpmf_graphics";
        private const string StaticMapName = @"Local\acpmf_static";

        // Sizes calculated via tool (adjust if fields change)
        private const int PhysicsSize = 288;
        private const int GraphicsSize = 468;
        private const int StaticSize = 864;

        public static Packet Run()
        {
            (var physics, var graphics, var staticData) = GetData();

            return new Packet(
                id: graphics.packetId,
                gas: physics.gas,
                brake: physics.brake,
                steerAngle: physics.steerAngle,
                speedKmh: physics.speedKmh,
                gear: physics.gear,
                rpms: physics.rpms,
                fuel: physics.fuel,
                maxFuel: staticData.maxFuel,
                maxRpm: staticData.maxRpm,
                turboBoost: physics.turboBoost,
                ballast: physics.ballast,
                drs: physics.drs,
                tc: physics.tc,
                abs: physics.abs,
                kersCharge: physics.kersCharge,
                kersInput: physics.kersInput,
                heading: physics.heading,
                pitch: physics.pitch,
                roll: physics.roll,
                status: (AC.ACEnums.AC_STATUS)graphics.status,
                session: (AC.ACEnums.AC_SESSION_TYPE)graphics.session,
                flag: (AC.ACEnums.AC_FLAG_TYPE)graphics.flag,
                sessionTimeLeft: graphics.sessionTimeLeft,
                numCars: staticData.numCars,
                sectorCount: staticData.sectorCount,
                airTemp: physics.airTemp, // Changed to physics
                roadTemp: physics.roadTemp, // Changed to physics
                surfaceGrip: graphics.surfaceGrip,
                normalizedCarPosition: (int)graphics.normalizedCarPosition,
                completedLaps: graphics.completedLaps,
                position: graphics.position,
                currentTime: graphics.iCurrentTime,
                lastTime: graphics.iLastTime,
                bestTime: graphics.iBestTime,
                distanceTraveled: graphics.distanceTraveled,
                isInPit: graphics.isInPit,
                isInPitLane: graphics.isInPitLane,
                currentSectorIndex: graphics.currentSectorIndex,
                lastSectorTime: graphics.lastSectorTime,
                tyreCompound: graphics.tyreCompound
            );
        }

        private static (ACStructs.SPageFilePhysics, ACStructs.SPageFileGraphic, ACStructs.SPageFileStatic) GetData()
        {
            MemoryMappedFile mmfPhysics = null!;
            MemoryMappedFile mmfGraphics = null!;
            MemoryMappedFile mmfStatic = null!;
            MemoryMappedViewAccessor accessorPhysics = null!;
            MemoryMappedViewAccessor accessorGraphics = null!;
            MemoryMappedViewAccessor accessorStatic = null!;

            try
            {
                //ValidateStructureSizes();

                mmfPhysics = MemoryMappedFile.OpenExisting(PhysicsMapName);
                accessorPhysics = mmfPhysics.CreateViewAccessor(0, PhysicsSize, MemoryMappedFileAccess.Read);

                mmfGraphics = MemoryMappedFile.OpenExisting(GraphicsMapName);
                accessorGraphics = mmfGraphics.CreateViewAccessor(0, GraphicsSize, MemoryMappedFileAccess.Read);

                mmfStatic = MemoryMappedFile.OpenExisting(StaticMapName);
                accessorStatic = mmfStatic.CreateViewAccessor(0, StaticSize, MemoryMappedFileAccess.Read);

                // Static data can be read once
                byte[] staticBuffer = new byte[StaticSize];
                accessorStatic.ReadArray(0, staticBuffer, 0, staticBuffer.Length);
                GCHandle staticHandle = GCHandle.Alloc(staticBuffer, GCHandleType.Pinned);
                ACStructs.SPageFileStatic staticData = (ACStructs.SPageFileStatic)Marshal.PtrToStructure(staticHandle.AddrOfPinnedObject(), typeof(ACStructs.SPageFileStatic));
                staticHandle.Free();

                // Read physics
                byte[] physicsBuffer = new byte[PhysicsSize];
                accessorPhysics.ReadArray(0, physicsBuffer, 0, physicsBuffer.Length);
                GCHandle physicsHandle = GCHandle.Alloc(physicsBuffer, GCHandleType.Pinned);
                ACStructs.SPageFilePhysics physics = (ACStructs.SPageFilePhysics)Marshal.PtrToStructure(physicsHandle.AddrOfPinnedObject(), typeof(ACStructs.SPageFilePhysics));
                physicsHandle.Free();

                // Read graphics
                byte[] graphicsBuffer = new byte[GraphicsSize];
                accessorGraphics.ReadArray(0, graphicsBuffer, 0, graphicsBuffer.Length);
                GCHandle graphicsHandle = GCHandle.Alloc(graphicsBuffer, GCHandleType.Pinned);
                ACStructs.SPageFileGraphic graphics = (ACStructs.SPageFileGraphic)Marshal.PtrToStructure(graphicsHandle.AddrOfPinnedObject(), typeof(ACStructs.SPageFileGraphic));
                graphicsHandle.Free();

                return (physics, graphics, staticData);
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("One or more shared memory maps not found. Ensure Assetto Corsa is running.");
                return (new ACStructs.SPageFilePhysics(), new ACStructs.SPageFileGraphic(), new ACStructs.SPageFileStatic());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}. Verify game is in an active session.");
                return (new ACStructs.SPageFilePhysics(), new ACStructs.SPageFileGraphic(), new ACStructs.SPageFileStatic());
            }
            finally
            {
                accessorPhysics?.Dispose();
                accessorGraphics?.Dispose();
                accessorStatic?.Dispose();
                mmfPhysics?.Dispose();
                mmfGraphics?.Dispose();
                mmfStatic?.Dispose();
            }
        }

        private static void ValidateStructureSizes()
        {
            int physicsSize = Marshal.SizeOf(typeof(ACStructs.SPageFilePhysics));
            int graphicsSize = Marshal.SizeOf(typeof(ACStructs.SPageFileGraphic));
            int staticSize = Marshal.SizeOf(typeof(ACStructs.SPageFileStatic));
            Console.WriteLine($"Calculated Sizes: Physics={physicsSize}, Graphics={graphicsSize}, Static={staticSize}");
            Console.WriteLine($"Expected Sizes: Physics={PhysicsSize}, Graphics={GraphicsSize}, Static={StaticSize}");
            if (physicsSize != PhysicsSize || graphicsSize != GraphicsSize || staticSize != StaticSize)
            {
                Console.WriteLine("Error: Structure sizes do not match expected values!");
            }
        }
    }
}
