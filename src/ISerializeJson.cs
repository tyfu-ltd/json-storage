using System;

namespace tyfu.JsonStorage
{
    public interface ISerializeJson
    {
        string Serialize<T>(T graph);
        T Deserialize<T>(string json);
    }
}
