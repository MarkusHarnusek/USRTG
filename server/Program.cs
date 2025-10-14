using System;
using USRTG.AC;

namespace USRTG
{
    internal class Program
    {
        private static volatile Packet? latestPacket;

        static void Main(string[] args)
        {
            var network = new Network(() => latestPacket);
            network.Start();

            while (true)
            {
                latestPacket = ACHandle.Run();
            }
        }
    }
}