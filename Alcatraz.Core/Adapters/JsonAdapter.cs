using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using SignalR;

namespace Alcatraz.Core.Adapters
{
    public class EnumJsonConvertAdapter : IJsonSerializer
    {
        private static readonly JsonConverter[] JsonConverters = new[] { new StringEnumConverter() };

        public string Stringify(object obj)
        {
            return JsonConvert.SerializeObject(obj, JsonConverters);
        }

        public object Parse(string json)
        {
            return JsonConvert.DeserializeObject(json);
        }

        public object Parse(string json, Type targetType)
        {
            return JsonConvert.DeserializeObject(json, targetType, JsonConverters);
        }

        public T Parse<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json, JsonConverters);
        }
    }

}