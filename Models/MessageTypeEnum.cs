using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projeto_TS_Pedro_Pereira_2241606_Wilson_Tsuyoshi_2240115.Models
{
    class MessageTypeEnum
    {
        //enum para os tipos diferentes de comunicação que pode haver entre cliente e servidor
        public enum MessageType : byte
        {
            Register = 1,
            Login = 2,
            SendMessage = 3,
            SendPublicKey = 4
        }

        //metodo de endereçamento do tipo a mensagem
        public byte[] CreateMessage(MessageType type, byte[] mensagem)
        {
            byte[] messagemcComTipo = new byte[1 + mensagem.Length];
            messagemcComTipo[0] = (byte)type; // first byte is message type
            Buffer.BlockCopy(mensagem, 0, messagemcComTipo, 1, mensagem.Length);
            return messagemcComTipo;
        }


    }
}
