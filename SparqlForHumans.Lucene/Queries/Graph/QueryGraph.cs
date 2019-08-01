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
            Edges = rdfGraph.edges.Select(x => new QueryEdge(x)).ToList();
            Nodes = rdfGraph.nodes.Select(x => new QueryNode(x)).ToList();
            Selected = rdfGraph.selected;
            this.ExploreGraph(PropertiesIndexPath);
            this.Nodes.ForEach(x => this.TraverseDepthFirstNode(x.id));
            this.Edges.ForEach(x => this.TraverseDepthFirstEdge(x.id));
        }

        public string EntitiesIndexPath { get; set; }
        public string PropertiesIndexPath { get; set; }

        public List<QueryNode> Nodes { get; set; }
        public List<QueryEdge> Edges { get; set; }
        public Selected Selected { get; set; }
        public List<string> Results
        {
            get
            {
                if(Selected.isNode)
                    return Nodes.Find(x=>x.id.Equals(Selected.id)).Results.Select(x=>$"{x.Id}#{x.Label}").ToList();
                else
                    return Edges.Find(x=>x.id.Equals(Selected.id)).Results.Select(x=>$"{x.Id}#{x.Label}").ToList();
            }
        }
    }
}
