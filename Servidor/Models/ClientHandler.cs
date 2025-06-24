using EI.SI;
using Projeto_TS_Pedro_Pereira_2241606_Wilson_Tsuyoshi_2240115.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Projeto_TS_Pedro_Pereira_2241606_Wilson_Tsuyoshi_2240115.Models;
using Microsoft.Win32;
using System.Diagnostics.Eventing.Reader;
using System.Runtime.Remoting;
using System.Security.Cryptography;
using System.IO;

namespace Servidor.Models
{
    class ClientHandler
    {
        //Lista de clientes conectados
        public static List<ClientInfo> clientes = new List<ClientInfo>();
        private string UltimachavePub = null;
        private TcpClient ultimoCliente = null;
        private byte[] chaveSimetrica = null;

        AesCryptoServiceProvider aes;

        string username;

        //instancia o controller de registo
        ControllerFormRegistar controllerRegistar = new ControllerFormRegistar();

        //instancia o controller de login
        ControllerFormLogin controllerLogin = new ControllerFormLogin();


        //Assinatura



        public ClientHandler(TcpClient client)
        {
            this.ultimoCliente = client;
        }

        public void Handle()
        {
            Thread thread = new Thread(threadHandler);
            thread.Start();
        }

        //Vai tratar os threads 
        public void threadHandler()
        {
            NetworkStream networkStream = this.ultimoCliente.GetStream();
            ProtocolSI protocolSI = new ProtocolSI();

            while (protocolSI.GetCmdType() != ProtocolSICmdType.EOT)
            {
                int bytesRead = networkStream.Read(protocolSI.Buffer, 0, protocolSI.Buffer.Length);
                //ACK (acknowledgement)
                byte[] ack;

                switch (protocolSI.GetCmdType())
                {
                    //ENVIA DADOS
                    case ProtocolSICmdType.DATA:

                        byte[] dadosCifrados = protocolSI.GetData(); //recebe os dados cifradso

                        byte[] dadosCifradosSemTipo = dadosCifrados.Skip(1).ToArray();


                        switch (dadosCifrados[0])
                        {
                            case 1:
                                Usuario user = ControllerSerializar.DeserializaDeArrayBytes<Usuario>(dadosCifradosSemTipo); // Desserializa os dados recebidos para um objeto Usuario
                                string mensagem = controllerRegistar.Registar(user); //enviar os dados do usuario para o controller de registo e obter a mensagem de resposta
                                username = user.username; //obter o username do utilizador para mostrar no cmd
                                //realiza o registo do utilizador
                                addToLogAndMessage("[Servidor] - Registo de utilizador iniciado pelo cliente: " + username);

                                //mandar msg para o utilizador a enviar uma resposta a dizer se foi bem sucedido ou nao

                                byte[] resposta = protocolSI.Make(ProtocolSICmdType.DATA, mensagem);
                                networkStream.Write(resposta, 0, resposta.Length);

                                ack = protocolSI.Make(ProtocolSICmdType.ACK);
                                networkStream.Write(ack, 0, ack.Length);

                                break;
                            case 2:
                                byte[] respostaProfilePic = null;

                                string dadoscifradosloginsemtipo = Convert.ToBase64String(dadosCifradosSemTipo);

                                //decifrar a mensagem
                                byte[] dadosLoginSemTipo = decifrarDadosLogin(dadosCifradosSemTipo, chaveSimetrica);

                                //Converte os dados recebidos em uma string
                                string dadosLoginString = Encoding.UTF8.GetString(dadosLoginSemTipo);

                                //Separa os dados em partes usando o separador de nova linha
                                string[] parts = dadosLoginString.Split('\n');
                                username = parts[0];
                                string SaltePasswordHashString = parts[1];

                                //realiza o login do utilizador e mostra no cmd
                                addToLogAndMessage("[Servidor] - Login iniciado pelo cliente: " + username);

                                //Converte o SaltePasswordHashString de volta para um array de bytes
                                byte[] profPic = controllerLogin.verifyLogin(username, SaltePasswordHashString);

                                if (profPic == null)
                                {
                                    //Se o utilizador nao existir, envia uma mensagem de erro
                                    addToLogAndMessage("[Servidor] - Login falhou para o cliente: " + username);
                                    respostaProfilePic = protocolSI.Make(ProtocolSICmdType.EOT);
                                }
                                else
                                {
                                    ClientInfo cliente_novo = new ClientInfo(UltimachavePub, ultimoCliente, chaveSimetrica);
                                    clientes.Add(cliente_novo);
                                    //Se o utilizador existir, envia a imagem do perfil
                                    addToLogAndMessage("[Servidor] - Login bem sucedido para o cliente: " + username);
                                    //encriptar profpic
                                    byte[] profPicEncript = cifrarMensagem(profPic, cliente_novo);
                                    //enviar profpic
                                    respostaProfilePic = protocolSI.Make(ProtocolSICmdType.DATA, profPicEncript); // Envia a imagem do perfil
                                    
                                }

                                networkStream.Write(respostaProfilePic, 0, respostaProfilePic.Length);
                                break;
                                
                            case 3:
                                // Decifrar a mensagem recebida
                                byte[] mensagemSemTipo = decifrarMensagemRecebida(dadosCifradosSemTipo);

                                // Usar MemoryStream e BinaryReader para separar assinatura e mensagem
                                using (MemoryStream ms = new MemoryStream(mensagemSemTipo))
                                using (BinaryReader br = new BinaryReader(ms))
                                {
                                    // 1. Ler o tamanho da assinatura (4 bytes)
                                    int assinaturaLength = br.ReadInt32();

                                    // 2. Ler a assinatura
                                    byte[] assinatura = br.ReadBytes(assinaturaLength);

                                    // 3. Ler a mensagem original
                                    byte[] mensagemOriginal = br.ReadBytes((int)(ms.Length - ms.Position));

                                    // 4. Calcular hash da mensagem original
                                    byte[] hash;
                                    using (SHA256 sha256 = SHA256.Create())
                                        hash = sha256.ComputeHash(mensagemOriginal);

                                    // 5. Obter a chave pública do cliente
                                    string chavePublicaCliente = UltimachavePub;

                                    foreach (var cli in clientes)
                                    {
                                        if (cli.cliente == ultimoCliente)
                                        {
                                            chavePublicaCliente = cli.pubkey;
                                            break;
                                        }
                                    }

                                    RSACryptoServiceProvider rsaVerify = new RSACryptoServiceProvider();
                                    rsaVerify.FromXmlString(chavePublicaCliente);

                                    // 6. Verificar assinatura
                                    bool assinaturaValida = rsaVerify.VerifyHash(hash, CryptoConfig.MapNameToOID("SHA256"), assinatura);

                                    if (assinaturaValida)
                                    {
                                        string mensagemString = "[" + username + "] - " + Encoding.UTF8.GetString(mensagemOriginal);
                                        addToLogAndMessage(mensagemString);
                                        EnviarParaTodos(mensagemString);
                                    }
                                    else
                                    {
                                        addToLogAndMessage("[Servidor] - Assinatura digital inválida para mensagem de " + username);
                                        // Não retransmitir a mensagem
                                    }
                                }

                                ack = protocolSI.Make(ProtocolSICmdType.ACK);
                                networkStream.Write(ack, 0, ack.Length);
                                break;

                            case 4:
                                //recebe a chave publica
                                byte[] chavePubSemTipo = dadosCifrados.Skip(1).ToArray(); 
                                //aqi usamos os dados cifrados diretamente, porque o caso 4 nao manda dados cifrados (manda a chave publica)

                                //coverte para string
                                UltimachavePub = Encoding.UTF8.GetString(chavePubSemTipo);

                                byte[] chaveSimetricaAES;
                                //criar chave simmetrica
                                using (Aes aes = Aes.Create())
                                {
                                    aes.GenerateKey();
                                    chaveSimetricaAES = aes.Key; // Chave AES (32 bytes para AES-256)
                                }

                                //encriptar a chave simetrica com a chave publica
                                byte[] chaveSimetricaCifrada;

                                chaveSimetrica = chaveSimetricaAES;
                                using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
                                {
                                    rsa.FromXmlString(UltimachavePub); // Carrega a chave pública do cliente
                                    chaveSimetricaCifrada = rsa.Encrypt(chaveSimetrica, true); // Cifra com RSA
                                }


                                //Envia a chave simétrica cifrada para o cliente
                                byte[] respostaChave = protocolSI.Make(ProtocolSICmdType.DATA, chaveSimetricaCifrada);
                                networkStream.Write(respostaChave, 0, respostaChave.Length);

                                addToLogAndMessage("[Servidor] - Chave simétrica gerada e enviada para o cliente.");
                                break;

                            default:
                                throw new Exception("Tipo de comando desconhecido.");
                        }
                        //ack = protocolSI.Make(ProtocolSICmdType.ACK);
                        //networkStream.Write(ack, 0, ack.Length);
                        break;
                        
                    //CASO O CLIENTE ENVIE EOT (FIM DA TRANSMISSÃO)
                    case ProtocolSICmdType.EOT:
                        addToLogAndMessage("[Servidor] - A terminar a ligação com o cliente: " + username);
                        ack = protocolSI.Make(ProtocolSICmdType.ACK);
                        networkStream.Write(ack, 0, ack.Length);
                        break;
                }
            }
            //termina os recursos quando termina a transmissão
            networkStream.Close();
            ultimoCliente.Close();

            //passar pela lista de clientes e encontrar o cliente cujo o tcp coincide com UltimoCliente e quando encontrar elemina esse dado da lista de clientes
            foreach (ClientInfo dadosCliente in clientes)
            {
                if (dadosCliente.cliente == ultimoCliente)
                {
                    clientes.Remove(dadosCliente);
                    break;
                }
            }

        }

