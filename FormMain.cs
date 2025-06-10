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
        private const int PORT = 10000;

        //Criação das variaveis com suas classes
        NetworkStream networkStream;
        ProtocolSI protSI;
        TcpClient client;
        RSACryptoServiceProvider rsa;
        Thread tReceber;


        public FormMain(int id)
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

            string username = null; //vem da query
            pBImagem.Image = null; //vem da query

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

        private void label2_Click(object sender, EventArgs e)
        {

        }
    }
}
