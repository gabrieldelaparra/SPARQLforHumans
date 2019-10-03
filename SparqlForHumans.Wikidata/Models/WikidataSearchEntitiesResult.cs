using Newtonsoft.Json;
using System;

namespace SparqlForHumans.Wikidata.Models
{
    public class WikidataSearchEntitiesResult
    {
        [JsonProperty("searchinfo")]
        public Searchinfo Searchinfo { get; set; }

        [JsonProperty("search")]
        public Search[] Search { get; set; }

        [JsonProperty("search-continue", NullValueHandling = NullValueHandling.Ignore)]
        public long SearchContinue { get; set; }

        [JsonProperty("success")]
        public long Success { get; set; }
    }

    public class Search
    {
        [JsonProperty("repository")]
        public string Repository { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("concepturi")]
        public Uri Concepturi { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("pageid")]
        public long Pageid { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("label")]
        public string Label { get; set; }

        [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
        public string Description { get; set; }

        [JsonProperty("match")]
        public Match Match { get; set; }

        [JsonProperty("aliases", NullValueHandling = NullValueHandling.Ignore)]
        public string[] Aliases { get; set; }
    }

    public class Match
    {
        [JsonProperty("type")]
        public TypeEnum Type { get; set; }

        [JsonProperty("language")]
        public Language Language { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }
    }

    public class Searchinfo
    {
        [JsonProperty("search")]
        public string Search { get; set; }
    }

    public enum Language { En };

    public enum TypeEnum { Alias, Label };
}
