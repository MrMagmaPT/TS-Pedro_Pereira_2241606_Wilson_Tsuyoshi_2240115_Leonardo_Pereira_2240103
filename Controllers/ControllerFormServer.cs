using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using EI.SI;

namespace Projeto_TS_Pedro_Pereira_2241606_Wilson_Tsuyoshi_2240115.Controllers
{
    class ControllerServerLogin
    {

    }

    class ControllerServerMessages
    {

        private List<TcpClient> clientesConectados = new List<TcpClient>();

        public void AdicionarCliente(TcpClient cliente)
        {
            lock (clientesConectados)
            {
                clientesConectados.Add(cliente);
            }
        }

        public void RemoverCliente(TcpClient cliente)
        {
            lock (clientesConectados)
            {
                clientesConectados.Remove(cliente);
            }
        }

        public void EnviarMensagem(string mensagem, TcpClient remetente)
        {
            ProtocolSI protSI = new ProtocolSI();
            byte[] packet = protSI.Make(ProtocolSICmdType.DATA, mensagem);

            lock (clientesConectados)
            {
                foreach (var cliente in clientesConectados)
                {
                    if (cliente != remetente)
                    {
                        try
                        {
                            NetworkStream ns = cliente.GetStream();
                            ns.Write(packet, 0, packet.Length);
                        }
                        catch
                        {
                           //erro de envio
                           Console.WriteLine($"[Erro] Não foi possível enviar mensagem");

                        }
                    }
                }
            }
        }
    }

    class ControllerServerRegistar
    {

    }
}
