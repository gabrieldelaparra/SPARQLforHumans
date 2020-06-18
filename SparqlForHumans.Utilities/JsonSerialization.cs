using System.IO;
using Newtonsoft.Json;

namespace SparqlForHumans.Utilities
{
    public static class JsonSerialization
    {
        public static T DeserializeJson<T>(string inputJsonFilename)
        {
            var text = File.ReadAllText(inputJsonFilename);
            return JsonConvert.DeserializeObject<T>(text);
        }

        public static void SerializeJson(this object objectToSerialize, string outputJsonFilename)
        {
            outputJsonFilename.CreatePathIfNotExists();
            var serializer = new JsonSerializer
                {NullValueHandling = NullValueHandling.Ignore, Formatting = Formatting.Indented};

            using var sw = new StreamWriter(outputJsonFilename);
            using JsonWriter writer = new JsonTextWriter(sw);
            serializer.Serialize(writer, objectToSerialize);
        }
    }
}