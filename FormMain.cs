using EI.SI;
using Projeto_TS_Pedro_Pereira_2241606_Wilson_Tsuyoshi_2240115.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Projeto_TS
{
    public partial class FormMain: Form
    {
        private volatile bool running = false;

        //definição do port
        private const int PORT = 12345;

        //Criação das variaveis com suas classes
        NetworkStream networkStream;
        ProtocolSI protSI;
        TcpClient client;
        RSACryptoServiceProvider rsa;

        Thread tReceber;

        string _Username;
        Image _ProfilePicture;


        public FormMain(string username, byte[] profPic, TcpClient clientOld, NetworkStream nsOld, ProtocolSI protocolSIOld)
        {
            InitializeComponent();
            //inicializou/instanciou o endpoint que é a combinação do ip da propria maquina por isso loopback + PORT
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Loopback, PORT);

            //criação do cliente e conecta ao endpoint 
            client = clientOld;

            //Passagem de informação
            networkStream = nsOld;
                
            rsa = new RSACryptoServiceProvider(2048);

            protSI = protocolSIOld;

            Image profilePicture = null; //converte o byte[] para uma imagem
            if (profPic != null && profPic.Length != 0)
            {
                using (MemoryStream ms = new MemoryStream(profPic))
                {
                    profilePicture = Image.FromStream(ms);
                }
            } else
            {
                MessageBox.Show(Convert.ToBase64String(profPic));
            }


            _Username = username; //username terá o nome do utilizador que foi passado pelo form de login
            _ProfilePicture = profilePicture; //profilePicture terá a imagem do perfil que foi passada pelo form de login

            lbServerIP.Text = "Server IP: " + endPoint.ToString(); //mostra o ip do servidor no label
            lbNome.Text = "Nome: " + _Username; //mostra o nome do utilizador no label
            pbUserImage.Image = _ProfilePicture; //mostra a imagem do perfil no picturebox

            running = true;
            tReceber = new Thread(ReceberMensagens);
            tReceber.IsBackground = true;
            
        }


        private void btnEnviarMsg_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tbxMsg.Text)) //verifica se a textbox está vazia ou só com espaços em branco
            {
                MessageBox.Show("Por favor, escreva uma mensagem antes de enviar!"); //se estiver, mostra uma mensagem de erro
                return; //e sai da função
            } else
            {
                //Criação da mensagem (no caso o que for escrito na textbox) e depois de enviar ele limpa
                string msg = tbxMsg.Text;
                tbxMsg.Clear();

                Thread t = new Thread(() =>
                {
                    try
                    {
                        byte[] messageBytes = Encoding.UTF8.GetBytes(msg); //Converte a mensagem para bytes

                        MessageTypeEnum messageType = new MessageTypeEnum(); //instanciação do enum de tipo de mensagem
                        MessageTypeEnum.MessageType tipo = MessageTypeEnum.MessageType.SendMessage; //enum de prefixo para registar

                        byte[] mensagemComTipo = messageType.CreateMessage(tipo, messageBytes); //criação da mensagem com o tipo de mensagem e o array de bytes do utilizador

                        //Cria o packet que vai fazer o envio da mensagem
                        byte[] packet = protSI.Make(ProtocolSICmdType.DATA, mensagemComTipo);

                        networkStream.Write(packet, 0, packet.Length);

                        bool ackRecebido = false;
                        while (ackRecebido != true)
                        {
                            int bytesRead = networkStream.Read(protSI.Buffer, 0, protSI.Buffer.Length);

                            if (bytesRead == 0) 
                            {
                                // Conexão fechada pelo servidor
                                MessageBox.Show("Conexão encerrada pelo servidor.");
                                this.Invoke((MethodInvoker)delegate { this.Close(); });
                                // Fecha o formulário atual de forma segura pelo thread
                                return;
                            }
                            // Verifica se o comando recebido é ACK
                            if (protSI.GetCmdType() == ProtocolSICmdType.ACK)
                            {
                                ackRecebido = true;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Erro ao enviar a mensagem: " + ex.Message);
                        // Fecha o formulário atual de forma segura pelo thread
                        this.Invoke((MethodInvoker)delegate { this.Close(); });
                    }
                });
                t.IsBackground = true; //Define o thread como background para que ele não impeça o fechamento do programa
                t.Start(); //Inicia o thread para enviar a mensagem
            }
        }

        private void ReceberMensagens()
        {
            var protocolSIReceber = new ProtocolSI();
            try
            {
                while (running)
                {
                    try
                    {
                        int bytesRead = networkStream.Read(protocolSIReceber.Buffer, 0, protocolSIReceber.Buffer.Length);

                        if (bytesRead == 0)
                        {
                            // Conexão fechada pelo servidor
                            break;
                        }

                        var cmd = protocolSIReceber.GetCmdType();

                        // Só processa se for DATA
                        if (cmd == ProtocolSICmdType.DATA)
                        {
                            string mensagem = protocolSIReceber.GetStringFromData();
                            if (mensagem.Contains("["))
                            {
                                this.Invoke((MethodInvoker)delegate
                                {
                                    txbChat.AppendText(mensagem + Environment.NewLine);
                                });
                            }
                        }
                        // Se for EOT, encerra o loop
                        else if (cmd == ProtocolSICmdType.EOT)
                        {
                            break;
                        }
                        // Se for ACK, apenas continue
                        else if (cmd == ProtocolSICmdType.ACK)
                        {
                            continue;
                        }
                        // Se for outro comando, ignore ou trate conforme necessário
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Erro ao receber mensagens: " + ex.Message);
                    }
          
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao iniciar a thread de receber mensagens: " + ex.Message);
                this.Invoke((MethodInvoker)delegate { this.Close(); });
            }
        }

        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            byte[] EOT = protSI.Make(ProtocolSICmdType.EOT);
            networkStream.Write(EOT, 0, EOT.Length);
            running = false; // Define a flag para parar o loop de recebimento de mensagens
            //quando receber o ack do servidor depois do eot fecha as coneões e o formulário
            while (protSI.GetCmdType() != ProtocolSICmdType.ACK)
            {
                // Lê a resposta do servidor
                networkStream.Read(protSI.Buffer, 0, protSI.Buffer.Length);
            }
            networkStream.Close(); // Fecha o NetworkStream
            client.Close();
            
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            
        }
    }
}
