﻿using EI.SI;
using Projeto_TS_Pedro_Pereira_2241606_Wilson_Tsuyoshi_2240115.Controllers;
using Projeto_TS_Pedro_Pereira_2241606_Wilson_Tsuyoshi_2240115.Models;
using Projeto_TS_Pedro_Pereira_2241606_Wilson_Tsuyoshi_2240115.Properties;
using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace Projeto_TS_Pedro_Pereira_2241606_Wilson_Tsuyoshi_2240115
{
    public partial class FormRegistar : Form
    {
        //declaração de variaveis globais
        byte[] profPic = null;


        // Tamanho do salt em bytes
        private const int SALT_SIZE = 8; 


        //protocolo si e cenas
        ProtocolSI protocolSI;
        NetworkStream ns;
        TcpClient client;
        private const int PORT = 12345;




        public FormRegistar()
        {
            InitializeComponent();
        }

        private void pbUserImage_Click(object sender, EventArgs e)
        {
            using (var form = new FormSelecionarImagem())
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    profPic = form.profPic;
                    pbUserImage.Image = form.ProfPicImage; // Exibe a imagem selecionada na PictureBox
                }
                else
                {
                    // Dialog was cancelled
                    MessageBox.Show("Seleção de imagem cancelada.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void btnRegistar_Click(object sender, EventArgs e)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(txbUsername.Text))
                {
                    if (txbUsername.Text.Length <= 24)
                    {
                        if (!string.IsNullOrWhiteSpace(txbPass.Text))
                        {
                            if (!string.IsNullOrWhiteSpace(txbConfirmPass.Text))
                            {
                                if (txbPass.Text == txbConfirmPass.Text)
                                {
                                    // Facilita a leitura do código ao criar variáveis para o nome de utilizador e senha
                                    string username = txbUsername.Text;
                                    string password = txbPass.Text;

                                    // Gera um salt aleatório
                                    byte[] salt = Salt.GenerateSalt(SALT_SIZE);

                                    // Gera o hash da senha sem salt
                                    string passwordHash = Hash.HashPassword(password);

                                    // Gera o hash da senha com o salt (use the original password, not the hash)
                                    byte[] saltedPasswordHash = SaltedHashText.GenerateSaltedHash(passwordHash, salt, 1000);

                                    // Converte a imagem da PictureBox para um objeto Image
                                    Image userImage = pbUserImage.Image;
                                    if (userImage != null)
                                    {
                                        using (var ms = new MemoryStream())
                                        {
                                            userImage.Save(ms, System.Drawing.Imaging.ImageFormat.Png); // Salva a imagem no formato PNG
                                            profPic = ms.ToArray();
                                        }
                                    }

                                    //cria o objeto Usuario com os dados do utilizador
                                    Usuario usuario = new Usuario(username, passwordHash, saltedPasswordHash, salt, profPic);

                                    //converter usuario para array de byte
                                    byte[] userBytes = ControllerSerializar.SerializaParaArrayBytes<Usuario>(usuario);

                                    //adicionar o endereço da mensagem (enum do tipo de mensagem) para o array de bytes
                                    MessageTypeEnum messageType = new MessageTypeEnum(); //instanciação do enum de tipo de mensagem
                                    MessageTypeEnum.MessageType tipo = MessageTypeEnum.MessageType.Register; //enum de prefixo para registar
                                    byte[] mensagemComTipo = messageType.CreateMessage(tipo, userBytes); //criação da mensagem com o tipo de mensagem e o array de bytes do utilizador

                                    //criar metodo para enviar mensagem para o servidor
                                    enviarMensagem(mensagemComTipo); //envia a mensagem para o servidor


                                    while (protocolSI.GetCmdType() != ProtocolSICmdType.ACK)
                                    {
                                        // Lê a resposta do servidor
                                        ns.Read(protocolSI.Buffer, 0, protocolSI.Buffer.Length);
                                        byte[] resposta = protocolSI.GetData(); // Obtém os dados da resposta
                                        if (resposta == null)
                                        {
                                            MessageBox.Show("Erro ao receber a resposta do servidor.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                            return;
                                        }
                                        else if (Encoding.UTF8.GetString(resposta) == "0")
                                        {
                                            CloseRegistar();// se a resposta for 0, sai do metodo porque 0 significa semi EOT (End of Transmission) e não há mais dados a receber desta trsnmição
                                        }
                                        else
                                        {
                                            MessageBox.Show(Encoding.UTF8.GetString(resposta), "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                        }
                                    }
                                }
                                else
                                {
                                    MessageBox.Show("As senhas não coincidem.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }
                            }
                            else
                            {
                                MessageBox.Show("A confirmação da senha não pode estar vazia.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                        else
                        {
                            MessageBox.Show("A senha não pode estar vazia.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    } else
                    {
                        MessageBox.Show("O nome de utilizador não pode ter mais do que 24 caracteres.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    MessageBox.Show("O nome de utilizador não pode estar vazio.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error while registering user: " + ex.Message, ex);
            }
        }

        private bool enviarMensagem(byte[] mensagem)
        {
            try
            {
                //declaração inicial de dependencias para a comunicação entre o cliente e o servidor
                IPEndPoint endpoint = new IPEndPoint(IPAddress.Loopback, PORT); // Define o ponto final do servidor (localhost e porta 12345)

                client = new TcpClient(); // Cria uma nova instância do cliente TCP

                client.Connect(endpoint); // Conecta ao servidor

                ns = client.GetStream(); // Obtém o network stream do cliente para enviar e receber dados
                protocolSI = new ProtocolSI(); // Instanciação do protocolo SI


                //prepara a informação em modelo do protocolo SI
                byte[] packet = protocolSI.Make(ProtocolSICmdType.DATA, mensagem); // Criação da mensagem com o tipo de comando e os dados do utilizador

                // Envia a mensagem para o servidor
                ns.Write(packet, 0, packet.Length); // Envia a mensagem para o servidor

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro a enviar os dados de registo para o servidor: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private void CloseRegistar()
        {
            //ENVIAR O EOT(END OF TRANSMISSION) PARA O SERVIDOR
            byte[] eot = protocolSI.Make(ProtocolSICmdType.EOT);
            ns.Write(eot, 0, eot.Length);

            //LER O ACK(acknowledgement)
            ns.Read(protocolSI.Buffer, 0, protocolSI.Buffer.Length);

            ns.Close();
            client.Close();
            this.Close(); // Fecha o formulário de registo
        }

        public byte[] cifrarRegistar(byte[] mensagem, ClientInfo cliente)
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

        private void pbVisibilidade_Click(object sender, EventArgs e)
        {
            if (txbPass.PasswordChar == '\0')
            {
                //se nao ta censurado censura
                txbPass.PasswordChar = '*';
                txbConfirmPass.PasswordChar = '*';
                pbVisibilidade.Image = Resources.EyeOpen;
            }
            else
            {
                //se ta censurado descensura
                txbPass.PasswordChar = '\0';
                txbConfirmPass.PasswordChar = '\0';
                pbVisibilidade.Image = Resources.eyeClose;
            }
        }
    }
}
