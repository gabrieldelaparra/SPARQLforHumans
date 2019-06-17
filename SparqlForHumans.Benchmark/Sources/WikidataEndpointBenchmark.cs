using Newtonsoft.Json;
using SparqlForHumans.Benchmark.Models.Base;
using SparqlForHumans.Benchmark.Models.WikidataResponse;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace SparqlForHumans.Benchmark.Sources
{
    public class WikidataEndpointBenchmark : AbstractBenchmark
    {
        public string HttpRequest { get; set; }

        public WikidataEndpointBenchmark(string httpRequest)
        {
            HttpRequest = "https://query.wikidata.org/sparql?format=json&query=" + httpRequest;
        }

        public override List<string> RunQuery()
        {
            var httpResponse = Get(HttpRequest);
            var response = JsonConvert.DeserializeObject<Rootobject>(httpResponse);
            return response.results.bindings.Select(x => x.item.value).ToList();
            //return null;
            //var list = JsonConvert.DeserializeObject<List<string>>(results.bindings);
            //Logger.Info($"Query Results: {response.results.bindings.Count}");
        }

        public string Get(string uri)
        {
            var request = (HttpWebRequest)WebRequest.Create(uri);
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            using (var response = (HttpWebResponse)request.GetResponse())
            using (var stream = response.GetResponseStream())
            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
