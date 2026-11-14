using System.Runtime.Versioning;
using USRTG.Telemetry;

namespace USRTG.Kunos
{
    /// <summary>
    /// Telemetry source for the two Kunos titles. They share the shared-memory names, so a single
    /// source decides which game is running and delegates to the matching handler. Only one handler
    /// keeps its mappings open at a time.
    /// </summary>
    [SupportedOSPlatform("windows")]
    internal sealed class KunosSource : ITelemetrySource
    {
        private readonly AC.Handle _ac = new();
        private readonly ACC.Handle _acc = new();
        private readonly SharedMemoryPage<AC.Structs.SPageFileStatic> _probe = new(AC.Structs.StaticMapName);

        public string Name => "Assetto Corsa / Assetto Corsa Competizione";

        public Packet? TryRead()
        {
            var game = GameDetector.DetectByProcess();

            if (game == Enums.Game.Unknown)
            {
                // No known executable, but maybe a renamed one wrote the pages anyway.
                // The static page prefix is identical in both games, so the AC layout is safe to probe with.
                if (!_probe.TryRead(out var staticPage))
                {
                    _ac.Close();
                    _acc.Close();
                    return null;
                }
                game = GameDetector.DetectBySharedMemoryVersion(staticPage.smVersion);
            }
            _probe.Close();

            switch (game)
            {
                case Enums.Game.AssettoCorsa:
                    _acc.Close();
                    return _ac.TryRead();
                case Enums.Game.AssettoCorsaCompetizione:
                    _ac.Close();
                    return _acc.TryRead();
                default:
                    _ac.Close();
                    _acc.Close();
                    return null;
            }
        }

        public void Dispose()
        {
            _ac.Dispose();
            _acc.Dispose();
            _probe.Dispose();
        }
    }
}
