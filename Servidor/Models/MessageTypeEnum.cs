using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projeto_TS_Pedro_Pereira_2241606_Wilson_Tsuyoshi_2240115.Models
{
    class MessageTypeEnum
    {
        public enum MessageType : byte
        {
            Register = 1,
            Login = 2,
            SendMessage = 3
        }

        public byte[] CreateMessage(MessageType type, byte[] payload)
        {
            byte[] message = new byte[1 + payload.Length];
            message[0] = (byte)type; // first byte is message type
            Buffer.BlockCopy(payload, 0, message, 1, payload.Length);
            return message;
        }


    }
}
