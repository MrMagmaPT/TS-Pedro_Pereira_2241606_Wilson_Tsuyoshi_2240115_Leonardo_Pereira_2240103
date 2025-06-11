using EI.SI;
using Projeto_TS_Pedro_Pereira_2241606_Wilson_Tsuyoshi_2240115.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Projeto_TS_Pedro_Pereira_2241606_Wilson_Tsuyoshi_2240115.Models;
using Microsoft.Win32;
namespace Servidor.Models
{
    class ClientHandler
    {
        private TcpClient client;
        private int clientID;

        //instancia o controller de registo
        ControllerFormRegistar controllerRegistar = new ControllerFormRegistar();

        //instancia o controller de login
        ControllerFormLogin controllerLogin = new ControllerFormLogin();

        public ClientHandler(TcpClient client, int clientID)
        {
            this.client = client;
            this.clientID = clientID;
        }

        public void Handle()
        {
            Thread thread = new Thread(threadHandler);
            thread.Start();
        }

        //Vai tratar os threads 
        public void threadHandler()
        {
            NetworkStream networkStream = this.client.GetStream();
            ProtocolSI protocolSI = new ProtocolSI();

            while (protocolSI.GetCmdType() != ProtocolSICmdType.EOT)
            {
                int bytesRead = networkStream.Read(protocolSI.Buffer, 0, protocolSI.Buffer.Length);
                //ACK (acknowledgement)
                byte[] ack;

                switch (protocolSI.GetCmdType())
                {
                    //ENVIA DADOS
                    case ProtocolSICmdType.DATA:
                        //ESCREVER MSG DO CLIENTE NA CONSOLA
                        Console.WriteLine("Client " + clientID + ": " + protocolSI.GetStringFromData());

                        byte[] dados = protocolSI.GetData();

                        switch (dados[0])
                        {
                            case 1:
                                //realiza o registo do utilizador
                                Console.WriteLine("Registo de utilizador iniciado pelo cliente {0}", clientID);

                                //obter dados sem o primero byte
                                byte[] dadosRegistarSemTipo = dados.Skip(1).ToArray(); // Pula o primeiro byte que é o tipo de comando

                                Usuario user = ControllerSerializar.DeserializaDeArrayBytes<Usuario>(dadosRegistarSemTipo);
                                string mensagem = controllerRegistar.Registar(user);
                                Console.WriteLine("Mensagem de registo: " + mensagem);

                                //mandar msg para o utilizador a enviar uma resposta a dizer se foi bem sucedido ou nao

                                byte[] resposta = protocolSI.Make(ProtocolSICmdType.DATA, mensagem);
                                networkStream.Write(resposta, 0, resposta.Length);

                                break;
                            case 2:
                                //realiza o login do utilizador e mostra no cmd
                                Console.WriteLine("Login iniciado pelo cliente {0}", clientID);

                                //obter dados sem o primero byte
                                byte[] dadosLoginSemTipo = dados.Skip(1).ToArray();

                                //Converte os dados recebidos em uma string
                                string dadosLoginString = Encoding.UTF8.GetString(dadosLoginSemTipo);

                                //Separa os dados em partes usando o separador de nova linha
                                string[] parts = dadosLoginString.Split('\n');
                                string username = parts[0];
                                string SaltePasswordHashString = parts[1];

                                //Converte o SaltePasswordHashString de volta para um array de bytes
                                byte[] profPic = controllerLogin.verifyLogin(username, SaltePasswordHashString);

                                // Se o profPic for null, significa que o login falhou
                                byte[] respostaProfilePic = protocolSI.Make(ProtocolSICmdType.DATA, profPic);
                                networkStream.Write(respostaProfilePic, 0, respostaProfilePic.Length);

                                break;
                            case 3:
                                //comcepica
                                break;
                            default:
                                throw new Exception("Tipo de comando desconhecido.");
                        }
                        ack = protocolSI.Make(ProtocolSICmdType.ACK);
                        networkStream.Write(ack, 0, ack.Length);
                        break;
                    //CASO O CLIENTE ENVIE EOT (FIM DA TRANSMISSÃO)
                    case ProtocolSICmdType.EOT:
                        Console.WriteLine("Ending Thread from client {0}", clientID);
                        ack = protocolSI.Make(ProtocolSICmdType.ACK);
                        networkStream.Write(ack, 0, ack.Length);
                        break;
                }
            }
            networkStream.Close();
            client.Close();

        }
    }
}