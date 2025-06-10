using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Projeto_TS_Pedro_Pereira_2241606_Wilson_Tsuyoshi_2240115.Controllers
{
    class Hash
    {
        public static string HashPassword(string password)
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
    }

    class Salt
    {

        public static byte[] GenerateSalt(int size)
        {
            //Generate a cryptographic random number.
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            byte[] buff = new byte[size];
            rng.GetBytes(buff);
            return buff;
        }
    }
    
    class SaltedHashText
    {
        public static byte[] GenerateSaltedHash(string plainText, byte[] salt, int number_of_iterations)
        {
            Rfc2898DeriveBytes rfc2898 = new Rfc2898DeriveBytes(plainText, salt, number_of_iterations);
            return rfc2898.GetBytes(32);
        }
     }
}
