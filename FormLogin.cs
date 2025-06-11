using EI.SI;
using Projeto_TS_Pedro_Pereira_2241606_Wilson_Tsuyoshi_2240115;
using Projeto_TS_Pedro_Pereira_2241606_Wilson_Tsuyoshi_2240115.Controllers;
using Projeto_TS_Pedro_Pereira_2241606_Wilson_Tsuyoshi_2240115.Models;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;
using static Projeto_TS_Pedro_Pereira_2241606_Wilson_Tsuyoshi_2240115.Models.MessageTypeEnum;

namespace Projeto_TS
{
    public partial class FormLogin: Form
    {
        FormRegistar formRegistar = new FormRegistar();


        //instancia do protocolo SI para enviar e receber mensagens
        ProtocolSI protocolSI;
        NetworkStream ns;
        TcpClient client;
        private const int PORT = 12345;


        public FormLogin()
        {
            InitializeComponent();
        }

        private void btnRegistar_Click(object sender, EventArgs e)
        {
            //verifica se o formRegistar ainda está intanciado, se não estiver, cria uma nova 
            if (formRegistar.IsDisposed)
            {
                formRegistar = new FormRegistar();
            }
            formRegistar.Show();
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            string password = txbPassword.Text;
            string username = txbUsername.Text;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Por favor, preencha todos os campos.");
                return;
            }
            //Gera Hash da password
            string passwordHash = Hash.HashPassword(password); // Gera o hash da senha com um salt de 1000 iterações


            //Conecta ao servidor para fazer o login
            IPEndPoint endpoint = new IPEndPoint(IPAddress.Loopback, PORT); // Define o ponto final do servidor (localhost e porta 12345)

            client = new TcpClient(); // Cria uma nova instância do cliente TCP

            client.Connect(endpoint); // Conecta ao servidor

            ns = client.GetStream(); // Obtém o network stream do cliente para enviar e receber dados
            protocolSI = new ProtocolSI(); // Instanciação do protocolo SI

            // Cria o comando de login

            // Create the login string
            string loginstring = username + "\n" + passwordHash;

            byte[] loginBytes = Encoding.UTF8.GetBytes(loginstring);

            MessageTypeEnum messageType = new MessageTypeEnum();
            MessageTypeEnum.MessageType tipo = MessageTypeEnum.MessageType.Login;
            byte[] mensagemComTipo = messageType.CreateMessage(tipo, loginBytes);

            byte[] packet = protocolSI.Make(ProtocolSICmdType.DATA, mensagemComTipo);

            ns.Write(packet, 0, packet.Length); // Envia o comando de login para o servidor

            while (protocolSI.GetCmdType() != ProtocolSICmdType.ACK)
            {
                // Lê a resposta do servidor
                ns.Read(protocolSI.Buffer, 0, protocolSI.Buffer.Length);
                byte[] profPic = protocolSI.GetData(); // Obtém os dados da resposta
                if (profPic == null)
                {
                    MessageBox.Show("Erro no login", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                else if (Encoding.UTF8.GetString(profPic) == "0")
                {
                    //envia para o form chat as infromacoes do utilizador
                } else
                {
                    MessageBox.Show("Sucesso no login do user: " + username, "Sucesso!", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }
    }
}
