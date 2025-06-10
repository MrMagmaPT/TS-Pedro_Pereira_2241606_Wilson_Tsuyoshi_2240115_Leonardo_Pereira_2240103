using EI.SI;
using Projeto_TS;
using Projeto_TS_Pedro_Pereira_2241606_Wilson_Tsuyoshi_2240115;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Projeto_TS
{
    using System;
    using System.IO;
    using System.Net.Sockets;
    using System.Security.Cryptography;
    using System.Text;
    using System.Windows.Forms;
    using EI.SI; // ProtocolSI

    public partial class FormLogin : Form
    {

        FormRegistar formRegistar = new FormRegistar();
        public FormLogin()
        {
            InitializeComponent();
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txbUsername.Text) || string.IsNullOrWhiteSpace(txbPassword.Text))
            {
                MessageBox.Show("Preencha todos os campos.");
                return;
            }

            string username = txbUsername.Text.Trim();
            string password = txbPassword.Text;

            try
            {
                using (TcpClient client = new TcpClient("127.0.0.1", 10000))
                using (NetworkStream ns = client.GetStream())
                {
                    ProtocolSI protocol = new ProtocolSI();

                    // Handshake assimétrico (RSA)
                    RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
                    rsa.FromXmlString(ObterChavePublicaServidor());

                    using (AesCryptoServiceProvider aes = new AesCryptoServiceProvider())
                    {
                        aes.GenerateKey();
                        aes.GenerateIV();

                        byte[] keyPacket = protocol.Make(ProtocolSICmdType.SECRET_KEY, rsa.Encrypt(aes.Key, false));
                        ns.Write(keyPacket, 0, keyPacket.Length);
                        byte[] ivPacket = protocol.Make(ProtocolSICmdType.IV, rsa.Encrypt(aes.IV, false));
                        ns.Write(ivPacket, 0, ivPacket.Length);

                        // Pedido de salt ao servidor
                        string pedidoSalt = "SALT|" + username;
                        byte[] saltPacket = protocol.Make(ProtocolSICmdType.DATA, pedidoSalt);
                        ns.Write(saltPacket, 0, saltPacket.Length);

                        ns.Read(protocol.Buffer, 0, protocol.Buffer.Length);
                        string resposta = protocol.GetStringFromData();
                        if (!resposta.StartsWith("SALT|"))
                        {
                            MessageBox.Show("Utilizador não encontrado.");
                            return;
                        }
                        byte[] salt = Convert.FromBase64String(resposta.Split('|')[1]);

                        // Gerar salted hash da password
                        byte[] hash = GerarSaltedHash(password, salt);

                        // Dados de login cifrados
                        string dadosLogin = $"{username}|{Convert.ToBase64String(hash)}";
                        byte[] loginBytes = Encoding.UTF8.GetBytes(dadosLogin);
                        byte[] loginCifrado = CifrarAES(loginBytes, aes);

                        byte[] packetLogin = protocol.Make(ProtocolSICmdType.SYM_CIPHER_DATA, loginCifrado);
                        ns.Write(packetLogin, 0, packetLogin.Length);

                        // Receber resposta (imagem ou erro)
                        ns.Read(protocol.Buffer, 0, protocol.Buffer.Length);
                        if (protocol.GetCmdType() == ProtocolSICmdType.DATA)
                        {
                            string msg = protocol.GetStringFromData();
                            MessageBox.Show(msg);
                        }
                        else if (protocol.GetCmdType() == ProtocolSICmdType.SYM_CIPHER_DATA)
                        {
                            byte[] encryptedImage = protocol.GetData();
                            byte[] imageBytes = DecifrarAES(encryptedImage, aes);
                            // Aqui podes guardar ou mostrar a imagem de perfil recebida
                            MessageBox.Show("Login efetuado com sucesso e imagem recebida.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro: " + ex.Message);
            }
        }

        private byte[] GerarSaltedHash(string password, byte[] salt)
        {
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000))
            {
                return pbkdf2.GetBytes(32);
            }
        }

        private byte[] CifrarAES(byte[] dados, AesCryptoServiceProvider aes)
        {
            using (var ms = new MemoryStream())
            using (var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
            {
                cs.Write(dados, 0, dados.Length);
                cs.Close();
                return ms.ToArray();
            }
        }

        private byte[] DecifrarAES(byte[] dados, AesCryptoServiceProvider aes)
        {
            using (var ms = new MemoryStream(dados))
            using (var cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read))
            {
                byte[] plain = new byte[dados.Length];
                int bytesRead = cs.Read(plain, 0, plain.Length);
                Array.Resize(ref plain, bytesRead);
                return plain;
            }
        }

        private string ObterChavePublicaServidor()
        {
            // Lê a chave pública do servidor de ficheiro, por exemplo:
            // return File.ReadAllText("server_public.xml");
            throw new NotImplementedException("Implementa a leitura da chave pública do servidor conforme o teu ambiente.");
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
    }

}
