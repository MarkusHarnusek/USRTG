namespace USRTG
{
    public class Program
    {
        private static volatile Packet? latestPacket;
        public readonly string acInstallPath = "C:\\Program Files (x86)\\Steam\\steamapps\\common\\assettocorsa";

        static void Main(string[] args)
        {
            var network = new Network(() => latestPacket);
            network.Start();

            while (true)
            {
                latestPacket = AC.Handle.Run();
            }
        }
    }
}