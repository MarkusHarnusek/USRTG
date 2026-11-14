using System.Diagnostics;

namespace USRTG.Kunos
{
    /// <summary>
    /// Assetto Corsa and Assetto Corsa Competizione publish their telemetry under the very same
    /// shared-memory names (Local\acpmf_physics, Local\acpmf_graphics, Local\acpmf_static), so the
    /// pages alone do not say which game wrote them. The running process is the reliable indicator;
    /// the shared-memory version string is used as a fallback for renamed executables.
    /// </summary>
    internal static class GameDetector
    {
        private const string AcProcessName = "acs";                 // Assetto Corsa (acs.exe)
        private const string AccProcessName = "AC2-Win64-Shipping"; // Assetto Corsa Competizione

        /// <summary>How often the process list is scanned; scanning is comparatively expensive.</summary>
        public static readonly TimeSpan ScanInterval = TimeSpan.FromSeconds(2);

        private static Enums.Game _cached = Enums.Game.Unknown;
        private static DateTime _lastScan = DateTime.MinValue;

        public static Enums.Game DetectByProcess()
        {
            var now = DateTime.UtcNow;
            if (now - _lastScan < ScanInterval)
            {
                return _cached;
            }

            _lastScan = now;
            _cached = IsRunning(AccProcessName) ? Enums.Game.AssettoCorsaCompetizione
                    : IsRunning(AcProcessName) ? Enums.Game.AssettoCorsa
                    : Enums.Game.Unknown;
            return _cached;
        }

        /// <summary>
        /// Fallback when no known process is found but a shared memory page exists:
        /// AC reports shared memory version 1.7, ACC reports 1.8 and higher.
        /// </summary>
        public static Enums.Game DetectBySharedMemoryVersion(string? smVersion)
        {
            if (string.IsNullOrWhiteSpace(smVersion))
            {
                return Enums.Game.Unknown;
            }

            var parts = smVersion.Trim().Split('.');
            if (parts.Length < 2 || !int.TryParse(parts[0], out var major) || !int.TryParse(parts[1], out var minor))
            {
                return Enums.Game.Unknown;
            }

            return major > 1 || (major == 1 && minor >= 8)
                ? Enums.Game.AssettoCorsaCompetizione
                : Enums.Game.AssettoCorsa;
        }

        private static bool IsRunning(string processName)
        {
            Process[] processes;
            try
            {
                processes = Process.GetProcessesByName(processName);
            }
            catch (InvalidOperationException)
            {
                return false;
            }

            try
            {
                return processes.Length > 0;
            }
            finally
            {
                foreach (var p in processes)
                {
                    p.Dispose();
                }
            }
        }
    }
}
