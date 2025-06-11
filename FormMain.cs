using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using EI.SI;

namespace Projeto_TS
{
    public partial class FormMain: Form
    {
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


        public FormMain(string username, Image profilePicture)
        {
            InitializeComponent();
            //inicializou/instanciou o endpoint que é a combinação do ip da propria maquina por isso loopback + PORT
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Loopback, PORT);

            //criação do cliente e conecta ao endpoint 
            client = new TcpClient();
            client.Connect(endPoint);

            //Passagem de informação
            networkStream = client.GetStream();

            rsa = new RSACryptoServiceProvider(2048);

            protSI = new ProtocolSI();

            _Username = username; //username terá o nome do utilizador que foi passado pelo form de login
            _ProfilePicture = profilePicture; //profilePicture terá a imagem do perfil que foi passada pelo form de login

            lbServerIP.Text = "Server IP: " + endPoint.ToString(); //mostra o ip do servidor no label
            lbNome.Text = _Username; //mostra o nome do utilizador no label
            pbUserImage.Image = _ProfilePicture; //mostra a imagem do perfil no picturebox

            //ENVIA O USERNAME
            byte[] userPacket = protSI.Make(ProtocolSICmdType.USER_OPTION_1, _Username);
            networkStream.Write(userPacket, 0, userPacket.Length);
            networkStream.Read(protSI.Buffer, 0, protSI.Buffer.Length); // Espera resposta do servidor

            // ENVIA A CHAVE PÚBLICA
            string publicKey = rsa.ToXmlString(false);
            byte[] keyPacket = protSI.Make(ProtocolSICmdType.USER_OPTION_2, publicKey);
            networkStream.Write(keyPacket, 0, keyPacket.Length);
            networkStream.Read(protSI.Buffer, 0, protSI.Buffer.Length);

            // Inicia a thread que irá receber mensagens do servidor
            tReceber = new Thread(ReceberMensagens);
            tReceber.IsBackground = true;
            tReceber.Start();

            // Solicita histórico de mensagens ao servidor
            byte[] pedirHistorico = protSI.Make(ProtocolSICmdType.USER_OPTION_3);
            networkStream.Write(pedirHistorico, 0, pedirHistorico.Length);
        }

        private void btnEnviarMsg_Click(object sender, EventArgs e)
        {

            string msg = tbxMsg.Text;
            tbxMsg.Clear();

            // Cria uma thread para enviar a mensagem o programa travava ao enviar a mensagem
            Thread t = new Thread(() =>
            {
                try
                {
                    byte[] packet = protSI.Make(ProtocolSICmdType.DATA, msg);
                    networkStream.Write(packet, 0, packet.Length);

                    bool ackRecebido = false;
                    while (!ackRecebido)
                    {
                        int bytesRead = networkStream.Read(protSI.Buffer, 0, protSI.Buffer.Length);
                        if (bytesRead == 0)
                        {
                            // Conexão fechada pelo servidor
                            MessageBox.Show("Conexão encerrada pelo servidor.");
                            this.Invoke((MethodInvoker)delegate { this.Close(); });
                            return;

                        }
                        // Verifica se recebeu ACK
                        if (protSI.GetCmdType() == ProtocolSICmdType.ACK)
                        {
                            ackRecebido = true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Erro ao enviar ou conexão encerrada
                    MessageBox.Show("Erro ao enviar mensagem ou conexão encerrada: " + ex.Message);
                    this.Invoke((MethodInvoker)delegate { this.Close(); });
                }
            });
            t.IsBackground = true;
            t.Start();
        }


        private void ReceberMensagens()
        {
            var protSIReceber = new ProtocolSI(); // Instância local
            try
            {
                while (true)
                {
                    int bytesRead = networkStream.Read(protSIReceber.Buffer, 0, protSIReceber.Buffer.Length);

                    var cmd = protSIReceber.GetCmdType();
                    if (cmd == ProtocolSICmdType.DATA)
                    {
                        string mensagem = protSIReceber.GetStringFromData();

                        if (mensagem.Contains(":")) // verifica se a mensagem contem o formato username: mensagem
                        {
                            this.Invoke((MethodInvoker)delegate
                            {
                                textBox1.AppendText(mensagem + Environment.NewLine); // Adiciona a mensagem ao TextBox
                            });
                        }
                    }
                }
            }
            catch (Exception)
            {
                //falta excecao
            }
        }

        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            Environment.Exit(0);
        }
    }
}
