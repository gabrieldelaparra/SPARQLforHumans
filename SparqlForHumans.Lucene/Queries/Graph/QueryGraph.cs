using System.Linq;
using System.Collections.Generic;
using SparqlForHumans.Models.Query;

namespace SparqlForHumans.Lucene.Queries.Graph
{
    public class QueryGraph
    {
        public QueryGraph(RDFExplorerGraph rdfGraph, string entitiesIndexPath = "", string propertyIndexPath = "")
        {
            EntitiesIndexPath = entitiesIndexPath;
            PropertiesIndexPath = propertyIndexPath;
            Selected = rdfGraph.selected;
            this.ExploreGraph(EntitiesIndexPath, PropertiesIndexPath);

            Nodes = rdfGraph.nodes.ToDictionary(  x => x.id, x=> new QueryNode(x));
            Edges = rdfGraph.edges.ToDictionary(  x => x.id, x=> new QueryEdge(x));
            foreach (var node in Nodes)
                this.TraverseDepthFirstNode(node.Key);
            foreach (var edge in Edges)
                this.TraverseDepthFirstEdge(edge.Key);
        }

        public string EntitiesIndexPath { get; set; }
        public string PropertiesIndexPath { get; set; }

        public Dictionary<int, QueryNode> Nodes { get; set; }
        public Dictionary<int, QueryEdge> Edges { get; set; }
        public Selected Selected { get; set; }
        public List<string> Results
        {
            get
            {
                if (Selected.isNode)
                    return Nodes.FirstOrDefault(x => x.Key.Equals(Selected.id)).Value.Results.Select(x => $"{x.Id}#{x.Label}").ToList();
                else
                    return Edges.FirstOrDefault(x => x.Key.Equals(Selected.id)).Value.Results.Select(x => $"{x.Id}#{x.Label}").ToList();
            }
        }
    }
}
