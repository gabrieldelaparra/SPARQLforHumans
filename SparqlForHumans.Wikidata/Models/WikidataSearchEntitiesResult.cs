using Newtonsoft.Json;
using System;

namespace SparqlForHumans.Wikidata.Models
{
    public class WikidataResult
    {
        public WikidataResult(Result result)
        {
            this.Id = result.Id;
            this.Description = result.Description;
            this.Label = result.Label;
            this.Title = result.Title;
            this.Url = result.Url;
        }
        public string Id { get; set; }
        public string Title { get; set; }
        public string Url { get; set; }
        public string Label { get; set; }
        public string Description { get; set; }
    }
    public class WikidataSearchEntitiesResult
    {
        [JsonProperty("search")]
        public Result[] Search { get; set; }
    }

    public class Result
    {
        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public string Id { get; set; }

        [JsonProperty("title", NullValueHandling = NullValueHandling.Ignore)]
        public string Title { get; set; }

        [JsonProperty("url", NullValueHandling = NullValueHandling.Ignore)]
        public string Url { get; set; }

        [JsonProperty("label", NullValueHandling = NullValueHandling.Ignore)]
        public string Label { get; set; }

        [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
        public string Description { get; set; }
    }
}
