using ServiceStack.Text;

namespace tyfu.JsonStorage
{
    public class JsonServiceStack : ISerializeJson
    {
        public string Serialize<T>(T document)
        {
            return JsonSerializer.SerializeToString<T>(document);
        }

        public T Deserialize<T>(string document)
        {
            return JsonSerializer.DeserializeFromString<T>(document);
        }
    }
}
