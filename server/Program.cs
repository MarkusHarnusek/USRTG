using USRTG.Telemetry;

namespace USRTG
{
    public static class Program
    {
        private const int DefaultHttpPort = 8080;
        private const double DefaultPollHz = 60;

        /// <summary>
        /// Usage: USRTG [--port 8080] [--f1-port 20777] [--rate 60]
        /// </summary>
        public static int Main(string[] args)
        {
            int httpPort = IntArg(args, "--port", DefaultHttpPort);
            int f1Port = IntArg(args, "--f1-port", F1_2020.Spec.DefaultPort);
            double pollHz = IntArg(args, "--rate", (int)DefaultPollHz);

            var sources = new List<ITelemetrySource>();
            if (OperatingSystem.IsWindows())
            {
                sources.Add(new Kunos.KunosSource());
            }
            else
            {
                Console.WriteLine("Assetto Corsa / ACC shared memory is only available on Windows; those sources are disabled.");
            }

            var f1 = new F1_2020.Handle(f1Port);
            f1.Start();
            sources.Add(f1);

            using var cts = new CancellationTokenSource();
            Console.CancelKeyPress += (_, e) =>
            {
                e.Cancel = true;
                cts.Cancel();
            };

            using var manager = new TelemetryManager(sources, pollHz);
            var network = new Network(() => manager.LatestCommon, httpPort);
            if (!network.Start())
            {
                return 1;
            }

            Console.WriteLine($"Polling telemetry at {pollHz:0} Hz. Press Ctrl+C to stop.");
            manager.Run(cts.Token);

            network.Stop();
            Console.WriteLine("Stopped.");
            return 0;
        }

        private static int IntArg(string[] args, string name, int fallback)
        {
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (string.Equals(args[i], name, StringComparison.OrdinalIgnoreCase) && int.TryParse(args[i + 1], out var value))
                {
                    return value;
                }
            }
            return fallback;
        }
    }
}
