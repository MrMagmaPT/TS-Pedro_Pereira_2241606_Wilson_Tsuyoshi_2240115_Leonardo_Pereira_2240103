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
using System.Security.Cryptography;

namespace Projeto_TS
{
    public partial class FormLogin: Form
    {
        FormRegistar formRegistar = new FormRegistar();
        FormMain formMain;

        //instancia do protocolo SI para enviar e receber mensagens
        ProtocolSI protocolSI;
        NetworkStream ns;
        TcpClient client;
        private const int PORT = 12345;

        //=====================================================================================

        //instacia crypto ervice provider
        RSACryptoServiceProvider rsa;

        string publicKey;

        string chaveSimetrica;

        //=====================================================================================

        public FormLogin()
        {
            InitializeComponent();

            // Inicializa o provedor RSA com uma chave de 2048 bits
            rsa = new RSACryptoServiceProvider(2048);

            // Obtém a chave pública em formato XML(false devolve a chave publica)
            publicKey = rsa.ToXmlString(false);
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
            MessageTypeEnum messageType = new MessageTypeEnum();

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Por favor, preencha todos os campos.");
                return;
            }

            string passwordHash = Hash.HashPassword(password); // Gera o hash da senha com um salt de 1000 iterações



            //Conecta ao servidor para fazer o login e a chave simetrica/publica
            IPEndPoint endpoint = new IPEndPoint(IPAddress.Loopback, PORT); //define o ponto final do servidor (localhost e porta 12345)

            client = new TcpClient(); // Cria uma nova instância do cliente TCP

            client.Connect(endpoint); //conecta o cliente ao servidor

            ns = client.GetStream(); // Obtém o network stream do cliente para enviar e receber dados
            protocolSI = new ProtocolSI(); // Instanciação do protocolo SI


            //enviar chave publica - servidor
            byte[] pubKeyBytes = Encoding.UTF8.GetBytes(publicKey);

            MessageTypeEnum.MessageType tipoChave = MessageTypeEnum.MessageType.SendPublicKey;

            byte[] chavePubComTipo = messageType.CreateMessage(tipoChave, pubKeyBytes);

            byte[] packetChave = protocolSI.Make(ProtocolSICmdType.DATA, chavePubComTipo);

            //Envia a chave publica para o servidor
            ns.Write(packetChave, 0, packetChave.Length);

            // recebe a chave simetrica do servidor
            ns.Read(protocolSI.Buffer, 0, protocolSI.Buffer.Length);
            //ns.Read(protocolSI.Buffer, 0, protocolSI.Buffer.Length);
            byte[] chaveSimetricaCifrada = protocolSI.GetData(); //obtem a chave simetrica cifrada com a chave publica


            //Decifrar chave simétrica usando chave privada RSA
            byte[] chaveSimetricaDecifrada;
            try
            {

                chaveSimetricaDecifrada = rsa.Decrypt(chaveSimetricaCifrada, true);

                //Guardar a chave simétrica decifrada (em Base64 para fácil armazenamento)
                chaveSimetrica = Convert.ToBase64String(chaveSimetricaDecifrada);

            }
            catch (CryptographicException ex)
            {
                MessageBox.Show("Erro ao decifrar a chave simétrica: " + ex.Message);
                return;
            }

            //Cria a string de login e enviar
            string loginstring = username + "\n" + passwordHash;

            byte[] loginBytes = Encoding.UTF8.GetBytes(loginstring);

            
            //encriptar mensagem
            byte[] mensagemCifrada;
            using (Aes aes = Aes.Create())
            {
                // Configurar a chave AES (convertida de Base64 para bytes)
                aes.Key = Convert.FromBase64String(chaveSimetrica);


                // Criar o cifrador (usará configurações padrão: CBC mode e PKCS7 padding)
                using (ICryptoTransform encryptor = aes.CreateEncryptor())
                using (MemoryStream ms = new MemoryStream())
                {
                    // Escrever o IV primeiro (necessário para decifração)
                    ms.Write(aes.IV, 0, aes.IV.Length);

                    // Cifrar os dados
                    using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    {
                        cs.Write(loginBytes, 0, loginBytes.Length);
                        cs.FlushFinalBlock();
                    }

                    mensagemCifrada = ms.ToArray();
                }
            }

            MessageTypeEnum.MessageType tipoLogin = MessageTypeEnum.MessageType.Login;
            byte[] mensagemComTipoCifrada = messageType.CreateMessage(tipoLogin, mensagemCifrada);

            byte[] packetLogin = protocolSI.Make(ProtocolSICmdType.DATA, mensagemComTipoCifrada);

            //Envia o comando de login para o servidor
            ns.Write(packetLogin, 0, packetLogin.Length); 

            // recebe a confimação do login (ou rejeição) baseado naprofile pic
            while (protocolSI.GetCmdType() != ProtocolSICmdType.ACK || protocolSI.GetCmdType() != ProtocolSICmdType.EOT)
            {
                // Lê a resposta do servidor
                ns.Read(protocolSI.Buffer, 0, protocolSI.Buffer.Length);
                byte[] profPicCifrada = protocolSI.GetData(); // Obtém os dados da resposta da stream

                //decifrar profilepic
                byte[] profPic = decifrarProfPic(profPicCifrada,Convert.FromBase64String(chaveSimetrica));

                if (profPic[0] != 48)
                {//48 e o valor associado a NULL em vez de verificar o byte[0] == 0, verificamos se o primeiro byte é diferente de 48
                    MessageBox.Show("Sucesso no login do user: " + username, "Sucesso!", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    ClientInfo clienteCompleto = new ClientInfo(publicKey, client, Convert.FromBase64String(chaveSimetrica));

                    formMain = new FormMain(username, profPic, clienteCompleto, ns, protocolSI, this);
                    formMain.Show(); // Mostra o formulário principal
                    this.Hide(); // Esconde o formulário de login
                    return;
                }
                else
                {
                    MessageBox.Show("Login falhou as credenciais não estão corretas: " + username, "Falhou!", MessageBoxButtons.OK, MessageBoxIcon.Error);

                    txbPassword.Text = String.Empty;
                    txbUsername.Text = String.Empty;
                    return;
                }

            };
        }

        public byte[] decifrarProfPic(byte[] dadosCifrados,byte[] chavesimetrica)
        {
            if (dadosCifrados[0] != 48) 
            {
                try
                {
                    byte[] iv = new byte[16];
                    Buffer.BlockCopy(dadosCifrados, 0, iv, 0, 16);

                    byte[] dadosCifradosSemIV = new byte[dadosCifrados.Length - 16];
                    Buffer.BlockCopy(dadosCifrados, 16, dadosCifradosSemIV, 0, dadosCifradosSemIV.Length);

                    byte[] chaveSimetricaParaDecifrar = chavesimetrica;
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
            } else
            {
                return dadosCifrados;
            }
        }
    }
}
