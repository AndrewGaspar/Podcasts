using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;

namespace Podcasts.Utilities
{
    public class JsonSerializer<T>
    {
        private DataContractJsonSerializer Serializer = new DataContractJsonSerializer(typeof(T));

        public string ToJson(T obj)
        {
            using (var ms = new MemoryStream())
            {
                WriteJson(ms, obj);
                return Encoding.UTF8.GetString(ms.ToArray());
            }
        }

        public void WriteJson(Stream stream, T msg)
        {
            Serializer.WriteObject(stream, msg);
        }

        public T FromJson(string msg)
        {
            try
            {
                using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(msg)))
                {
                    return ReadJson(ms);
                }
            }
            catch (SerializationException exc)
            {
                throw new SerializationException($"Unable to deserialize JSON: {msg}", exc);
            }
        }

        public T ReadJson(Stream stream)
        {
            return (T)Serializer.ReadObject(stream);
        }
    }

    public static class JsonHelper
    {
        public static string ToJson<T>(T obj)
        {
            return new JsonSerializer<T>().ToJson(obj);
        }

        public static void WriteJson<T>(Stream stream, T msg)
        {
            new JsonSerializer<T>().WriteJson(stream, msg);
        }

        public static T FromJson<T>(string msg)
        {
            return new JsonSerializer<T>().FromJson(msg);
        }

        public static T ReadJson<T>(Stream stream)
        {
            return new JsonSerializer<T>().ReadJson(stream);
        }
    }
}