using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;
#pragma warning disable CA1416
#pragma warning disable CA2263

namespace USRTG.AC
{
    public class Handle
    {
        private const string PhysicsMapName = @"Local\acpmf_physics";
        private const string GraphicsMapName = @"Local\acpmf_graphics";
        private const string StaticMapName = @"Local\acpmf_static";

        private const int PhysicsSize = 288;
        private const int GraphicsSize = 468;
        private const int StaticSize = 864;
        private static TCMapping tc = new TCMapping();
        private static ABSMapping abs = new ABSMapping();

        public static Packet Run()
        {
            (var physics, var graphics, var staticData) = GetData();
            tc.UpdateCurve(staticData.carModel, physics.tc);
            abs.UpdateCurve(staticData.carModel, physics.abs);

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
                tc: tc.GetTCLevel(staticData.carModel, physics.tc),
                abs: abs.GetABSLevel(staticData.carModel, physics.abs),
                kersCharge: physics.kersCharge,
                kersInput: physics.kersInput,
                heading: physics.heading,
                pitch: physics.pitch,
                roll: physics.roll,
                gameState: Mapping.MapGameState(graphics.status, USRTG.Enums.Game.AssettoCorsa),
                sessionType: Mapping.MapSessionType(graphics.session, USRTG.Enums.Game.AssettoCorsa),
                flag: Mapping.MapFlagType(graphics.flag, USRTG.Enums.Game.AssettoCorsa),
                sessionTimeLeft: graphics.sessionTimeLeft,
                numCars: staticData.numCars,
                sectorCount: staticData.sectorCount,
                airTemp: physics.airTemp,
                roadTemp: physics.roadTemp,
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
                tyreCompound: graphics.tyreCompound,
                tyreWear: physics.tyreWear,
                tyreTempInner: physics.tyreTempI,
                tyreTempMiddle: physics.tyreTempM,
                tyreTempOuter: physics.tyreTempO,
                tyreCoreTemp: physics.tyreCoreTemperature,
                wheelSlip: physics.wheelSlip
            );
        }

        private static (Structs.SPageFilePhysics, Structs.SPageFileGraphic, Structs.SPageFileStatic) GetData()
        {
            MemoryMappedFile mmfPhysics = null!;
            MemoryMappedFile mmfGraphics = null!;
            MemoryMappedFile mmfStatic = null!;
            MemoryMappedViewAccessor accessorPhysics = null!;
            MemoryMappedViewAccessor accessorGraphics = null!;
            MemoryMappedViewAccessor accessorStatic = null!;

            //ValidateStructureSizes();

            try
            {
                mmfPhysics = MemoryMappedFile.OpenExisting(PhysicsMapName);
                accessorPhysics = mmfPhysics.CreateViewAccessor(0, PhysicsSize, MemoryMappedFileAccess.Read);

                mmfGraphics = MemoryMappedFile.OpenExisting(GraphicsMapName);
                accessorGraphics = mmfGraphics.CreateViewAccessor(0, GraphicsSize, MemoryMappedFileAccess.Read);

                mmfStatic = MemoryMappedFile.OpenExisting(StaticMapName);
                accessorStatic = mmfStatic.CreateViewAccessor(0, StaticSize, MemoryMappedFileAccess.Read);

#pragma warning disable CS8605

                byte[] staticBuffer = new byte[StaticSize];
                accessorStatic.ReadArray(0, staticBuffer, 0, staticBuffer.Length);
                GCHandle staticHandle = GCHandle.Alloc(staticBuffer, GCHandleType.Pinned);
                Structs.SPageFileStatic staticData = (Structs.SPageFileStatic)Marshal.PtrToStructure(staticHandle.AddrOfPinnedObject(), typeof(Structs.SPageFileStatic));
                staticHandle.Free();

                byte[] physicsBuffer = new byte[PhysicsSize];
                accessorPhysics.ReadArray(0, physicsBuffer, 0, physicsBuffer.Length);
                GCHandle physicsHandle = GCHandle.Alloc(physicsBuffer, GCHandleType.Pinned);
                Structs.SPageFilePhysics physics = (Structs.SPageFilePhysics)Marshal.PtrToStructure(physicsHandle.AddrOfPinnedObject(), typeof(Structs.SPageFilePhysics));
                physicsHandle.Free();

                byte[] graphicsBuffer = new byte[GraphicsSize];
                accessorGraphics.ReadArray(0, graphicsBuffer, 0, graphicsBuffer.Length);
                GCHandle graphicsHandle = GCHandle.Alloc(graphicsBuffer, GCHandleType.Pinned);
                Structs.SPageFileGraphic graphics = (Structs.SPageFileGraphic)Marshal.PtrToStructure(graphicsHandle.AddrOfPinnedObject(), typeof(Structs.SPageFileGraphic));
                graphicsHandle.Free();

#pragma warning restore CS8605

                return (physics, graphics, staticData);
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("One or more shared memory maps not found. Ensure Assetto Corsa is running.");
                return (new Structs.SPageFilePhysics(), new Structs.SPageFileGraphic(), new Structs.SPageFileStatic());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}. Verify game is in an active session.");
                return (new Structs.SPageFilePhysics(), new Structs.SPageFileGraphic(), new Structs.SPageFileStatic());
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
            int physicsSize = Marshal.SizeOf(typeof(Structs.SPageFilePhysics));
            int graphicsSize = Marshal.SizeOf(typeof(Structs.SPageFileGraphic));
            int staticSize = Marshal.SizeOf(typeof(Structs.SPageFileStatic));
            Console.WriteLine($"Calculated Sizes: Physics={physicsSize}, Graphics={graphicsSize}, Static={staticSize}");
            Console.WriteLine($"Expected Sizes: Physics={PhysicsSize}, Graphics={GraphicsSize}, Static={StaticSize}");
            if (physicsSize != PhysicsSize || graphicsSize != GraphicsSize || staticSize != StaticSize)
            {
                Console.WriteLine("Error: Structure sizes do not match expected values!");
            }
        }
    }
}
