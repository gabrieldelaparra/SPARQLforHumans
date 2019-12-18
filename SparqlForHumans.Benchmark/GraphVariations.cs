using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SparqlForHumans.Models.RDFExplorer;

namespace SparqlForHumans.Benchmark
{
    public static class GraphVariations
    {
        public static List<RDFExplorerGraph> GetGraphVariations(this RDFExplorerGraph graph)
        {
            var graphList = new List<RDFExplorerGraph> { graph };

            var edges = graph.edges;
            var nodes = graph.nodes;
            var count = edges.Count();

            for (var i = 0; i < Math.Pow(2, count); i++)
            {
                var newEdges = new List<Edge>();


                foreach (var edge in edges)
                {
                    var newEdge = new Edge(edge.id, edge.name, edge.sourceId, edge.targetId, edge.uris);
                    if ((newEdge.id & i) == 0)
                        newEdge = DisableEdge(newEdge);
                    newEdges.Add(newEdge);
                }

                var newGraph = new RDFExplorerGraph() { nodes = nodes, edges = newEdges.ToArray() };

                if (!graphList.Any(x => x.ToString().Equals(newGraph.ToString())))
                    graphList.Add(newGraph);
            }

            return graphList;
        }

        private static Edge DisableEdge(Edge edge)
        {
            var uris = new string[0];
            edge.uris = uris;
            return edge;
        }
    }
}
