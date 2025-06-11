using Projeto_TS_Pedro_Pereira_2241606_Wilson_Tsuyoshi_2240115.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Projeto_TS_Pedro_Pereira_2241606_Wilson_Tsuyoshi_2240115.Controllers
{
    public static class ControllerSerializar
    {
        //incrivel contorllador para serialização e desserialização de classes independente do tipo de classes (amazing) 
        public static byte[] SerializaParaArrayBytes<T>(T obj)
        {
            //usamos os serializer para obter o tipo do obtejo
            XmlSerializer serializer = new XmlSerializer(typeof(T));

            //usando um memory stream fazemos a serialização do obj para array de bytes
            using (MemoryStream ms = new MemoryStream())
            {
                serializer.Serialize(ms, obj);
                //retornamos o array de bbytes formatado em xml
                return ms.ToArray();
            }
        }

        public static T DeserializaDeArrayBytes<T>(byte[] data)
        {
            //uamos o serializer para obter o tipo dos dados a devolver (obtidos na chamada da função)
            XmlSerializer serializer = new XmlSerializer(typeof(T));

            //usamos o memory stream para converter de array de bytes para um objeto do tipo t (que é uma alias para o tipo de class utilizado na chamada da função)
            using (MemoryStream ms = new MemoryStream(data))
            {
                //devolvemos serializado com o tipo da class que chamou a funcção
                return (T)serializer.Deserialize(ms);
            }
        }
    }
}
