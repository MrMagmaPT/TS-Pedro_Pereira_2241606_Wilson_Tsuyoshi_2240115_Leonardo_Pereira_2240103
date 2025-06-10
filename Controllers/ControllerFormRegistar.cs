using Projeto_TS_Pedro_Pereira_2241606_Wilson_Tsuyoshi_2240115.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace Projeto_TS_Pedro_Pereira_2241606_Wilson_Tsuyoshi_2240115.Controllers
{
    class ControllerFormRegistar
    {
        private SqlConnection connection = null;

        string caminhoDB = Directory.GetParent(Environment.CurrentDirectory).Parent.FullName; //ajustar o numero de .parent para subir ou descer na arvore de organização dos folders

        public void Registar(Usuario user)
        {
            try
            {
                connection = new SqlConnection();
                connection.ConnectionString = String.Format(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename='" + caminhoDB + "\\UserDataBase.mdf" + "';Integrated Security=True"); // String de conexão com a base de dados

                connection.Open(); // Abre a conexão com a base de dados

                SqlParameter usernameParam = new SqlParameter("@Username", user.username);
                SqlParameter passwordHashParam = new SqlParameter("@passwordHash", user.passwordHash);
                SqlParameter saltedPasswordHashParam = new SqlParameter("@saltedPasswordHash", user.saltedPasswordHash);
                SqlParameter saltParam = new SqlParameter("@salt", user.salt);
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
                if (user.profilePicture != null)
                {
                    // Converte a imagem para um array de bytes
                    using (MemoryStream ms = new MemoryStream())
                    {
                        user.profilePicture.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                        profPicParam.Value = ms.ToArray();
                    }
                }
                else
                {
                    // Se não houver imagem, define o parâmetro como DBNull
                    profPicParam.Value = null;
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

            }
            catch (Exception ex)
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
    }
}