        public void EnviarParaTodos(string mensagem)
        {
            ProtocolSI protocolSI = new ProtocolSI();

            byte[] mensagemBytes = UTF8Encoding.UTF8.GetBytes(mensagem);

            lock (clientes)
            {
                foreach (ClientInfo dadosCliente in clientes)
                {

                    try
                    {
                        NetworkStream ns = dadosCliente.cliente.GetStream();

                        // encriptar a mensagem
                        byte[] mensagemBytesCifrados = cifrarMensagem(mensagemBytes, dadosCliente);

                        //transforma em packet de dados
                        byte[] packetCifrado = protocolSI.Make(ProtocolSICmdType.DATA, mensagemBytesCifrados);

                        //enviar a mensagem cifrada
                        ns.Write(packetCifrado, 0, packetCifrado.Length);

                        //entrega individual
                        addToLogAndMessage("[Servidor] - Conversação entre clientes: " + username);
                    }
                    catch (Exception)
                    {
                        //Caso nao chegue a mensagem ao cliente, remove o cliente da lista e fecha a conexão, considera tambem como cliente desconectado
                        addToLogAndMessage("[Servidor] - A terminar a ligação com o cliente: " + username);
                        dadosCliente.cliente.Close();
                        clientes.Remove(dadosCliente);
                    }
                }
            
            }
        }
        public byte[] cifrarMensagem(byte[] mensagem,ClientInfo cliente)
        {
            //encriptar mensagem
            byte[] mensagemCifrada;
            using (Aes aes = Aes.Create())
            {
                // Configurar a chave AES
                aes.Key = cliente.chaveSimetrica;

                // Criar o cifrador
                using (ICryptoTransform encryptor = aes.CreateEncryptor())
                using (MemoryStream ms = new MemoryStream())
                {
                    // Escrever o IV primeiro (necessário para decifração)
                    ms.Write(aes.IV, 0, aes.IV.Length);

                    // Cifrar os dados
                    using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    {
                        cs.Write(mensagem, 0, mensagem.Length);
                        cs.FlushFinalBlock();
                    }

                    return mensagemCifrada = ms.ToArray();
                }
            }
        }

