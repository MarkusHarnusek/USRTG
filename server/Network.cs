using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace USRTG
{
    /// <summary>
    /// Minimal HTTP server. It serves exactly one resource, GET /packet, which returns the current
    /// <see cref="CommonPacket"/> as JSON, or 204 No Content while no game is running.
    /// </summary>
    internal sealed class Network
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals // NaN/Infinity from the game must not kill the response
        };

        private readonly Func<CommonPacket?> _getPacket;
        private readonly int _port;
        private readonly CancellationTokenSource _cts = new();
        private HttpListener? _listener;
        private Task? _serverTask;

        public Network(Func<CommonPacket?> getPacket, int port = 8080)
        {
            _getPacket = getPacket;
            _port = port;
        }

        /// <summary>Starts listening. Returns false if no prefix could be bound.</summary>
        public bool Start()
        {
            _listener = TryStart($"http://+:{_port}/") ?? TryStart($"http://localhost:{_port}/");
            if (_listener == null)
            {
                Console.WriteLine($"HTTP server could not bind port {_port}.");
                return false;
            }

            _serverTask = Task.Run(() => AcceptLoop(_listener, _cts.Token));
            return true;
        }

        private HttpListener? TryStart(string prefix)
        {
            var listener = new HttpListener();
            listener.Prefixes.Add(prefix);
            try
            {
                listener.Start();
                Console.WriteLine($"HTTP server listening on {prefix}packet (local IP {GetLocalIPAddress()})");
                return listener;
            }
            catch (HttpListenerException ex)
            {
                listener.Close();
                if (prefix.Contains('+'))
                {
                    // Binding all interfaces needs a URL ACL on Windows unless running elevated.
                    Console.WriteLine($"Cannot bind {prefix} ({ex.Message}). Falling back to localhost only. " +
                                      $"To allow LAN access run once as administrator: netsh http add urlacl url={prefix} user=Everyone");
                }
                else
                {
                    Console.WriteLine($"Cannot bind {prefix}: {ex.Message}");
                }
                return null;
            }
        }

        public void Stop()
        {
            _cts.Cancel();
            _listener?.Stop();
            try
            {
                _serverTask?.Wait(TimeSpan.FromSeconds(1));
            }
            catch (AggregateException)
            {
                // cancellation
            }
            _listener?.Close();
        }

        private async Task AcceptLoop(HttpListener listener, CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                HttpListenerContext context;
                try
                {
                    context = await listener.GetContextAsync();
                }
                catch (Exception) when (token.IsCancellationRequested)
                {
                    break;
                }
                catch (HttpListenerException ex)
                {
                    Console.WriteLine($"HTTP accept failed: {ex.Message}");
                    continue;
                }
                catch (ObjectDisposedException)
                {
                    break;
                }

                _ = Task.Run(() => HandleContext(context, token), token);
            }
        }

        private async Task HandleContext(HttpListenerContext context, CancellationToken token)
        {
            var response = context.Response;
            try
            {
                response.Headers.Add("Access-Control-Allow-Origin", "*");
                response.Headers.Add("Access-Control-Allow-Methods", "GET, OPTIONS");
                response.Headers.Add("Access-Control-Allow-Headers", "Content-Type");
                response.Headers.Add("Cache-Control", "no-store");

                if (context.Request.HttpMethod == "OPTIONS")
                {
                    response.StatusCode = 204;
                    return;
                }

                if (context.Request.HttpMethod != "GET" || context.Request.Url?.AbsolutePath != "/packet")
                {
                    response.StatusCode = 404;
                    return;
                }

                var packet = _getPacket();
                if (packet == null)
                {
                    response.StatusCode = 204;
                    return;
                }

                byte[] json = JsonSerializer.SerializeToUtf8Bytes(packet, JsonOptions);
                response.StatusCode = 200;
                response.ContentType = "application/json; charset=utf-8";
                response.ContentLength64 = json.Length;
                await response.OutputStream.WriteAsync(json, token);
            }
            catch (Exception ex) when (ex is HttpListenerException or IOException or ObjectDisposedException)
            {
                // Client went away mid-response; nothing to do.
            }
            catch (Exception ex)
            {
                Console.WriteLine($"HTTP request failed: {ex.Message}");
                try { response.StatusCode = 500; } catch { /* headers may already be sent */ }
            }
            finally
            {
                try { response.Close(); } catch { /* already closed */ }
            }
        }

        private static string GetLocalIPAddress()
        {
            try
            {
                foreach (var ip in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(ip))
                    {
                        return ip.ToString();
                    }
                }
            }
            catch (SocketException)
            {
                // No network; purely informational anyway.
            }
            return "unknown";
        }
    }
}
