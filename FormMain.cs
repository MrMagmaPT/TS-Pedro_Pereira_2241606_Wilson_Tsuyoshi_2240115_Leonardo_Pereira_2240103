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
        //=====================================================================================
        // RSA para assinar
        private RSACryptoServiceProvider rsaSign;
        //=====================================================================================
        Thread tReceber;

        string _Username;
        Image _ProfilePicture;

        FormLogin loginform;

        ClientInfo clienteCompleto;

        public FormMain(string username, byte[] profPic, ClientInfo clientOld, NetworkStream nsOld, ProtocolSI protocolSIOld, FormLogin formLogin, RSACryptoServiceProvider rsaPriv)
        {
            InitializeComponent();
            //instancia o form login
            loginform = formLogin;

            //inicializou/instanciou o endpoint que é a combinação do ip da propria maquina por isso loopback + PORT
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Loopback, PORT);

            //criação do cliente e conecta ao endpoint 
            client = clientOld.cliente;
            clienteCompleto = clientOld;

            //Passagem de informação
            networkStream = nsOld;


            //=====================================================================================

            //instancia a chave publica
            rsaSign = rsaPriv;

            //=====================================================================================
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
            tReceber.Start();
        }


        private void btnEnviarMsg_Click(object sender, EventArgs e)
        {

            lblPalavras.Text = "";
            if (string.IsNullOrWhiteSpace(tbxMsg.Text)) //verifica se a textbox está vazia ou só com espaços em branco
            {
                MessageBox.Show("Por favor, escreva uma mensagem antes de enviar!"); //se estiver, mostra uma mensagem de erro
                return; //e sai da função
            } 
            else if (Encoding.UTF8.GetByteCount(tbxMsg.Text) > 999) 
            {
                MessageBox.Show("A mensagem nao deve ter mais de 999 caracteres!"); //se estiver, mostra uma mensagem de erro
                return; //e sai da função
            } 
            else
            {
                //Criação da mensagem (no caso o que for escrito na textbox) e depois de enviar ele limpa
                string msg = tbxMsg.Text;
                tbxMsg.Clear();

                Thread t = new Thread(() =>
                {
                    try
                    {
                        byte[] messageBytes = Encoding.UTF8.GetBytes(msg); //Converte a mensagem para bytes

                        // 1. Assinar a mensagem

                        // 1. Gerar hash da mensagem
                        byte[] hash;
                        using (SHA256 sha256 = SHA256.Create())
                        hash = sha256.ComputeHash(messageBytes);

                        // 2. Assinar a hash
                        byte[] assinatura = rsaSign.SignHash(hash, CryptoConfig.MapNameToOID("SHA256"));

                        // 3. Juntar assinatura e mensagem
                        using (MemoryStream ms = new MemoryStream())
                        {
                            ms.Write(BitConverter.GetBytes(assinatura.Length), 0, 4); // 4 bytes: tamanho da assinatura
                            ms.Write(assinatura, 0, assinatura.Length);               // assinatura
                            ms.Write(messageBytes, 0, messageBytes.Length);           // mensagem original
                            messageBytes = ms.ToArray();
                        }

                        //cifra a mensagem
                        byte[] packetCifrado = cifrarMensagem(messageBytes, clienteCompleto);

                        //endereça a mensagem
                        MessageTypeEnum messageType = new MessageTypeEnum(); //instanciação do enum de tipo de mensagem
                        MessageTypeEnum.MessageType tipo = MessageTypeEnum.MessageType.SendMessage; //enum de prefixo para registar

                        byte[] mensagemComTipo = messageType.CreateMessage(tipo, packetCifrado); //criação da mensagem com o tipo de mensagem e o array de bytes do utilizador

                        //Cria o packet que vai fazer o envio da mensagem
                        byte[] packet = protSI.Make(ProtocolSICmdType.DATA, mensagemComTipo);

                        //envia a mensagem
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
                            byte[] mensagemCifrada = protocolSIReceber.GetData();

                            //decifrar dados recebidos (mensagem)
                            byte[] mensagemDecifrada = decifrarMensagemRecebida(mensagemCifrada);

                            string mensagem = Encoding.UTF8.GetString(mensagemDecifrada);

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
            if (networkStream.CanWrite == true) { //verificamos se ainda é possivel escrever no ns
                networkStream.Write(EOT, 0, EOT.Length);

                networkStream.Read(protSI.Buffer, 0, protSI.Buffer.Length);
                running = false;
            }

            // Define a flag para parar o loop de recebimento de mensagens
            //quando receber o ack do servidor depois do eot fecha as coneões e o formulário

            networkStream.Close(); // Fecha o NetworkStream
            client.Close();
            loginform.txbPassword.Text = "";
            loginform.txbUsername.Text = "";
            loginform.Show();
        }

        public byte[] cifrarMensagem(byte[] mensagem, ClientInfo cliente)
        {
            //encriptar mensagem
            byte[] mensagemCifrada;
            using (Aes aes = Aes.Create())
            {
                // Configurar a chave AES
                aes.Key = cliente.chaveSimetrica;

                // Criar o cifrador
                using (ICryptoTransform encryptor = aes.CreateEncryptor())
                using (MemoryStream ms = new MemoryStream())
                {
                    // Escrever o IV primeiro (necessário para decifração)
                    ms.Write(aes.IV, 0, aes.IV.Length);

                    // Cifrar os dados
                    using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    {
                        cs.Write(mensagem, 0, mensagem.Length);
                        cs.FlushFinalBlock();
                    }

                    return mensagemCifrada = ms.ToArray();
                }
            }
        }

        public byte[] decifrarMensagemRecebida(byte[] dadosCifrados)
        {
            try
            {
                byte[] iv = new byte[16];
                Buffer.BlockCopy(dadosCifrados, 0, iv, 0, 16);

                byte[] dadosCifradosSemIV = new byte[dadosCifrados.Length - 16];
                Buffer.BlockCopy(dadosCifrados, 16, dadosCifradosSemIV, 0, dadosCifradosSemIV.Length);

                byte[] chaveSimetricaParaDecifrar = clienteCompleto.chaveSimetrica;
                if (chaveSimetricaParaDecifrar == null)
                    return dadosCifrados;

                using (Aes aes = Aes.Create())
                {
                    aes.Key = chaveSimetricaParaDecifrar;
                    aes.IV = iv;

                    using (MemoryStream ms = new MemoryStream())
                    using (CryptoStream cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(dadosCifradosSemIV, 0, dadosCifradosSemIV.Length);
                        cs.FlushFinalBlock();
                        return ms.ToArray();
                    }
                }
            }
            catch (CryptographicException ex)
            {
                Console.WriteLine($"[Servidor] - Erro ao decifrar: {ex.Message}");
                return dadosCifrados;
            }
        }

        private byte[] AssinarMensagem(byte[] mensagem)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hash = sha256.ComputeHash(mensagem);
                return rsaSign.SignHash(hash, CryptoConfig.MapNameToOID("SHA256"));
            }
        }

        private void tbxMsg_TextChanged(object sender, EventArgs e)
        {
            int charCount = Encoding.UTF8.GetByteCount(tbxMsg.Text);
            int charRestantes = 999 - charCount;
            if (charRestantes < 50)
            {
                lblPalavras.Text = "Characteres restantes: " + charRestantes;
            }else
            {
                lblPalavras.Text = "";
            }
            if (charRestantes < 0)
            {
                btnEnviarMsg.Enabled = false;
            }
            else
            {
                btnEnviarMsg.Enabled = true;
            }
        }
    }
}
