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

namespace Projeto_TS_Pedro_Pereira_2241606_Wilson_Tsuyoshi_2240115
{
    public partial class FormRegistar : Form
    {
        //declaração de variaveis globais
        bool hasCustomImage = false; // Verifica se o utilizador selecionou uma imagem personalizada
        SqlConnection conn = new SqlConnection(); // Conexão com a base de dados
        string caminhoDB = "D:\\2 semestre\\TS-Tópicos de Segurança\\Projeto\\TS-Pedro_Pereira_2241606_Wilson_Tsuyoshi_2240115";
        string username = null;
        string password = null;
        string passwordHash = null;
        string saltedPasswordHash = null;
        Image defaultUserImage = Properties.Resources.defaultUserImage_png; // Imagem padrão do utilizador
        private const int NUMBER_OF_ITERATIONS = 10000; // Número de iterações para o algoritmo de hashing
        private const int SALT_SIZE = 8; // Tamanho do salt em bytes

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
                                string username = txbUsername.Text;
                                string password = txbPass.Text;

                                // Gera um salt aleatório
                                byte[] salt = GenerateSalt(SALT_SIZE);

                                // Gera o hash da senha sem salt
                                string hashPassword = HashPassword(password);

                                // Gera o hash da senha com o salt (use the original password, not the hash)
                                byte[] saltedPasswordHash = GenerateSaltedHash(password, salt);

                                Image userImage = pbUserImage.Image ?? defaultUserImage;

                                Registar(username, hashPassword, saltedPasswordHash, salt, userImage);
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

        private static byte[] GenerateSalt(int size)
        {
            //Generate a cryptographic random number.
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            byte[] buff = new byte[size];
            rng.GetBytes(buff);
            return buff;
        }

        private static byte[] GenerateSaltedHash(string plainText, byte[] salt)
        {
            Rfc2898DeriveBytes rfc2898 = new Rfc2898DeriveBytes(plainText, salt, NUMBER_OF_ITERATIONS);
            return rfc2898.GetBytes(32);
        }
        private static string HashPassword(string password)
        {
            // Cria um hash da senha usando SHA256
            using (SHA256 sha1 = SHA256.Create())
            {
                //transforma a string em bytes
                byte[] dados = Encoding.UTF8.GetBytes(password);

                // gera o hash
                byte[] hash = sha1.ComputeHash(dados);

                // converte o hash em string base64
                password = Convert.ToBase64String(hash);

                return password; // retorna o hash da senha

                // converte o hash em string hexadecimal
                //tbBitsHashData.Text = (hash.Length * 8).ToString();
            }
        }
        private void Registar(string username, string passwordHash, byte[] saltedPasswordHash, byte[] salt, Image profPic)
        {
            SqlConnection connection = null;
            try
            {
                connection = new SqlConnection();
                connection.ConnectionString = String.Format(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename='" + caminhoDB + "\\UserDataBase.mdf" + "';Integrated Security=True"); // String de conexão com a base de dados
                
                connection.Open(); // Abre a conexão com a base de dados

                SqlParameter usernameParam = new SqlParameter("@Username", username);
                SqlParameter passwordHashParam = new SqlParameter("@passwordHash", passwordHash);
                SqlParameter saltedPasswordHashParam = new SqlParameter("@saltedPasswordHash", saltedPasswordHash);
                SqlParameter saltParam = new SqlParameter("@salt", salt);
                SqlParameter profPicParam = new SqlParameter("@profPic", SqlDbType.Image);


                // Declaração do comando SQL
                String sql = "INSERT INTO UserData (Username, PasswordHash, SaltedPasswordHash, Salt, ProfPic) VALUES (@Username, @passwordHash ,@saltedPasswordHash, @salt, @profPic)";

                SqlCommand comando = new SqlCommand(sql, connection);

                // Adiciona os parâmetros ao comando SQL
                comando.Parameters.Add(usernameParam);
                comando.Parameters.Add(passwordHashParam);
                comando.Parameters.Add(saltedPasswordHashParam);
                comando.Parameters.Add(saltParam);

                // Converte a imagem para um array de bytes e adiciona ao comando SQL
                if (profPic != null)
                {
                    // Converte a imagem para um array de bytes
                    using (MemoryStream ms = new MemoryStream())
                    {
                        profPic.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                        profPicParam.Value = ms.ToArray();
                    }
                }
                else
                {
                    // Se não houver imagem, define o parâmetro como DBNull
                    profPicParam.Value = DBNull.Value;
                }
                // Adiciona o parâmetro da imagem ao comando SQL
                comando.Parameters.Add(profPicParam);   

                int lines = comando.ExecuteNonQuery(); // Executa o comando SQL e retorna o número de linhas afetadas

                connection.Close(); // Fecha a conexão com a base de dados

                if (lines > 0) // Verifica se o comando foi executado com sucesso
                {
                    MessageBox.Show("Utilizador registado com sucesso!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Erro ao registar utilizador.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

            }catch(Exception ex)
            {
                MessageBox.Show("Erro ao registar utilizador: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (connection != null && connection.State == ConnectionState.Open)
                {
                    connection.Close(); // Garante que a conexão é fechada
                }
            }

        }
        private int ExecuteInsertUserQuery(SqlConnection connection, string username, string passwordHash, byte[] saltedPasswordHash, byte[] salt, Image profPic)
        {
            using (SqlCommand comando = new SqlCommand("INSERT INTO UserData (Username, PasswordHash, SaltedPasswordHash, Salt, ProfPic) VALUES (@Username, @passwordHash ,@saltedPasswordHash, @salt, @profPic)", connection))
            {
                comando.Parameters.Add(new SqlParameter("@Username", username));
                comando.Parameters.Add(new SqlParameter("@passwordHash", passwordHash));
                comando.Parameters.Add(new SqlParameter("@saltedPasswordHash", saltedPasswordHash));
                comando.Parameters.Add(new SqlParameter("@salt", salt));

                SqlParameter profPicParam = new SqlParameter("@profPic", SqlDbType.Image);
                if (profPic != null)
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        profPic.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                        profPicParam.Value = ms.ToArray();
                    }
                }
                else
                {
                    profPicParam.Value = DBNull.Value;
                }
                comando.Parameters.Add(profPicParam);

                return comando.ExecuteNonQuery();
            }
        }
    }
}
