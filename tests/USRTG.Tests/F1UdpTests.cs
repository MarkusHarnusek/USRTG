using System.Net;
using System.Net.Sockets;
using USRTG.F1_2020;
using Xunit;

namespace USRTG.Tests
{
    /// <summary>End-to-end: real UDP datagrams through the listener on a loopback port.</summary>
    public class F1UdpTests
    {
        [Fact]
        public async Task PacketsSentOverUdp_ProduceATelemetryPacket()
        {
            int port = FreeUdpPort();
            using var handle = new Handle(port);
            handle.Start();
            Assert.True(handle.IsListening);

            using var sender = new UdpClient();
            var target = new IPEndPoint(IPAddress.Loopback, port);
            foreach (var datagram in new[]
                     {
                         F1PacketBuilder.Session(), F1PacketBuilder.LapData(), F1PacketBuilder.CarStatus(),
                         F1PacketBuilder.Motion(), F1PacketBuilder.Participants(), F1PacketBuilder.CarTelemetry()
                     })
            {
                await sender.SendAsync(datagram, datagram.Length, target);
            }

            Packet? packet = null;
            for (int i = 0; i < 50 && packet == null; i++)
            {
                await Task.Delay(20);
                packet = handle.TryRead();
            }

            Assert.NotNull(packet);
            Assert.Equal(Enums.Game.F1_2020, packet.Game);
            Assert.Equal(287f, packet.SpeedKmh);
            Assert.Equal("Silverstone", packet.Track);
        }

        [Fact]
        public void PortInUse_DisablesSourceInsteadOfThrowing()
        {
            int port = FreeUdpPort();
            using var blocker = new UdpClient(new IPEndPoint(IPAddress.Any, port));
            using var handle = new Handle(port);
            handle.Start();
            Assert.False(handle.IsListening);
            Assert.Null(handle.TryRead());
        }

        private static int FreeUdpPort()
        {
            using var probe = new UdpClient(new IPEndPoint(IPAddress.Loopback, 0));
            return ((IPEndPoint)probe.Client.LocalEndPoint!).Port;
        }
    }
}
