using System.Diagnostics;

namespace USRTG.Telemetry
{
    /// <summary>
    /// Polls the registered sources at a fixed rate and keeps the latest packet.
    /// The first source that returns a packet wins, so the order of <paramref name="sources"/> is the priority.
    /// </summary>
    internal sealed class TelemetryManager : IDisposable
    {
        private readonly IReadOnlyList<ITelemetrySource> _sources;
        private readonly TimeSpan _interval;
        private static readonly TimeSpan IdleInterval = TimeSpan.FromMilliseconds(500);

        private volatile Packet? _latest;
        private volatile CommonPacket? _latestCommon;

        public TelemetryManager(IReadOnlyList<ITelemetrySource> sources, double pollHz)
        {
            _sources = sources;
            _interval = TimeSpan.FromSeconds(1.0 / Math.Clamp(pollHz, 1, 500));
        }

        public Packet? Latest => _latest;
        public CommonPacket? LatestCommon => _latestCommon;

        /// <summary>Runs the poll loop on the calling thread until <paramref name="token"/> is cancelled.</summary>
        public void Run(CancellationToken token)
        {
            ITelemetrySource? active = null;
            var clock = Stopwatch.StartNew();

            while (!token.IsCancellationRequested)
            {
                var tickStart = clock.Elapsed;
                Packet? packet = null;
                ITelemetrySource? producer = null;

                foreach (var source in _sources)
                {
                    try
                    {
                        packet = source.TryRead();
                    }
                    catch (Exception ex)
                    {
                        // A broken adapter must not take the whole server down.
                        Console.WriteLine($"[{source.Name}] read failed: {ex.Message}");
                        packet = null;
                    }

                    if (packet != null)
                    {
                        producer = source;
                        break;
                    }
                }

                if (producer != active)
                {
                    Console.WriteLine(producer == null
                        ? $"Lost telemetry source {active!.Name}; waiting for a game."
                        : $"Telemetry source: {producer.Name}");
                    active = producer;
                }

                _latest = packet;
                _latestCommon = packet == null ? null : CommonPacket.From(packet);

                var wait = (packet == null ? IdleInterval : _interval) - (clock.Elapsed - tickStart);
                if (wait > TimeSpan.Zero)
                {
                    token.WaitHandle.WaitOne(wait);
                }
            }
        }

        public void Dispose()
        {
            foreach (var source in _sources)
            {
                source.Dispose();
            }
        }
    }
}
