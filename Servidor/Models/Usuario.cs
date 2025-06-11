using Servidor.Properties;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projeto_TS_Pedro_Pereira_2241606_Wilson_Tsuyoshi_2240115.Models
{
    public class Usuario
    {
        public string username { get; set; }
        public string passwordHash { get; set; }
        public byte[] saltedPasswordHash { get; set; }
        public byte[] salt { get; set; }  // Salt é um número inteiro, mas pode ser usado como byte[] se necessário  
        public byte[] profilePicture { get; set; } // Imagem padrão do utilizador  

        public Usuario(string username, string passwordHash, byte[] saltedPasswordHash, byte[] salt, byte[] profilePicture)
        {
            this.username = username;
            this.passwordHash = passwordHash;
            this.saltedPasswordHash = saltedPasswordHash;
            this.salt = salt;
            if (profilePicture == null)
            {
                // Define uma imagem padrão se a imagem do perfil for nula
                using (MemoryStream ms = new MemoryStream())
                {
                    Resources.defaultUserImage.Save(ms, System.Drawing.Imaging.ImageFormat.Png); // Garante o formato PNG
                    this.profilePicture = ms.ToArray();
                }
            }
            else {
                this.profilePicture = profilePicture;
            }

                
        }
        public Usuario(string username, string passwordHash, byte[] saltedPasswordHash, byte[] salt)
        {
            this.username = username;
            this.passwordHash = passwordHash;
            this.saltedPasswordHash = saltedPasswordHash;
            this.salt = salt;
        }

        public Usuario() { }
    }
}