        public byte[] decifrarMensagemRecebida(byte[] dadosCifrados)
        {
            try
            {
                byte[] iv = new byte[16];
                Buffer.BlockCopy(dadosCifrados, 0, iv, 0, 16);

                byte[] dadosCifradosSemIV = new byte[dadosCifrados.Length - 16];
                Buffer.BlockCopy(dadosCifrados, 16, dadosCifradosSemIV, 0, dadosCifradosSemIV.Length);

                byte[] chaveSimetricaParaDecifrar = ObterChaveSimetricaCliente(ultimoCliente);
                if (chaveSimetricaParaDecifrar == null)
                    return dadosCifrados;

                using (Aes aes = Aes.Create())
                {
                    aes.Key = chaveSimetricaParaDecifrar;
                    aes.IV = iv;

                    using (MemoryStream ms = new MemoryStream())
                    using (CryptoStream cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(dadosCifradosSemIV, 0, dadosCifradosSemIV.Length);
                        cs.FlushFinalBlock();
                        return ms.ToArray();
                    }
                }
            }
            catch (CryptographicException ex)
            {
                addToLogAndMessage($"[Servidor] - Erro ao decifrar: {ex.Message}");
                return dadosCifrados;
            }
        }

        public byte[] decifrarDadosLogin(byte[] dadosCifrados,byte[] chaveSimetricaLogin)
        {
            try
            {
                byte[] iv = new byte[16];
                Buffer.BlockCopy(dadosCifrados, 0, iv, 0, 16);

                byte[] dadosCifradosSemIV = new byte[dadosCifrados.Length - 16];
                Buffer.BlockCopy(dadosCifrados, 16, dadosCifradosSemIV, 0, dadosCifradosSemIV.Length);

                byte[] chaveSimetricaParaDecifrar = chaveSimetricaLogin;
                if (chaveSimetricaParaDecifrar == null)
                    return dadosCifrados;

                using (Aes aes = Aes.Create())
                {
                    aes.Key = chaveSimetricaParaDecifrar;
                    aes.IV = iv;

                    using (MemoryStream ms = new MemoryStream())
                    using (CryptoStream cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(dadosCifradosSemIV, 0, dadosCifradosSemIV.Length); //ele aqui ta a decodificar sem problema
                        cs.FlushFinalBlock();
                        return ms.ToArray();
                    }
                }
            }
            catch (CryptographicException ex)
            {
                addToLogAndMessage($"[Servidor] - Erro ao decifrar: {ex.Message}");
                return dadosCifrados;
            }
        }

        private byte[] ObterChaveSimetricaCliente(TcpClient cliente)
        {
            foreach (ClientInfo dadosCliente in clientes)
            {
                if (dadosCliente.cliente == cliente)
                {
                    return dadosCliente.chaveSimetrica;
                }
            }
            return null;
        }

        public void addToLogAndMessage(string mensagem, bool writeOnConsole = true)
        {
            if (writeOnConsole == true)
            {
                Console.WriteLine(mensagem);
            }

            string dataAtual = DateTime.Now.ToString("yyyy-MM-dd");
            string dataHoraAtual = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.ff");

            // Define o caminho do diretório de logs
            string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string logsDir = Path.Combine(documentsPath, "Server_Logs");

            // Cria o diretório se não existir
            Directory.CreateDirectory(logsDir);

            string fileName = "Log_" + dataAtual + ".txt";
            string filePath = Path.Combine(logsDir, fileName);

            // Append das mensagens para o file log com time stamp
            using (FileStream fs = new FileStream(filePath, FileMode.Append, FileAccess.Write))
            using (StreamWriter sw = new StreamWriter(fs))
            {
                sw.WriteLine(dataHoraAtual + " " + mensagem);
            }
        }
    }
}