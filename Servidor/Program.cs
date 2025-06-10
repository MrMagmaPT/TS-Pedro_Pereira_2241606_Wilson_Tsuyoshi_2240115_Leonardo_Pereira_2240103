using System;
using System.Data.SqlClient;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using EI.SI; // ProtocolSI

namespace Servidor
{
    class Program
    {
        private const int PORT = 10000;
        private const string localpath = "E:\\UNI\\Segundo Semestre\\TS - Técnicas de segurança\\Projeto de TS\\Projeto-TS-Pedro_Pereira_2241606&Wilson_Tsuyoshi_2240115";
        private const string CONN_STRING = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=" + localpath + "UserDataBase.mdf;Integrated Security=True";
        private static RSACryptoServiceProvider rsa;

        static void Main(string[] args)
        {
            rsa = new RSACryptoServiceProvider(2048);
            File.WriteAllText("server_public.xml", rsa.ToXmlString(false)); // Chave pública para clientes

            IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, PORT);
            TcpListener listener = new TcpListener(endPoint);

            listener.Start();
            Console.WriteLine("Server Ready!!!");

            while (true)
            {
                TcpClient client = listener.AcceptTcpClient();
                Console.WriteLine("Cliente conectado: " + ((IPEndPoint)client.Client.RemoteEndPoint).Address);
                Thread t = new Thread(() => HandleClient(client));
                t.Start();
            }
        }

