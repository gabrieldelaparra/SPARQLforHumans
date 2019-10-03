using Newtonsoft.Json;
using SparqlForHumans.Wikidata.Models;
using System.Net;

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
            WebClient webClient = new WebClient();
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
    }
}
