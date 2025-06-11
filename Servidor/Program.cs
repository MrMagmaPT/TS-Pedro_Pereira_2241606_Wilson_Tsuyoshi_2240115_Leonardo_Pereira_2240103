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
            Console.WriteLine("The Server IS READY");
            int clientCounter = 0;

            while (true)
            {
                //LIGAÇÂO DO CLIENTE
                TcpClient client = listener.AcceptTcpClient();
                //INCREMENTE O NUMERO DE CLIENTES
                clientCounter++;
                Console.WriteLine("Cliente {0} connected.", clientCounter);

                ClientHandler clientHandler = new ClientHandler(client, clientCounter);
                clientHandler.Handle();
            }

        }
    }
}

/*
if (protSI.GetCmdType() == ProtocolSICmdType.DATA)
{
    byte[] dados = protSI.GetData();
    switch (dados[0])
    {
        case 1:
                            
            break;
        case 2:
            //comecone
            break;
        case 3:
            //comcepica
            break;
        default:
            throw new Exception("Tipo de comando desconhecido.");
    }
}
*/
