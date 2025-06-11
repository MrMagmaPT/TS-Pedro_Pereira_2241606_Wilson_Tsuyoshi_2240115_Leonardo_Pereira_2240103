using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using EI.SI;
using System.Threading;

namespace Servidor
{
    class Program
    {
        static List<string> historicoMensagens = new List<string>();
        static TcpListener server;
        static List<TcpClient> clientes = new List<TcpClient>();

        // Dicionário para armazenar chaves públicas dos utilizadores
        static Dictionary<string, string> chavesPublicas = new Dictionary<string, string>(); 
        static object lockObj = new object();

        static void Main(string[] args)
        {
            int porta = 12345;
            server = new TcpListener(IPAddress.Any, porta);
            server.Start();
            Console.WriteLine($"[Servidor] A ouvir na porta {porta}...");

            while (true)
            {
                TcpClient client = server.AcceptTcpClient();
                Console.WriteLine("[Servidor] Cliente conectado.");
                Thread t = new Thread(TratarCliente);
                t.Start(client);
            }
        }

        static void TratarCliente(object obj)
        {
            TcpClient cliente = (TcpClient)obj;
            NetworkStream ns = cliente.GetStream();
            ProtocolSI protocolo = new ProtocolSI();

            string username = "";

            try
            {
                while (true)
                {
                    ns.Read(protocolo.Buffer, 0, protocolo.Buffer.Length);

                    switch (protocolo.GetCmdType())
                    {
                        case ProtocolSICmdType.USER_OPTION_1:
                            // Recebe o username
                            username = protocolo.GetStringFromData();
                            Console.WriteLine($"[Servidor] Utilizador identificado: {username}");

                            // Pede a chave pública
                            byte[] resposta = protocolo.Make(ProtocolSICmdType.DATA, "Envie a sua chave pública (base64):");
                            ns.Write(resposta, 0, resposta.Length);
                            break;

                        case ProtocolSICmdType.USER_OPTION_2:
                            // Recebe a chave pública
                            string chavePublicaBase64 = protocolo.GetStringFromData();
                            chavesPublicas[username] = chavePublicaBase64;
                            Console.WriteLine($"[Servidor] Chave pública recebida de {username}");

                            // Confirma autenticação
                            byte[] ok = protocolo.Make(ProtocolSICmdType.DATA, "Autenticado com sucesso!");
                            ns.Write(ok, 0, ok.Length);

                            // Adiciona à lista de clientes
                            lock (lockObj)
                                clientes.Add(cliente);

                            break;

                        case ProtocolSICmdType.USER_OPTION_3:
                            // histórico de mensagens
                            lock (lockObj)
                            {
                                foreach (var msg in historicoMensagens)
                                {
                                    byte[] msgPacket = protocolo.Make(ProtocolSICmdType.DATA, msg);
                                    ns.Write(msgPacket, 0, msgPacket.Length);
                                }
                            }
                            break;

                        case ProtocolSICmdType.DATA:
                            // Mensagem de chat
                            string msgChat = protocolo.GetStringFromData();
                            string mensagemFormatada = $"{username}: {msgChat}";
                            Console.WriteLine($"[Mensagem de {username}]: {msgChat}");

                            // Adiciona ao histórico
                            lock (lockObj)
                                historicoMensagens.Add(mensagemFormatada);

                            // Envia para todos os outros clientes
                            EnviarParaTodos($"{username}: {msgChat}", cliente);

                            // Envia ACK para o remetente
                            byte[] ack = protocolo.Make(ProtocolSICmdType.ACK);
                            ns.Write(ack, 0, ack.Length);
                            break;

                        case ProtocolSICmdType.EOF:
                            break;

                        default:
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Erro] {ex.Message}");
            }
            finally
            {
                cliente.Close();
                lock (lockObj)
                    clientes.Remove(cliente);
                Console.WriteLine($"[Servidor] Cliente {username} desconectado.");
            }
        }

        static void EnviarParaTodos(string mensagem, TcpClient remetente)
        {
            ProtocolSI protocolo = new ProtocolSI();
            byte[] dados = protocolo.Make(ProtocolSICmdType.DATA, mensagem);

            lock (lockObj)
            {
                foreach (TcpClient cli in clientes)
                {
                    NetworkStream ns = cli.GetStream();
                    ns.Write(dados, 0, dados.Length);
                }
            }
        }

        

    }
}
