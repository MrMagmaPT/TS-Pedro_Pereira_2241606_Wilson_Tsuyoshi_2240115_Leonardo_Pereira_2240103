using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace Projeto_TS
{
    public partial class FormLogin: Form
    {
        string caminhoDB = "E:\\UNI\\Segundo Semestre\\TS - Técnicas de segurança\\Projeto de TS\\Projeto-TS-Pedro_Pereira_2241606&Wilson_Tsuyoshi_2240115";
        private const int NUMBER_OF_ITERATIONS = 10000; // Número de iterações para o algoritmo de hashing
        private const int SALT_SIZE = 8; // Tamanho do salt em bytes

        //private RSACryptoServiceProvider rsaSign;

        //private RSACryptoServiceProvider rsaVerify;


        FormRegistar formRegistar = new FormRegistar();
        public FormLogin()
        {
            InitializeComponent();
            
            //rsaSign = new RSACryptoServiceProvider(2048);
            //string publicKey = rsaSign.ToXmlString(false);

            //rsaVerify = new RSACryptoServiceProvider(2048);
            //rsaVerify.FromXmlString(publicKey);

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

        private bool verifyLogin(string username, string password)
        {
            SqlConnection connection = null;

            try
            {
                connection = new SqlConnection();
                connection.ConnectionString = String.Format(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename='" + caminhoDB + "\\UserDataBase.mdf" + "';Integrated Security=True"); // String de conexão com a base de dados
                connection.Open(); // Abre a conexão com a base de dados

                // Declaração do comando SQL
                String sql = "SELECT * FROM UserData WHERE Username = @username";
                SqlCommand cmd = new SqlCommand();
                cmd.CommandText = sql;

                // Declaração dos parâmetros do comando SQL
                SqlParameter param = new SqlParameter("@username", username);

                // Introduzir valor ao parâmentro registado no comando SQL
                cmd.Parameters.Add(param);

                // Associar ligação à Base de Dados ao comando a ser executado
                cmd.Connection = connection;

                // Executar comando SQL
                SqlDataReader reader = cmd.ExecuteReader();

                if (!reader.HasRows)
                {
                    throw new Exception("Error while trying to access an user");
                }

                // Ler resultado da pesquisa
                reader.Read();

                // Obter Hash (password + salt)
                byte[] saltedPasswordHashStored = (byte[])reader["SaltedPasswordHash"];


                // Obter salt
                byte[] saltStored = (byte[])reader["Salt"];

                connection.Close();

                //TODO: verificar se a password na base de dados 
                byte[] hash = GenerateSaltedHash(password, saltStored);

                return saltedPasswordHashStored.SequenceEqual(hash);

                throw new NotImplementedException();
            }
            catch (Exception e)
            {
                MessageBox.Show("An error occurred: " + e.Message);
                return false;
            }
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            string password = txbPassword.Text;
            string username = txbUsername.Text;
            if (verifyLogin(username, password))
            {
                MessageBox.Show("Login efetuado com sucesso!", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);

            }
            else
            {
                MessageBox.Show("Nome de usuário ou senha incorretos.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static byte[] GenerateSaltedHash(string plainText, byte[] salt)
        {
            Rfc2898DeriveBytes rfc2898 = new Rfc2898DeriveBytes(plainText, salt, NUMBER_OF_ITERATIONS);
            return rfc2898.GetBytes(32);
        }
    }
}
