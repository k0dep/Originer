using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;

namespace Originer
{
    public static class JsonUtils
    {
        public static T JsonSerialize<T>(this string data)
        {
            var jsonFormatter = new DataContractJsonSerializer(typeof(T), new DataContractJsonSerializerSettings()
            {
                UseSimpleDictionaryFormat = true
            });
            
            var obj = default(T);
            using (var s = new MemoryStream(Encoding.UTF8.GetBytes(data)))
            {
                obj = (T)jsonFormatter.ReadObject(s);
            }

            return obj;
        }
        
        public static string JsonDeserialize<T>(this T data)
        {
            var jsonFormatter = new DataContractJsonSerializer(typeof(T), new DataContractJsonSerializerSettings()
            {
                UseSimpleDictionaryFormat = true
            });
            
            using (var stream = new MemoryStream())
            using (var jsonStream = JsonReaderWriterFactory.CreateJsonWriter(stream, Encoding.UTF8, true, true, "  "))
            {
                jsonFormatter.WriteObject(jsonStream, data);
                jsonStream.Flush();
                return Encoding.UTF8.GetString(stream.ToArray()).Replace("\\", "");
            }
        }
    }
}