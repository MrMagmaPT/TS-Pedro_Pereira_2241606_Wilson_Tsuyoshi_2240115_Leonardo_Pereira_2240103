using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.Remoting;
using System.Text;
using System.Threading.Tasks;

namespace Projeto_TS_Pedro_Pereira_2241606_Wilson_Tsuyoshi_2240115.Models
{
    public class ClientInfo
    {
        public string pubkey { get; set; }
        public TcpClient cliente { get; set; }
        public byte[] chaveSimetrica { get; set; }
        public ClientInfo() { }

        public ClientInfo(string chavePublica, TcpClient clienteTCP, byte[] chaveSim)
        {
            pubkey = chavePublica;
            cliente = clienteTCP; 
            chaveSimetrica = chaveSim;
        }
    }

    
}