        private static void HandleClient(TcpClient client)
        {
            using (client)
            using (NetworkStream ns = client.GetStream())
            {
                ProtocolSI protocol = new ProtocolSI();
                byte[] aesKey = null, aesIV = null;

                try
                {
                    // Handshake: receber chave AES e IV cifrados com RSA
                    while (aesKey == null || aesIV == null)
                    {
                        ns.Read(protocol.Buffer, 0, protocol.Buffer.Length);
                        switch (protocol.GetCmdType())
                        {
                            case ProtocolSICmdType.SECRET_KEY:
                                aesKey = rsa.Decrypt(protocol.GetData(), false);
                                break;
                            case ProtocolSICmdType.IV:
                                aesIV = rsa.Decrypt(protocol.GetData(), false);
                                break;
                        }
                    }

                    // Espera por comandos do cliente
                    while (true)
                    {
                        ns.Read(protocol.Buffer, 0, protocol.Buffer.Length);
                        if (protocol.GetCmdType() == ProtocolSICmdType.DATA)
                        {
                            string comando = protocol.GetStringFromData();
                            if (comando.StartsWith("SALT|"))
                            {
                                string username = comando.Split('|')[1];
                                byte[] salt = GetSaltFromDB(username);
                                if (salt == null)
                                {
                                    byte[] packet = protocol.Make(ProtocolSICmdType.DATA, "Utilizador não encontrado.");
                                    ns.Write(packet, 0, packet.Length);
                                    return;
                                }
                                else
                                {
                                    byte[] packet = protocol.Make(ProtocolSICmdType.DATA, "SALT|" + Convert.ToBase64String(salt));
                                    ns.Write(packet, 0, packet.Length);
                                }
                            }
                        }
                        else if (protocol.GetCmdType() == ProtocolSICmdType.SYM_CIPHER_DATA)
                        {
                            // Receber dados cifrados (login ou registo)
                            byte[] dadosCifrados = protocol.GetData();
                            byte[] dadosDecifrados = DecifrarAES(dadosCifrados, aesKey, aesIV);
                            string dados = Encoding.UTF8.GetString(dadosDecifrados);

                            string[] partes = dados.Split('|');
                            if (partes.Length == 3)
                            {
                                // REGISTO: username|saltedHash|salt
                                string username = partes[0];
                                byte[] saltedHash = Convert.FromBase64String(partes[1]);
                                byte[] salt = Convert.FromBase64String(partes[2]);

                                // Receber imagem cifrada
                                ns.Read(protocol.Buffer, 0, protocol.Buffer.Length);
                                byte[] imgCifrada = protocol.GetData();
                                byte[] imgBytes = DecifrarAES(imgCifrada, aesKey, aesIV);

                                bool sucesso = RegistarUser(username, saltedHash, salt, imgBytes);
                                string msg = sucesso ? "Registo efetuado com sucesso." : "Erro: username já existe.";
                                byte[] resposta = protocol.Make(ProtocolSICmdType.DATA, msg);
                                ns.Write(resposta, 0, resposta.Length);
                                return;
                            }
                            else if (partes.Length == 2)
                            {
                                // LOGIN: username|saltedHash
                                string username = partes[0];
                                byte[] saltedHash = Convert.FromBase64String(partes[1]);

                                byte[] img = ObterImagemSeLoginValido(username, saltedHash);
                                if (img == null)
                                {
                                    byte[] resposta = protocol.Make(ProtocolSICmdType.DATA, "Utilizador não existe ou password incorreta.");
                                    ns.Write(resposta, 0, resposta.Length);
                                }
                                else
                                {
                                    byte[] imgCifrada = CifrarAES(img, aesKey, aesIV);
                                    byte[] resposta = protocol.Make(ProtocolSICmdType.SYM_CIPHER_DATA, imgCifrada);
                                    ns.Write(resposta, 0, resposta.Length);
                                }
                                return;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Erro no cliente: " + ex.Message);
                }
            }
        }

        private static byte[] GetSaltFromDB(string username)
        {
            using (var conn = new SqlConnection(CONN_STRING))
            {
                conn.Open();
                var cmd = new SqlCommand("SELECT Salt FROM UserData WHERE Username=@u", conn);
                cmd.Parameters.AddWithValue("@u", username);
                var result = cmd.ExecuteScalar();
                return result as byte[];
            }
        }

        private static bool RegistarUser(string username, byte[] saltedHash, byte[] salt, byte[] img)
        {
            using (var conn = new SqlConnection(CONN_STRING))
            {
                conn.Open();
                var cmd = new SqlCommand("INSERT INTO UserData (Username, SaltedPasswordHash, Salt, ProfPic) VALUES (@u, @h, @s, @p)", conn);
                cmd.Parameters.AddWithValue("@u", username);
                cmd.Parameters.AddWithValue("@h", saltedHash);
                cmd.Parameters.AddWithValue("@s", salt);
                cmd.Parameters.AddWithValue("@p", img);
                try
                {
                    cmd.ExecuteNonQuery();
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        private static byte[] ObterImagemSeLoginValido(string username, byte[] saltedHash)
        {
            using (var conn = new SqlConnection(CONN_STRING))
            {
                conn.Open();
                var cmd = new SqlCommand("SELECT ProfPic FROM UserData WHERE Username=@u AND SaltedPasswordHash=@h", conn);
                cmd.Parameters.AddWithValue("@u", username);
                cmd.Parameters.AddWithValue("@h", saltedHash);
                var result = cmd.ExecuteScalar();
                return result as byte[];
            }
        }

        private static byte[] DecifrarAES(byte[] dados, byte[] key, byte[] iv)
        {
            using (var ms = new MemoryStream(dados))
            using (var aes = new AesCryptoServiceProvider { Key = key, IV = iv })
            using (var cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read))
            {
                byte[] plain = new byte[dados.Length];
                int bytesRead = cs.Read(plain, 0, plain.Length);
                Array.Resize(ref plain, bytesRead);
                return plain;
            }
        }

        private static byte[] CifrarAES(byte[] dados, byte[] key, byte[] iv)
        {
            using (var ms = new MemoryStream())
            using (var aes = new AesCryptoServiceProvider { Key = key, IV = iv })
            using (var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
            {
                cs.Write(dados, 0, dados.Length);
                cs.Close();
                return ms.ToArray();
            }
        }
    }
}
