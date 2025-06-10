using EI.SI;
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
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace Projeto_TS_Pedro_Pereira_2241606_Wilson_Tsuyoshi_2240115
{

    public partial class FormRegistar : Form
    {

        public FormRegistar()
        {
            InitializeComponent();
            pbUserImage.Click += pbUserImage_Click;
        }

        private void pbUserImage_Click(object sender, EventArgs e)
        {
            if (openFileDialogImagem.ShowDialog() == DialogResult.OK)
            {
                pbUserImage.ImageLocation = openFileDialogImagem.FileName;
            }
        }

        private void btnRegistar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txbUsername.Text) ||
                string.IsNullOrWhiteSpace(txbPass.Text) ||
                string.IsNullOrWhiteSpace(txbConfirmPass.Text))
            {
                MessageBox.Show("Preencha todos os campos.");
                return;
            }

            if (txbPass.Text != txbConfirmPass.Text)
            {
                MessageBox.Show("As passwords não coincidem.");
                return;
            }

            if (pbUserImage.ImageLocation == null)
            {
                MessageBox.Show("Selecione uma imagem de perfil.");
                return;
            }

            // Validação da imagem (formato e dimensões) já foi feita aqui antes do envio

            byte[] imagemBytes = File.ReadAllBytes(pbUserImage.ImageLocation);

            byte[] salt = GerarSalt();
            byte[] saltedHash = GerarSaltedHash(txbPass.Text, salt);

            string username = txbUsername.Text.Trim();
            string dados = $"{username}|{Convert.ToBase64String(saltedHash)}|{Convert.ToBase64String(salt)}";

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

                        // Dados de registo cifrados
                        byte[] dadosBytes = Encoding.UTF8.GetBytes(dados);
                        byte[] dadosCifrados = CifrarAES(dadosBytes, aes);

                        byte[] packetRegisto = protocol.Make(ProtocolSICmdType.SYM_CIPHER_DATA, dadosCifrados);
                        ns.Write(packetRegisto, 0, packetRegisto.Length);

                        // Imagem cifrada
                        byte[] imgCifrada = CifrarAES(imagemBytes, aes);
                        byte[] packetImg = protocol.Make(ProtocolSICmdType.SYM_CIPHER_DATA, imgCifrada);
                        ns.Write(packetImg, 0, packetImg.Length);

                        // Resposta do servidor
                        ns.Read(protocol.Buffer, 0, protocol.Buffer.Length);
                        string resposta = protocol.GetStringFromData();
                        MessageBox.Show(resposta);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao registar: " + ex.Message);
            }
        }

        private byte[] GerarSalt()
        {
            byte[] salt = new byte[8];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(salt);
            }
            return salt;
        }

        private byte[] GerarSaltedHash(string pass, byte[] salt)
        {
            using (var pbkdf2 = new Rfc2898DeriveBytes(pass, salt, 10000))
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

        private string ObterChavePublicaServidor()
        {
            // Lê o conteúdo do ficheiro XML com a chave pública do servidor
            // O ficheiro deve ter sido previamente criado pelo servidor com rsa.ToXmlString(false)
            string caminhoChave = "server_public.xml";
            if (!File.Exists(caminhoChave))
                throw new FileNotFoundException("Ficheiro da chave pública do servidor não encontrado: " + caminhoChave);

            return File.ReadAllText(caminhoChave);
        }


    }

}
