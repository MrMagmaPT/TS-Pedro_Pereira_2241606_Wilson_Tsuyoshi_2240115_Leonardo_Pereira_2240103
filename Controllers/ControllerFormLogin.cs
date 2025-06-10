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
using System.IO;
using Projeto_TS;
using Projeto_TS_Pedro_Pereira_2241606_Wilson_Tsuyoshi_2240115.Controllers;

namespace Projeto_TS_Pedro_Pereira_2241606_Wilson_Tsuyoshi_2240115.Controllers
{
    class ControllerFormLogin
    {
        public Image verifyLogin(string username, string password)
        {
            SqlConnection connection = null;

            string caminhoDB = Directory.GetParent(Environment.CurrentDirectory).Parent.FullName; //ajustar o numero de .parent para subir ou descer na arvore de organização dos folders

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

                connection.Open(); // Reabre a conexão para obter a imagem de perfil

                sql = "SELECT ProfPic FROM UserData WHERE Username = @username";
                cmd = new SqlCommand();
                cmd.CommandText = sql;

                param = new SqlParameter("@username", username);

                cmd.Parameters.Add(param);

                cmd.Connection = connection;

                reader = cmd.ExecuteReader();

                if (!reader.HasRows)
                {
                    throw new Exception("Error while trying to access "+ username +" image");
                }
                Image ProfPic = null;
                if (reader.Read() && !reader.IsDBNull(reader.GetOrdinal("ProfPic")))
                {
                    byte[] profPicBytes = (byte[])reader["ProfPic"];
                    if (profPicBytes != null && profPicBytes.Length > 0)
                    {
                        using (MemoryStream ms = new MemoryStream(profPicBytes))
                        {
                            ProfPic = Image.FromStream(ms);
                        }
                    }
                }


                //TODO: verificar se a password na base de dados 
                byte[] saltedPasswordHash = Controllers.SaltedHashText.GenerateSaltedHash(password, saltStored, 1000);


                if (saltedPasswordHashStored.SequenceEqual(saltedPasswordHash))
                {
                    return ProfPic; // Se a password corresponder, retorna a imagem de perfil do utilizador
                } else
                {
                    return null; // Se a password não corresponder, retorna false
                }

                    throw new NotImplementedException();
            }
            catch (Exception e)
            {
                MessageBox.Show("An error occurred: " + e);
                return null;
            }
        }



    }
}
