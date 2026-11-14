using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace USRTG.Kunos
{
    /// <summary>
    /// One named shared-memory page written by a Kunos game (Assetto Corsa / Assetto Corsa Competizione).
    ///
    /// The mapping is opened once and kept open between reads. Opening and closing three mappings on
    /// every poll (the previous behaviour) costs several syscalls per frame and leaves a window in
    /// which the game cannot be told apart from "not running".
    /// </summary>
    [SupportedOSPlatform("windows")]
    internal sealed class SharedMemoryPage<T> : IDisposable where T : struct
    {
        private readonly string _name;
        private readonly int _size;
        private readonly byte[] _buffer;
        private MemoryMappedFile? _mmf;
        private MemoryMappedViewAccessor? _accessor;

        public SharedMemoryPage(string name)
        {
            _name = name;
            _size = Marshal.SizeOf<T>();
            _buffer = new byte[_size];
        }

        public bool IsOpen => _accessor != null;

        /// <summary>Returns false when the page does not exist, i.e. the game is not running.</summary>
        public bool TryRead(out T value)
        {
            try
            {
                if (_accessor == null)
                {
                    _mmf = MemoryMappedFile.OpenExisting(_name, MemoryMappedFileRights.Read);
                    _accessor = _mmf.CreateViewAccessor(0, _size, MemoryMappedFileAccess.Read);
                }

                _accessor.ReadArray(0, _buffer, 0, _size);
                var handle = GCHandle.Alloc(_buffer, GCHandleType.Pinned);
                try
                {
                    value = Marshal.PtrToStructure<T>(handle.AddrOfPinnedObject());
                }
                finally
                {
                    handle.Free();
                }
                return true;
            }
            catch (FileNotFoundException)
            {
                Close();
                value = default;
                return false;
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                // The game went away (or is still starting) between opening the mapping and reading it.
                Close();
                value = default;
                return false;
            }
        }

        /// <summary>
        /// Releases the mapping. A shared-memory object stays alive while anyone holds a handle to it,
        /// so handles are dropped as soon as the game is gone; otherwise the next game instance (or the
        /// other Kunos title, which uses the same names) would inherit a stale object.
        /// </summary>
        public void Close()
        {
            _accessor?.Dispose();
            _mmf?.Dispose();
            _accessor = null;
            _mmf = null;
        }

        public void Dispose() => Close();
    }
}
