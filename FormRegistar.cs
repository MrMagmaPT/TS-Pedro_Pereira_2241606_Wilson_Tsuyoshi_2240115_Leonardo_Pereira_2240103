using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
using Projeto_TS_Pedro_Pereira_2241606_Wilson_Tsuyoshi_2240115.Models;
using Projeto_TS_Pedro_Pereira_2241606_Wilson_Tsuyoshi_2240115.Controllers;

namespace Projeto_TS_Pedro_Pereira_2241606_Wilson_Tsuyoshi_2240115
{
    public partial class FormRegistar : Form
    {
        //declaração de variaveis globais
        SqlConnection connection = new SqlConnection(); // Conexão com a base de dados

        //ControllerS
        ControllerFormRegistar ControllerRegistar = new Controllers.ControllerFormRegistar();
        Hash Hash = new Hash();
        Salt Salt = new Salt();
        SaltedHashText SaltedHashText = new SaltedHashText();


        // Tamanho do salt em bytes
        private const int SALT_SIZE = 8; 

        public FormRegistar()
        {
            InitializeComponent();
        }

        private void pbUserImage_Click(object sender, EventArgs e)
        {
            // Abre um diálogo para selecionar uma imagem
            openFileDialogImagem.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp";
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
                    if (userImage.Width > 256 || userImage.Height > 256)
                    {
                        MessageBox.Show("A imagem tem de ser 256x256 pixels ou menor.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    // Exibe a imagem na PictureBox
                    pbUserImage.Image = userImage;
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
                                Image profPic = pbUserImage.Image;

                                // Chama o método Registar para inserir os dados na base de dados
                                Controllers.ControllerFormRegistar controller = new Controllers.ControllerFormRegistar();

                                Usuario usuario = new Usuario(username, passwordHash, saltedPasswordHash, salt, profPic);

                                ControllerRegistar.Registar(usuario);
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
    }
}
