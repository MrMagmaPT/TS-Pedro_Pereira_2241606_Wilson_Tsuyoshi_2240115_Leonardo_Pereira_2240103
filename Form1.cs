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
using EI.SI;

namespace Projeto_TS_Pedro_Pereira_2241606_Wilson_Tsuyoshi_2240115
{
    public partial class Form1: Form
    {
        private const int PORT = 10000;
        NetworkStream networkStream;
        ProtocolSI protSI = new ProtocolSI();
        TcpClient client;
        public Form1()
        {
            InitializeComponent();
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Loopback, PORT);
            client = new TcpClient();
            client.Connect(endPoint);
            networkStream = client.GetStream();
            ProtocolSI prot = new ProtocolSI();
        }

        private void btnEnviarMsg_Click(object sender, EventArgs e)
        {
            string msg = tbxMsg.Text; //cria msg
            tbxMsg.Clear();
            byte[] packet = protSI.Make(ProtocolSICmdType.DATA, msg); //crai packet de envio
            networkStream.Write(packet, 0, packet.Length); //Envia o packet

            while (protSI.GetCmdType() != ProtocolSICmdType.ACK)  //espera por resposta
            {
                networkStream.Read(protSI.Buffer, 0, protSI.Buffer.Length);
            }
        }







    }
}
