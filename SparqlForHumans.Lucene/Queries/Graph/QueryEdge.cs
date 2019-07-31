using System.Collections.Generic;
namespace SparqlForHumans.Models.Query
{
    public class QueryEdge : Edge
    {
        public bool Traversed { get; set; } = false;
        public QueryEdge(Edge edge)
        {
            this.id = edge.id;
            this.name = edge.name;
            this.uris = edge.uris;
            this.sourceId = edge.sourceId;
            this.targetId = edge.targetId;
        }
        public QueryType QueryType {get;set; } = QueryType.Unkwown;
        public List<Property> Results { get; set; } = new List<Property>();
        public bool IsInstanceOf { get; set; } = false;
        public List<string> Domain { get; set; }
        public List<string> Range { get; set; }

        public override string ToString()
        {
            return $"{id}:{name} : ({sourceId})->({targetId})";
        }
    }


}
