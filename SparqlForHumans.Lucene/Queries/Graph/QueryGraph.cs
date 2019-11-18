using System.Collections.Generic;
using System.Linq;
using SparqlForHumans.Models.RDFExplorer;

namespace SparqlForHumans.Lucene.Queries.Graph
{
    public class QueryGraph
    {
        public QueryGraph(RDFExplorerGraph rdfGraph)
        {
            Nodes = rdfGraph.nodes.ToDictionary(x => x.id, x => new QueryNode(x));
            Edges = rdfGraph.edges.ToDictionary(x => x.id, x => new QueryEdge(x));

            this.CheckNodeTypes();

            foreach (var node in Nodes)
                this.TraverseDepthFirstNode(node.Key);
            foreach (var edge in Edges)
                this.TraverseDepthFirstEdge(edge.Key);
            Selected = rdfGraph.selected;

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
