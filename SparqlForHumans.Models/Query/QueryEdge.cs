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
        public List<string> Results { get; set; } = new List<string>();
        public bool IsInstanceOf { get; set; } = false;
        public override string ToString()
        {
            return $"{id}:{name} : ({sourceId})->({targetId}) : [IsP31: {IsInstanceOf}] : {string.Join(",", Results)}";
        }
    }


}
