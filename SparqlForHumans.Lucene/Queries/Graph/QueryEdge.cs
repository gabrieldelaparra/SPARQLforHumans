using System.Collections.Generic;
using System.Linq;
using SparqlForHumans.Models;
using SparqlForHumans.Models.RDFExplorer;
using SparqlForHumans.Models.Wikidata;

namespace SparqlForHumans.Lucene.Queries.Graph
{
    public class QueryEdge : Edge
    {
        public bool Traversed { get; set; } = false;
        public QueryEdge(Edge edge)
        {
            id = edge.id;
            name = edge.name;
            uris = edge.uris;
            sourceId = edge.sourceId;
            targetId = edge.targetId;
        }
        public QueryType QueryType { get; set; } = QueryType.Unknown;
        public List<Property> Results { get; set; } = new List<Property>();
        public Dictionary<string, QueryGraphExtensions.Result> Values => Results.ToDictionary();
        public bool IsInstanceOf => this.HasInstanceOf();
        public bool IsGivenType { get; set; }//=> uris.Any();
        public List<string> Domain { get; set; } = new List<string>();
        public List<string> Range { get; set; } = new List<string>();

        public override string ToString()
        {
            return $"{id}:{name} : ({sourceId})->({targetId})";
        }
    }


}
