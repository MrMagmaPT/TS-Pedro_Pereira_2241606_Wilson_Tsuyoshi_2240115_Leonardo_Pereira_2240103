using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Data.SqlClient;
using System.IO;
using Projeto_TS_Pedro_Pereira_2241606_Wilson_Tsuyoshi_2240115.Controllers;
using System.Runtime.InteropServices.WindowsRuntime;

namespace Projeto_TS_Pedro_Pereira_2241606_Wilson_Tsuyoshi_2240115.Controllers
{
    class ControllerFormLogin
    {
        public byte[] verifyLogin(string username, string passwordHash)
        {
            SqlDataReader reader = null;
            SqlConnection connection = null;
            byte[] profPicBytes = null;

            string caminhoDB = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName; //ajustar o numero de .parent para subir ou descer na arvore de organização dos folders

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
                SqlParameter usernameParametro = new SqlParameter("@username", username);

                // Introduzir valor ao parâmentro registado no comando SQL
                cmd.Parameters.Add(usernameParametro);

                // Associar ligação à Base de Dados ao comando a ser executado
                cmd.Connection = connection;

                // Executar comando SQL
                reader = cmd.ExecuteReader();

                if (!reader.HasRows)
                {
                    throw new Exception("Error while trying to access an user");
                }

                // Ler resultado da pesquisa
                reader.Read();

                // Obter salt
                byte[] saltedPasswordHashStored = (byte[])reader["SaltedPasswordHash"];
                byte[] saltedStored = (byte[])reader["Salt"];

                connection.Close(); // Fecha a conexão com a base de dados após ler os dados iniciais

                connection.Open(); // Reabre a conexão para ler a imagem de perfil

                sql = "SELECT ProfPic FROM UserData WHERE Username = @username";
                cmd = new SqlCommand();
                cmd.CommandText = sql;

                usernameParametro = new SqlParameter("@username", username);

                cmd.Parameters.Add(usernameParametro);

                cmd.Connection = connection;

                reader = cmd.ExecuteReader();

                if (!reader.HasRows)
                {
                    throw new Exception("Error while trying to access "+ username +" image");
                }
                if (reader.Read() && !reader.IsDBNull(reader.GetOrdinal("ProfPic")))
                {
                    profPicBytes = (byte[])reader["ProfPic"];
                }

                byte[] saltedPasswordHashByte = SaltedHashText.GenerateSaltedHash(passwordHash, saltedStored, 1000);

                connection.Close(); // Garante que a conexão é fechada, mesmo em caso de erro
                if (saltedPasswordHashStored.SequenceEqual(saltedPasswordHashByte))
                {
                    return profPicBytes; // Se a password corresponder, retorna a imagem de perfil do utilizador
                } else
                {
                    return null; // Se a password não corresponder, retorna false
                }
            }
            catch (Exception e) 
            {
                connection.Close(); // Garante que a conexão é fechada, mesmo em caso de erro
                return null; // Retorna null em caso de erro
            }
            
        }



    }
}
