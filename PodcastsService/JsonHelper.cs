using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace Podcasts
{
    public static class JsonHelper
    {
        public static string ToJson<T>(T msg)
        {
            var serializer = new DataContractJsonSerializer(typeof(T));
            using (var ms = new MemoryStream())
            {
                serializer.WriteObject(ms, msg);
                return Encoding.UTF8.GetString(ms.ToArray());
            }
        }

        public static T FromJson<T>(string msg)
        {
            var serializer = new DataContractJsonSerializer(typeof(T));
            try
            {
                using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(msg)))
                {
                    return (T)serializer.ReadObject(ms);
                }
            }
            catch (SerializationException exc)
            {
                throw new SerializationException($"Unable to deserialize JSON: {msg}", exc);
            }
        }
    }
}
