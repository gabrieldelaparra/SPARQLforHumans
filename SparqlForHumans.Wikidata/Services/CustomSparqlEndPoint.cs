using System;
using System.Net;
using VDS.RDF.Query;

namespace SparqlForHumans.Wikidata.Services
{
    public class CustomSparqlEndPoint : SparqlRemoteEndpoint
    {
        public CustomSparqlEndPoint(Uri endpointUri) : base(endpointUri) { }

        protected override void ApplyCustomRequestOptions(HttpWebRequest httpRequest)
        {
            httpRequest.Method = "GET";
            httpRequest.Accept = "application/sparql-results+json";
            httpRequest.UserAgent = ".Net Client";
            base.ApplyCustomRequestOptions(httpRequest);
        }
    }
}
