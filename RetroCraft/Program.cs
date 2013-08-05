using System;
using System.Net;
using System.Linq;
using System.Net.Sockets;

namespace RetroCraft
{
    class Program
    {
        public static void Main(string[] args)
        {
            var remote = ParseEndPoint(args[0]);
            var proxy = new Proxy(new IPEndPoint(IPAddress.Loopback, 25564), remote);
            proxy.Start();

            Console.WriteLine("You can connect to 127.0.0.1:25564 now.");
            Console.WriteLine("Press 'q' to exit.");
            ConsoleKeyInfo cki;
            do cki = Console.ReadKey(true);
            while (cki.KeyChar != 'q');
        }

        private static IPEndPoint ParseEndPoint(string arg)
        {
            IPAddress address;
            int port;
            if (arg.Contains(':'))
            {
                // Both IP and port are specified
                var parts = arg.Split(':');
                if (!IPAddress.TryParse(parts[0], out address))
                    address = Resolve(parts[0]);
                return new IPEndPoint(address, int.Parse(parts[1]));
            }
            if (IPAddress.TryParse(arg, out address))
                return new IPEndPoint(address, 25565);
            if (int.TryParse(arg, out port))
                return new IPEndPoint(IPAddress.Loopback, port);
            return new IPEndPoint(Resolve(arg), 25565);
        }

        private static IPAddress Resolve(string arg)
        {
            return Dns.GetHostEntry(arg).AddressList.FirstOrDefault(item => item.AddressFamily == AddressFamily.InterNetwork);
        }
    }
}
