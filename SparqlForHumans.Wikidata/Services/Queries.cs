using Newtonsoft.Json;
using SparqlForHumans.Wikidata.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace SparqlForHumans.Wikidata.Services
{
    public static class Queries
    {
        public static WikidataSearchEntitiesResult QueryWikidataSearchByLabel(string[] input)
        {
            return QueryWikidataSearchByLabel(string.Join("|", input));
        }
        public static WikidataSearchEntitiesResult QueryWikidataSearchByLabel(string input)
        {
            var webClient = new WebClient();
            webClient.QueryString.Add("action", "wbsearchentities");
            webClient.QueryString.Add("format", "json");
            webClient.QueryString.Add("language", "en");
            webClient.QueryString.Add("uselang", "en");
            webClient.QueryString.Add("continue", "0");
            webClient.QueryString.Add("limit", "20");
            webClient.QueryString.Add("search", input);
            webClient.QueryString.Add("origin", "*");
            string result = webClient.DownloadString("https://www.wikidata.org/w/api.php");
            return JsonConvert.DeserializeObject<WikidataSearchEntitiesResult>(result);
        }
        public static WikidataGetEntitiesQueryResult QueryWikidataPropertiesById(string[] input)
        {
            return QueryWikidataPropertiesById(string.Join("|", input));
        }
        public static WikidataGetEntitiesQueryResult QueryWikidataPropertiesById(string input)
        {
            var webClient = new WebClient();
            webClient.QueryString.Add("action", "wbgetentities");
            webClient.QueryString.Add("format", "json");
            webClient.QueryString.Add("languages", "en");
            webClient.QueryString.Add("ids", input);
            webClient.QueryString.Add("props", "labels|descriptions");
            string result = webClient.DownloadString("https://www.wikidata.org/w/api.php");
            return JsonConvert.DeserializeObject<WikidataGetEntitiesQueryResult>(result);
        }
        public static WikidataGetEntitiesQueryResult QueryWikidataEntitiesById(string[] input)
        {
            return QueryWikidataEntitiesById(string.Join("|", input));
        }
        public static WikidataGetEntitiesQueryResult QueryWikidataEntitiesById(string input)
        {
            var webClient = new WebClient();
            webClient.QueryString.Add("action", "wbgetentities");
            webClient.QueryString.Add("format", "json");
            webClient.QueryString.Add("languages", "en");
            webClient.QueryString.Add("ids", input);
            webClient.QueryString.Add("props", "labels|descriptions|claims");
            string result = webClient.DownloadString("https://www.wikidata.org/w/api.php");
            return JsonConvert.DeserializeObject<WikidataGetEntitiesQueryResult>(result);
        }

        public static async Task<IEnumerable<WikidataResult>> QueryWikidataSearchByLabelAsync(string[] searchText)
        {
            return await QueryWikidataSearchByLabelAsync(string.Join("|", searchText));
        }
        public static async Task<IEnumerable<WikidataResult>> QueryWikidataSearchByLabelAsync(string searchText)
        {
            var webClient = new WebClient();
            webClient.QueryString.Add("action", "wbsearchentities");
            webClient.QueryString.Add("format", "json");
            webClient.QueryString.Add("language", "en");
            webClient.QueryString.Add("uselang", "en");
            webClient.QueryString.Add("continue", "0");
            webClient.QueryString.Add("limit", "20");
            webClient.QueryString.Add("search", searchText);
            webClient.QueryString.Add("origin", "*");
            var data = await webClient.DownloadStringTaskAsync("https://www.wikidata.org/w/api.php");
            var rawResults = JsonConvert.DeserializeObject<WikidataSearchEntitiesResult>(data);
            return rawResults.Search.Select(x => new WikidataResult(x));
        }
    }
}
