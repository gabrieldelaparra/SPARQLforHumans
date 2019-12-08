using System.Collections.Generic;
using System.Linq;
using SparqlForHumans.Models;
using SparqlForHumans.Models.RDFExplorer;
using SparqlForHumans.Utilities;

namespace SparqlForHumans.Lucene.Queries.Graph
{
    public class QueryEdge : Edge
    {
        public bool Traversed { get; set; } = false;
        public QueryEdge(Edge edge)
        {
            id = edge.id;
            name = edge.name;
            uris = edge.uris.Select(x => x.GetUriIdentifier()).ToArray();
            sourceId = edge.sourceId;
            targetId = edge.targetId;
        }
        public List<Property> Results { get; set; } = new List<Property>();
        public Dictionary<string, QueryGraphExtensions.Result> Values => Results.ToDictionary();
        public bool IsInstanceOf => this.HasInstanceOf();
        public bool IsGivenType { get; set; }
        public List<string> DomainBaseTypes { get; set; } = new List<string>();
        public List<string> DomainDerivedTypes { get; set; } = new List<string>();
        public List<string> RangeBaseTypes { get; set; } = new List<string>();
        public List<string> RangeDerivedTypes { get; set; } = new List<string>();

        public override string ToString()
        {
            return base.ToString();
        }
    }
}
