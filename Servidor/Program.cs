using EI.SI;
using Projeto_TS_Pedro_Pereira_2241606_Wilson_Tsuyoshi_2240115;
using Servidor.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Servidor
{
    class Program
    {
        private const int PORT = 12345;
        private static int clienteCounter = 0;
        static void Main(string[] args)
        {
            //combina o ip com o Port
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, PORT);

            TcpListener listener = new TcpListener(endPoint);

            listener.Start();
            Console.WriteLine("[Servidor] - O Server está online e a ouvir na porta: "+PORT);
            int clientCounter = 0;

            while (true)
            {
                //LIGAÇÂO DO CLIENTE
                TcpClient client = listener.AcceptTcpClient();
                //INCREMENTE O NUMERO DE CLIENTES
                clientCounter++;
                Console.WriteLine("[Servidor] - Cliente connectado.");

                ClientHandler clientHandler = new ClientHandler(client);
                clientHandler.Handle();
            }

        }
    }
}