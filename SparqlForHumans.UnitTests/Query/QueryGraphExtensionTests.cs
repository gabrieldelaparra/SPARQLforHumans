using System.Linq;
using SparqlForHumans.Lucene.Extensions;
using SparqlForHumans.Lucene.Models;
using SparqlForHumans.Lucene.Queries.Graph;
using SparqlForHumans.Models.RDFExplorer;
using Xunit;

namespace SparqlForHumans.UnitTests.Query
{
    public class QueryGraphExtensionTests
    {
        [Fact]
        public void TestEdgeHasInstanceOfFalse()
        {
            var edge = new Edge(0, "?prop0", 0, 1);
            var queryEdge = new QueryEdge(edge);
            Assert.False(queryEdge.HasInstanceOf());
        }

        [Fact]
        public void TestEdgeHasInstanceOfTrue()
        {
            var edge = new Edge(0, "?prop0", 0, 1, new[] { "http://www.wikidata.org/prop/direct/P31" });
            var queryEdge = new QueryEdge(edge);
            Assert.True(queryEdge.HasInstanceOf());
        }

        [Fact]
        public void TestEdgeIsInferibleFalse()
        {
            var edge = new Edge(0, "?prop0", 0, 1);
            var queryEdge = new QueryEdge(edge);
            Assert.False(queryEdge.IsInferible());
        }

        [Fact]
        public void TestEdgeIsInferibleFalseInstanceOf()
        {
            var edge = new Edge(0, "?prop0", 0, 1, new[] { "http://www.wikidata.org/prop/direct/P31" });
            var queryEdge = new QueryEdge(edge);
            Assert.False(queryEdge.IsInferible());
        }

        [Fact]
        public void TestEdgeIsInferibleTrue()
        {
            var edge = new Edge(0, "?prop0", 0, 1, new[] { "http://www.wikidata.org/prop/direct/P25" });
            var queryEdge = new QueryEdge(edge);
            Assert.True(queryEdge.IsInferible());
        }

        [Fact]
        public void TestGetInstanceOfEdges()
        {
            var graph = new RDFExplorerGraph()
            {
                nodes = new[]
                {
                    new Node(0, "?var0" ),
                    new Node(1, "?var1", new[]{"http://www.wikidata.org/entity/Q5"}),
                },
                edges = new[]
                {
                    new Edge(0, "?prop0", 0, 1, new []{"http://www.wikidata.org/prop/direct/P31"})
                },
            };
            var queryGraph = new QueryGraph(graph);
            var node0 = queryGraph.Nodes[0];
            var instanceOfEdges0 = node0.GetInstanceOfEdges(queryGraph).ToArray();
            Assert.Single(instanceOfEdges0);
            Assert.Equal(0, instanceOfEdges0[0].id);

            var node1 = queryGraph.Nodes[1];
            var instanceOfEdges1 = node1.GetInstanceOfEdges(queryGraph).ToArray();
            Assert.Empty(instanceOfEdges1);
        }

        [Fact]
        public void TestGetEdgeSourceAndTargetNodes()
        {
            var graph = new RDFExplorerGraph()
            {
                nodes = new[]
                {
                    new Node(0, "?var0"),
                    new Node(1, "?var1", new[]{"http://www.wikidata.org/entity/Q5"} ),
                },
                edges = new[]
                {
                    new Edge(0, "?prop0", 0, 1, new []{"http://www.wikidata.org/prop/direct/P31"})
                },
            };
            var queryGraph = new QueryGraph(graph);
            var node0 = queryGraph.Nodes[0];
            var node1 = queryGraph.Nodes[1];
            var edge0 = queryGraph.Edges[0];

            Assert.Equal(node0, edge0.GetSourceNode(queryGraph));
            Assert.Equal(node1, edge0.GetTargetNode(queryGraph));
        }

        [Fact]
        public void TestGetNodeInstanceOfValues()
        {
            var graph = new RDFExplorerGraph()
            {
                nodes = new[]
                {
                    new Node(0, "?var0" ),
                    new Node(1, "?var1", new[]{"http://www.wikidata.org/entity/Q5"}),
                },
                edges = new[]
                {
                    new Edge(0, "?prop0", 0, 1, new []{"http://www.wikidata.org/prop/direct/P31"})
                },
            };
            var queryGraph = new QueryGraph(graph);
            var node0 = queryGraph.Nodes[0];

            Assert.Equal(new[] { "Q5" }, node0.GetInstanceOfValues(queryGraph).ToArray());
        }

        [Fact]
        public void TestGetNodeIncomingAndOutgoingEdges()
        {
            var graph = new RDFExplorerGraph()
            {
                nodes = new[]
                {
                    new Node(0, "?var0"),
                    new Node(1, "?var1" ),
                    new Node(2, "?var2" ),
                },
                edges = new[]
                {
                    new Edge(0, "?prop0", 0, 1),
                    new Edge(1, "?prop0", 1, 2)
                },
            };
            var queryGraph = new QueryGraph(graph);
            var node0 = queryGraph.Nodes[0];
            var node1 = queryGraph.Nodes[1];
            var node2 = queryGraph.Nodes[2];
            var edge0 = queryGraph.Edges[0];
            var edge1 = queryGraph.Edges[1];

            Assert.Single(node1.GetIncomingEdges(queryGraph));
            Assert.Equal(edge0, node1.GetIncomingEdges(queryGraph).First());

            Assert.Single(node1.GetOutgoingEdges(queryGraph));
            Assert.Equal(edge1, node1.GetOutgoingEdges(queryGraph).First());

            Assert.Single(node0.GetOutgoingEdges(queryGraph));
            Assert.Equal(edge0, node0.GetOutgoingEdges(queryGraph).First());

            Assert.Single(node2.GetIncomingEdges(queryGraph));
            Assert.Equal(edge1, node2.GetIncomingEdges(queryGraph).First());
        }

        [Fact]
        public void TestGetNodeIncomingAndOutgoingNodes()
        {
            var graph = new RDFExplorerGraph()
            {
                nodes = new[]
                {
                    new Node(0, "?var0"),
                    new Node(1, "?var1" ),
                    new Node(2, "?var2" ),
                },
                edges = new[]
                {
                    new Edge(0, "?prop0", 0, 1),
                    new Edge(1, "?prop0", 1, 2)
                },
            };
            var queryGraph = new QueryGraph(graph);
            var node0 = queryGraph.Nodes[0];
            var node1 = queryGraph.Nodes[1];
            var node2 = queryGraph.Nodes[2];

            Assert.Single(node1.GetIncomingNodes(queryGraph));
            Assert.Equal(node0, node1.GetIncomingNodes(queryGraph).First());

            Assert.Single(node1.GetOutgoingNodes(queryGraph));
            Assert.Equal(node2, node1.GetOutgoingNodes(queryGraph).First());
        }
    }
}
