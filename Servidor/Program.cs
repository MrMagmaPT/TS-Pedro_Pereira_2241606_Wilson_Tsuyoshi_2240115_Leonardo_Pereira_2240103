using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Servidor
{
    class Program
    {
        private const int PORT = 10000;
        private static int clientCounter = 0;
        static void Main(string[] args)
        {
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, PORT);
            TcpListener listener = new TcpListener(endPoint);

            listener.Start();
            Console.WriteLine("Server Ready!!!");
            int clientCounter = 0;


            
            while (clientCounter < 2)
            {
                TcpClient client = listener.AcceptTcpClient();
                clientCounter++;
                Console.WriteLine("Cliente {0} connected", clientCounter);

                if (clientCounter == 1) {
                    TcpClient cliente1 = listener.AcceptTcpClient();
                } else if (clientCounter == 2)
                {
                    TcpClient cliente2 = listener.AcceptTcpClient();
                }
            }
            Console.WriteLine("Tenho os dois clientes");
        }
    }
}
