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
        }

        private void btnEnviarMsg_Click(object sender, EventArgs e)
        {
            //Criação da mensagem (no caso o que for escrito na textbox) e depois de enviar ele limpa
            string msg = tbxMsg.Text;
            tbxMsg.Clear();

            //Cria o packet que vai fazer o envio da mensagem
            byte[] packet = protSI.Make(ProtocolSICmdType.DATA, msg); 

            //Faz o envio da mensagem usando a fução networkStream.Write
            //que precisa de um array de bytes + sua posição inicial que é 0 + o seu tamanho final(.Length para pegar a sua posição final)
            networkStream.Write(packet, 0, packet.Length);


            //Fazemos um ciclo While para esperar a resposta do cliente
            while (protSI.GetCmdType() != ProtocolSICmdType.ACK)  
            {
                //
                networkStream.Read(protSI.Buffer, 0, protSI.Buffer.Length);
            }
        }


    }
}
