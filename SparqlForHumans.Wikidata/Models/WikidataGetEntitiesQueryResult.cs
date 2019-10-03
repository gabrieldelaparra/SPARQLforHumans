using System.Collections.Generic;
using Newtonsoft.Json;

namespace SparqlForHumans.Wikidata.Models
{
    public class Entity
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("datatype")]
        public string Datatype { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("labels")]
        public Property Labels { get; set; }

        [JsonProperty("descriptions")]
        public Property Descriptions { get; set; }
    }

    public class Property
    {
        [JsonProperty("en")]
        public LanguageValues En { get; set; }
    }

    public class LanguageValues
    {
        [JsonProperty("language")]
        public string Language { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }
    }
    public class WikidataGetEntitiesQueryResult
    {
        [JsonProperty("entities")]
        public Dictionary<string, Entity> Entities { get; set; }

        [JsonProperty("success")]
        public long Success { get; set; }
    }
}
