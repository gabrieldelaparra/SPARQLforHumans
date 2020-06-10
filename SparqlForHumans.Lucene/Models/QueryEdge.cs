using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using SparqlForHumans.Lucene.Extensions;
using SparqlForHumans.Lucene.Queries.Graph;
using SparqlForHumans.Models;
using SparqlForHumans.Models.RDFExplorer;
using SparqlForHumans.Utilities;

namespace SparqlForHumans.Lucene.Models
{
    public class QueryEdge : Edge
    {
        [JsonIgnore]
        public bool Traversed { get; set; } = false;
        public QueryEdge(Edge edge)
        {
            id = edge.id;
            name = edge.name;
            uris = edge.uris.Select(x => x.GetUriIdentifier()).ToArray();
            sourceId = edge.sourceId;
            targetId = edge.targetId;
        }
        [JsonIgnore]
        public List<Property> Results { get; set; } = new List<Property>();
        public Dictionary<string, Result> Values => Results.ToDictionary();
        [JsonIgnore]
        public bool IsInstanceOf => this.HasInstanceOf();
        [JsonIgnore]
        public bool IsConstant { get; set; }
        [JsonIgnore]
        public List<string> DomainTypes { get; set; } = new List<string>();
        //public List<string> DomainDerivedTypes { get; set; } = new List<string>();
        [JsonIgnore]
        public List<string> RangeTypes { get; set; } = new List<string>();
        //public List<string> RangeDerivedTypes { get; set; } = new List<string>();
        [JsonIgnore]
        public bool AvoidQuery { get; set; }

        public override string ToString()
        {
            return base.ToString();
        }
    }
}
