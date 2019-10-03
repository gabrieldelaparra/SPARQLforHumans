using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace SparqlForHumans.Wikidata.Models
{
    public class Entity
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("labels")]
        public Attribute Label { get; set; }

        [JsonProperty("descriptions")]
        public Attribute Description { get; set; }

        [JsonProperty("claims")]
        public Dictionary<string, Property[]> Properties { get; set; }
    }

    public class Property
    {
        [JsonProperty("mainsnak")]
        public PropertyContent Content { get; set; }
        public string Value
        {
            get
            {
                var content = Content.DataValue.Content.ToString();
                var splitString = content.Split(new string[] { "\"id\": \"" }, StringSplitOptions.None);
                if(!splitString.Length.Equals(2)) return string.Empty;
                var afterId = splitString[1].Split('"')[0];
                return afterId;
            }
        }
        public override string ToString()
        {
            return Value;
        }
    }

    public class PropertyContent
    {
        [JsonProperty("datavalue")]
        public DataValue DataValue { get; set; }
    }

    public class DataValue
    {
        [JsonProperty("value")]
        public object Content { get; set; }
    }

    public class Attribute
    {
        [JsonProperty("en")]
        public LanguageValue LanguageValue { get; set; }
        public override string ToString()
        {
            return LanguageValue.Value;
        }
    }

    public class LanguageValue
    {
        [JsonProperty("value")]
        public string Value { get; set; }
    }
    public class WikidataGetEntitiesQueryResult
    {
        [JsonProperty("entities")]
        public Dictionary<string, Entity> Entities { get; set; }
    }
}
