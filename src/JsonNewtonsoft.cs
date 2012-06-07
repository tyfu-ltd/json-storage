using System;
using System.Net;
using System.Runtime.Serialization.Json;
using System.IO;
using System.Collections.Generic;
using System.Text;

using Newtonsoft.Json;
using tyfu.JsonStorage;

namespace tyfu.JsonStorage
{
    public class JsonNewtonsoft : ISerializeJson
    {
        public string Serialize<T>(T document)
        {
            try
            {
                return JsonConvert.SerializeObject(document);
            }
            catch (Exception ex)
            {
                var msg = string.Format("An execption was thrown while trying to serialise the document of type {0}\nMessage: {1}", document.GetType().FullName, ex.Message);
                throw new Exception(msg, ex);
            }
        }

        public T Deserialize<T>(string json)
        {
            try
            {
                return JsonConvert.DeserializeObject<T>(json);
            }
            catch (Exception ex)
            {
                var msg = string.Format("An exception was thrown while trying to deserialise the json of type {2}\nMessage:{1}\n\n{0}", json, ex.Message, typeof(T).FullName);
                throw new Exception(msg, ex);
            }
        }
    }
}