using System.Collections.Generic;
using System.Linq;
using SparqlForHumans.Utilities;
namespace SparqlForHumans.Models.Query
{
    public class QueryNode : Node
    {
        public bool Traversed { get; set; } = false;
        public QueryNode(Node node)
        {
            this.id = node.id;
            this.name = node.name;
            this.uris = node.uris;
        }
        public QueryType QueryType {get;set; } = QueryType.Unkwown;
        public List<Entity> Results { get; set; } = new List<Entity>();
        public List<string> Types { get; set; } = new List<string>();
        public bool IsKnownType { get; set; } = false;
        public bool IsDirectedToKnownType { get; set; } = false;
        public override string ToString()
        {
            return $"{id}:{name} {(Types.Any() ? string.Join(";", Types.Select(x=>x.GetUriIdentifier())) : string.Empty)}";
        }
    }


}
