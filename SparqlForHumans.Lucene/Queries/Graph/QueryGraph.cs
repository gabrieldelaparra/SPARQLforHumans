using System.Linq;
using System.Collections.Generic;
namespace SparqlForHumans.Models.Query
{
    //TODO: Implement IsEqualityComparer<QueryGraph>, para saber si el graph cambió
    public class QueryGraph
    {
        public QueryGraph(RDFExplorerGraph rdfGraph, string propertiesIndexPath, string entitiesIndexPath)
        {
            this.Edges = rdfGraph.edges.Select(x => new QueryEdge(x)).ToList();
            this.Nodes = rdfGraph.nodes.Select(x => new QueryNode(x)).ToList();
            this.Selected = rdfGraph.selected;
            //this.ExploreGraph(PropertiesIndexPath);
            //this.Nodes.ForEach(x => this.TraverseDepthFirstNode(x.id));
            //this.Edges.ForEach(x => this.TraverseDepthFirstEdge(x.id));
        }

        public string PropertiesIndexPath { get; set; }
        public string EntitiesIndexPath { get; set; }

        public List<QueryNode> Nodes { get; set; }
        public List<QueryEdge> Edges { get; set; }
        public Selected Selected { get; set; }
    }
}
