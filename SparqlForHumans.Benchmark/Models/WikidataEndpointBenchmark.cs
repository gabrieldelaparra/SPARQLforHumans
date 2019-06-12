using System.IO;
using System.Net;

namespace SparqlForHumans.Benchmark.Models
{
    public class WikidataEndpointBenchmark : AbstractBenchmark
    {
        public string HttpRequest { get; set; }

        public WikidataEndpointBenchmark(string httpRequest)
        {
            HttpRequest = "https://query.wikidata.org/sparql?format=json&query="+httpRequest;
            RunBenchmark();
        }

        public override void RunQuery()
        {
            var httpResponse = Get(HttpRequest);
            dynamic response = Newtonsoft.Json.JsonConvert.DeserializeObject(httpResponse);
            Logger.Info($"Query Results: {response.results.bindings.Count}");
        }

        public string Get(string uri)
        {
            var request = (HttpWebRequest)WebRequest.Create(uri);
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            using(var response = (HttpWebResponse)request.GetResponse())
            using(var stream = response.GetResponseStream())
            using(var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
