using System.Collections.Generic;
using System.Linq;
using SparqlForHumans.Models;
using SparqlForHumans.Models.RDFExplorer;
using SparqlForHumans.Models.Wikidata;
using SparqlForHumans.Utilities;

namespace SparqlForHumans.Lucene.Queries.Graph
{
    public class QueryNode : Node
    {
        public bool Traversed { get; set; } = false;
        public QueryNode(Node node)
        {
            id = node.id;
            name = node.name;
            uris = node.uris.Select(x => x.GetUriIdentifier()).ToArray();
        }
        //public QueryType QueryType { get; set; } = QueryType.Unknown;
        public List<Entity> Results { get; set; } = new List<Entity>();
        public List<string> Types { get; set; } = new List<string>();
        public List<string> GivenTypes { get; set; } = new List<string>();
        public List<string> InstanceOfBaseTypes { get; set; } = new List<string>();
        public List<string> InstanceOfDerivedTypes { get; set; } = new List<string>();
        public List<string> InferredBasedTypes { get; set; } = new List<string>();
        public List<string> InferredDerivedTypes { get; set; } = new List<string>();

        public Dictionary<string, QueryGraphExtensions.Result> Values => Results.ToDictionary();

        public bool IsGivenType { get; set; }
        public bool IsGoingToGivenType { get; set; } = false;
        public bool IsComingFromGivenType { get; set; } = false;

        public bool IsInstanceOfType { get; set; } = false;

        public bool IsInferredType => IsInferredDomainType || IsInferredRangeType;
        public bool IsInferredDomainType { get; set; } = false;
        public bool IsInferredRangeType { get; internal set; }
        //public List<string> InferredTypes { get; set; } = new List<string>();

        public override string ToString()
        {
            return $"{id}:{name} ({string.Join(";", Types.Select(x => x.GetUriIdentifier()))})";
        }
    }


}
