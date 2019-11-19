using Newtonsoft.Json;

namespace PokerLibrary.Networking
{
    public static class JsonService
    {
        private static JsonSerializerSettings SerializerSettings => new JsonSerializerSettings 
        { 
            TypeNameHandling = TypeNameHandling.All 
        }; 
        public static T Deserialize<T>(string input)
        {
            return JsonConvert.DeserializeObject<T>(input, SerializerSettings);
        }
        public static string Serialize<T>(T input)
        {
            return JsonConvert.SerializeObject(input, SerializerSettings);
        }
    }
}
