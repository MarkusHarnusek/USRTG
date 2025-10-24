using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace USRTG
{
    internal class Network
    {
        private readonly Func<Packet?> _getPacket;
        private readonly int _port;
        private readonly CancellationTokenSource _cts = new();

        public Network(Func<Packet?> getPacket, int port = 8080)
        {
            _getPacket = getPacket;
            _port = port;
        }

        public void Start()
        {
            Task.Run(() => RunServer(_cts.Token));
        }

        public void Stop()
        {
            _cts.Cancel();
        }

        private async Task RunServer(CancellationToken token)
        {
            var prefix = $"http://+:{_port}/";
            Console.WriteLine($"Starting HTTP server on {prefix} on local ip {GetLocalIPAddress()}");
            using var listener = new HttpListener();
            listener.Prefixes.Add(prefix);
            listener.Start();
            Console.WriteLine($"HTTP server started on {prefix}");

            while (!token.IsCancellationRequested)
            {
                var context = await listener.GetContextAsync();
                _ = Task.Run(() => HandleContext(context, token));
            }
        }

        private async Task HandleContext(HttpListenerContext context, CancellationToken token)
        {
            context.Response.Headers.Add("Access-Control-Allow-Origin", "*");
            context.Response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, OPTIONS");
            context.Response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Authorization");

            if (context.Request.HttpMethod == "OPTIONS")
            {
                context.Response.StatusCode = 200;
                context.Response.Close();
                return;
            }

            if (context.Request.HttpMethod == "GET" && context.Request.Url?.AbsolutePath == "/packet")
            {
                var packet = _getPacket();
                if (packet != null)
                {
                    string json = JsonSerializer.Serialize(packet, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
                    });
                    context.Response.ContentType = "application/json";
                    context.Response.StatusCode = 200;
                    await context.Response.OutputStream.WriteAsync(System.Text.Encoding.UTF8.GetBytes(json), token);
                }
                else
                {
                    context.Response.StatusCode = 204;
                }
            }
            else
            {
                context.Response.StatusCode = 404;
            }
            context.Response.Close();
        }

        private static IPAddress GetLocalIPAddress()
        {
            foreach (var ip in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip;
                }
            }
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }
    }
}
