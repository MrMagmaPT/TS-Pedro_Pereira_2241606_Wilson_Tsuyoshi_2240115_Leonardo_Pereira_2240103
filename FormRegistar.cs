using EI.SI;
using Projeto_TS_Pedro_Pereira_2241606_Wilson_Tsuyoshi_2240115.Controllers;
using Projeto_TS_Pedro_Pereira_2241606_Wilson_Tsuyoshi_2240115.Models;
using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Sockets;
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
            // Abre um diálogo para selecionar uma imagem
            openFileDialogImagem.Filter = "Image Files|*.png;";
            openFileDialogImagem.Title = "Select User Image - Max 256x256p";
            openFileDialogImagem.DefaultExt = "png";
            openFileDialogImagem.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            openFileDialogImagem.FileName = string.Empty; // Limpa seleção anterior
            openFileDialogImagem.Multiselect = false; // Permite selecionar apenas uma imagem


            if (openFileDialogImagem.ShowDialog() == DialogResult.OK)
            {
                //converter a imagem selecionada para a bdUserData e exibir na PictureBox
                try
                {
                    string imagePath = openFileDialogImagem.FileName;
                    Image userImage = Image.FromFile(imagePath);
                    // Verifica se a imagem é maior que 256x256 pixels
                    if (userImage.Width > 16 || userImage.Height > 16)
                    {
                        MessageBox.Show("A imagem tem de ser 16x16 pixels ou menor.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    // Exibe a imagem na PictureBox
                    pbUserImage.Image = userImage;
                    profPic = File.ReadAllBytes(openFileDialogImagem.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading image: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("No image selected.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }

        private void btnRegistar_Click(object sender, EventArgs e)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(txbUsername.Text))
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
                                byte[] saltedPasswordHash = SaltedHashText.GenerateSaltedHash(password, salt, 1000);

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

                                int num_resposta = 0; //variavel para receber a resposta do servidor

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
                                    } else
                                    {
                                        MessageBox.Show(Encoding.UTF8.GetString(resposta), "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                    }
                                }
                            }
                        }
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

                //encripatar a mensagem com o protocolo SI

                

                //  _____ __  __  ____ _____ __ _____ _____  ___   ____  ___   _____  
                //  ||==  ||\\|| ((    ||_// || ||_//  ||   ||=|| ((    ||=|| ((   )) 
                //  ||___ || \||  \\__ || \\ || ||     ||   || ||  \\__ || ||  \\_//  

                                                                                                                                                            

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
    }
}
