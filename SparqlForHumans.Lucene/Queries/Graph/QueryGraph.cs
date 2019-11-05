using System.Linq;
using System.Collections.Generic;
using SparqlForHumans.Models.Query;

namespace SparqlForHumans.Lucene.Queries.Graph
{
    public class QueryGraph
    {
        public void FindResults(string entitiesIndexPath, string propertyIndexPath)
        {
            EntitiesIndexPath = entitiesIndexPath;
            PropertiesIndexPath = propertyIndexPath;
            this.ExploreGraph(EntitiesIndexPath, PropertiesIndexPath);
        }
        public QueryGraph(RDFExplorerGraph rdfGraph)
        {
            Nodes = rdfGraph.nodes.ToDictionary(  x => x.id, x=> new QueryNode(x));
            Edges = rdfGraph.edges.ToDictionary(  x => x.id, x=> new QueryEdge(x));
            foreach (var node in Nodes)
                this.TraverseDepthFirstNode(node.Key);
            foreach (var edge in Edges)
                this.TraverseDepthFirstEdge(edge.Key);
            Selected = rdfGraph.selected;

            this.TraverseGraph();
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
                return Selected.isNode
                    ? Nodes.FirstOrDefault(x => x.Key.Equals(Selected.id)).Value.Results.Select(x => $"{x.Id}#{x.Label}").ToList()
                    : Edges.FirstOrDefault(x => x.Key.Equals(Selected.id)).Value.Results.Select(x => $"{x.Id}#{x.Label}").ToList();
            }
        }
    }
}
