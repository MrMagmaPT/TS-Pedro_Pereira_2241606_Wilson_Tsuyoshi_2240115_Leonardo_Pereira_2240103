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
using System.Diagnostics.Eventing.Reader;
namespace Servidor.Models
{
    class ClientHandler
    {
        //Lista de clientes conectados
        static List<TcpClient> clientes = new List<TcpClient>();

        private TcpClient ultimoCliente;

        string username;

        //instancia o controller de registo
        ControllerFormRegistar controllerRegistar = new ControllerFormRegistar();

        //instancia o controller de login
        ControllerFormLogin controllerLogin = new ControllerFormLogin();

        public ClientHandler(TcpClient client)
        {
            this.ultimoCliente = client;
        }

        public void Handle()
        {
            Thread thread = new Thread(threadHandler);
            thread.Start();
        }

        //Vai tratar os threads 
        public void threadHandler()
        {
            NetworkStream networkStream = this.ultimoCliente.GetStream();
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

                        byte[] dados = protocolSI.GetData();

                        switch (dados[0])
                        {
                            case 1:
                                //obter dados sem o primero byte
                                byte[] dadosRegistarSemTipo = dados.Skip(1).ToArray(); // Pula o primeiro byte que é o tipo de comando

                                Usuario user = ControllerSerializar.DeserializaDeArrayBytes<Usuario>(dadosRegistarSemTipo); // Desserializa os dados recebidos para um objeto Usuario
                                string mensagem = controllerRegistar.Registar(user); //enviar os dados do usuario para o controller de registo e obter a mensagem de resposta
                                username = user.username; //obter o username do utilizador para mostrar no cmd
                                //realiza o registo do utilizador
                                Console.WriteLine("[Servidor] - Registo de utilizador iniciado pelo cliente: " + username);

                                //mandar msg para o utilizador a enviar uma resposta a dizer se foi bem sucedido ou nao

                                byte[] resposta = protocolSI.Make(ProtocolSICmdType.DATA, mensagem);
                                networkStream.Write(resposta, 0, resposta.Length);

                                break;
                            case 2:
                                byte[] respostaProfilePic = null;
                                //obter dados sem o primero byte
                                byte[] dadosLoginSemTipo = dados.Skip(1).ToArray();

                                //Converte os dados recebidos em uma string
                                string dadosLoginString = Encoding.UTF8.GetString(dadosLoginSemTipo);

                                //Separa os dados em partes usando o separador de nova linha
                                string[] parts = dadosLoginString.Split('\n');
                                username = parts[0];
                                string SaltePasswordHashString = parts[1];

                                //realiza o login do utilizador e mostra no cmd
                                Console.WriteLine("[Servidor] - Login iniciado pelo cliente: " + username);

                                //Converte o SaltePasswordHashString de volta para um array de bytes
                                byte[] profPic = controllerLogin.verifyLogin(username, SaltePasswordHashString);

                                if (profPic == null)
                                {
                                    //Se o utilizador nao existir, envia uma mensagem de erro
                                    Console.WriteLine("[Servidor] - Login falhou para o cliente: " + username);
                                    respostaProfilePic = protocolSI.Make(ProtocolSICmdType.EOT);
                                }
                                else
                                {
                                    //Se o utilizador existir, envia a imagem do perfil
                                    Console.WriteLine("[Servidor] - Login bem sucedido para o cliente: " + username);
                                    respostaProfilePic = protocolSI.Make(ProtocolSICmdType.DATA, profPic); // Envia a imagem do perfil

                                    clientes.Add(ultimoCliente); //Adiciona o cliente à lista de clientes conectados
                                }

                                networkStream.Write(respostaProfilePic, 0, respostaProfilePic.Length);

                                break;
                                
                            case 3:
                                //obter dados sem o primero byte
                                byte[] mensagemSemTipo = dados.Skip(1).ToArray();

                                //Converte os dados recebidos em uma string
                                string mensagemString = "[" + username + "] - " + Encoding.UTF8.GetString(mensagemSemTipo);

                                //mostra a mensagem recebida no cmd
                                Console.WriteLine(mensagemString);

                                EnviarParaTodos(mensagemString); //Envia a mensagem para todos os clientes conectados
                                break;
                            default:
                                throw new Exception("Tipo de comando desconhecido.");
                        }
                        ack = protocolSI.Make(ProtocolSICmdType.ACK);
                        networkStream.Write(ack, 0, ack.Length);
                        break;
                        
                    //CASO O CLIENTE ENVIE EOT (FIM DA TRANSMISSÃO)
                    case ProtocolSICmdType.EOT:
                        Console.WriteLine("[Servidor] - A terminar a ligação com o cliente: " + username);
                        ack = protocolSI.Make(ProtocolSICmdType.ACK);
                        networkStream.Write(ack, 0, ack.Length);
                        break;
                }
            }
            //ermina os recursos quando termina a transmissão
            networkStream.Close();
            ultimoCliente.Close();
        }

        public void EnviarParaTodos(string mensagem)
        {
            ProtocolSI protocolSI = new ProtocolSI();
            byte[] mensagemBytes = protocolSI.Make(ProtocolSICmdType.DATA, mensagem);

            // usamos uma copia da lista para evitar problemas de concorrência ou modificações a lista enquanto iteramos sobre a mesma
            var clientesCopy = clientes.ToList();

            foreach (TcpClient cliente in clientesCopy)
            {
                try
                {
                    NetworkStream ns = cliente.GetStream();
                    ns.Write(mensagemBytes, 0, mensagemBytes.Length);
                }
                catch (Exception)
                { 
                    //Caso nao chegue a mensagem ao cliente, remove o cliente da lista e fecha a conexão, considera tambem como cliente desconectado
                    clientes.Remove(cliente);
                    Console.WriteLine("[Servidor] - A terminar a ligação com o cliente: " + username);
                    cliente.Close();
                }
            }
        }
    }
}