using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using SparqlForHumans.Lucene.Extensions;
using SparqlForHumans.Models;
using SparqlForHumans.Models.RDFExplorer;
using SparqlForHumans.Utilities;

namespace SparqlForHumans.Lucene.Models
{
    public class QueryNode : Node
    {
        public QueryNode(Node node)
        {
            id = node.id;
            name = node.name;
            uris = node.uris.Select(x => x.GetUriIdentifier()).ToArray();
        }

        [JsonIgnore] public bool AvoidQuery { get; set; }

        [JsonIgnore] public List<string> InferredTypes { get; set; } = new List<string>();

        [JsonIgnore] public bool IsConstant { get; set; }

        [JsonIgnore] public bool IsInferredDomainType { get; set; } = false;

        [JsonIgnore] public bool IsInferredRangeType { get; internal set; }

        [JsonIgnore] public bool IsInferredType => IsInferredDomainType || IsInferredRangeType;

        [JsonIgnore] public bool IsInstanceOf { get; set; } = false;

        [JsonIgnore] public List<string> ParentTypes { get; set; } = new List<string>();

        [JsonIgnore] public List<Entity> Results { get; set; } = new List<Entity>();

        [JsonIgnore] public bool Traversed { get; set; } = false;

        [JsonIgnore] public List<string> Types { get; set; } = new List<string>();

        public Dictionary<string, Result> Values => Results.ToDictionary();

        public override string ToString()
        {
            return $"{id}:{name} ({string.Join(";", uris.Select(x => x.GetUriIdentifier()))})";
        }
    }
}