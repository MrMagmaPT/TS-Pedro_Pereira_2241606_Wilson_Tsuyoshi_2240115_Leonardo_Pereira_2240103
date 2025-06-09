using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace Projeto_TS_Pedro_Pereira_2241606_Wilson_Tsuyoshi_2240115
{
    public partial class FormRegistar : Form
    {
        //declaração de variaveis globais
        bool hasCustomImage = false; // Verifica se o utilizador selecionou uma imagem personalizada
        SqlConnection conn = new SqlConnection(); // Conexão com a base de dados
        string caminhoDB = "E:\\UNI\\Segundo Semestre\\TS - Técnicas de segurança\\Projeto de TS\\Projeto-TS-Pedro_Pereira_2241606&Wilson_Tsuyoshi_2240115";
        string username = null;
        string password = null;
        string passwordHash = "brotha";
        string saltedPasswordHash = null;
        Image defaultUserImage = Properties.Resources.defaultUserImage; // Imagem padrão do utilizador



        public FormRegistar()
        {
            InitializeComponent();
        }

        private void pbUserImage_Click(object sender, EventArgs e)
        {
            openFileDialogImagem.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif";
            openFileDialogImagem.Title = "Select User Image - Max 256x256p";
            openFileDialogImagem.DefaultExt = "png";
            openFileDialogImagem.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            openFileDialogImagem.FileName = string.Empty; // Limpa seleção anterior
            openFileDialogImagem.Multiselect = false; // Permite selecionar apenas uma imagem


            if(openFileDialogImagem.ShowDialog() == DialogResult.OK)
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
                if (!string.IsNullOrWhiteSpace(txbUsername.Text)) // verifica se o username nao está vazio
                {
                    if (!string.IsNullOrWhiteSpace(txbPass.Text)) // verifica se a senha não está vazia 
                    {
                        if (!string.IsNullOrWhiteSpace(txbConfirmPass.Text)) // verifica se a confirmação da senha não está vazia
                        {
                            if (txbPass.Text == txbConfirmPass.Text) //verifica se as passwords coincidem
                            {
                                username = txbUsername.Text;
                                password = txbPass.Text;
                                passwordHash = txbPass.Text; //temp
                                saltedPasswordHash = txbPass.Text; //temp
                                


                                conn.ConnectionString = String.Format(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename='"+ caminhoDB +"';Integrated Security=True"); // String de conexão com a base de dados
                                conn.Open(); // Abre a conexão com a base de dados

                                SqlParameter paramUsername = new SqlParameter("@Username", username);
                                SqlParameter paramPassHash = new SqlParameter("@Passhash", passwordHash);
                                SqlParameter paramSaltHash = new SqlParameter("@Saltedhash", saltedPasswordHash);
                                SqlParameter paramImage = null; // Inicializa o parâmetro de imagem como nulo
                                //verifica se o utilizador selecionou uma imagem personalizada
                                if (hasCustomImage)
                                {
                                    Image userImage = pbUserImage.Image; // Obtém a imagem do utilizador selecionada na PictureBox
                                    paramImage = new SqlParameter("@Profpic", userImage); // Inicializa a imagem com a imagem selecionada
                                }
                                else
                                {
                                    paramImage = new SqlParameter("@Profpic", defaultUserImage); // Inicializa a imagem como nula
                                }
                                String sql = "INSERT INTO UserData (Username, Passhash, Saltedhash,Profpic) VALUES (@username,@Passhash,@Saltedhash,@Profpic)";

                                // Prepara comando SQL para ser executado na Base de Dados
                                SqlCommand cmd = new SqlCommand(sql, conn);

                                // Introduzir valores aos parâmentros registados no comando SQL
                                cmd.Parameters.Add(paramUsername);
                                cmd.Parameters.Add(paramPassHash);
                                cmd.Parameters.Add(paramSaltHash);
                                cmd.Parameters.Add(paramImage);

                                // Executar comando SQL
                                int lines = cmd.ExecuteNonQuery();

                                conn.Close(); // Fecha a conexão com a base de dados

                                if (lines == 0)
                                {
                                    // Se forem devolvidas 0 linhas alteradas então o não foi executado com sucesso
                                    throw new Exception("Error while inserting an user");
                                }
                                MessageBox.Show("Utilizador Registado Com Sucesso");

                                this.Close(); //fecha o form dps do registo
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
