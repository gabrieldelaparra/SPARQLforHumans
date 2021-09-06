using System.Linq;
using SparqlForHumans.Lucene.Queries.Graph;
using SparqlForHumans.Models.RDFExplorer;
using Xunit;

namespace SparqlForHumans.UnitTests.Query
{
    public class QueryGraphBaseTests
    {
        [Fact]
        public void TestEqualGraphs()
        {
            // Arrange
            var graph1 = new RDFExplorerGraph
            {
                nodes = new[]
                {
                    new Node(0, "?varDomain0"),
                    new Node(1, "?varRange1")
                },
                edges = new[]
                {
                    new Edge(0, "?CountryOfCitizenship", 0, 1, new[]{"http://www.wikidata.org/prop/direct/P27"}),
                    new Edge(1, "?propDomainRange1", 0, 1)

                }
            };

            var graph2 = new RDFExplorerGraph
            {
                nodes = new[]
                {
                    new Node(0, "?varDomain0"),
                    new Node(1, "?varRange1")
                },
                edges = new[]
                {
                    new Edge(0, "?CountryOfCitizenship", 0, 1, new[]{"http://www.wikidata.org/prop/direct/P27"}),
                    new Edge(1, "?propDomainRange1", 0, 1)

                }
            };

            //Are equal:
            Assert.Equal(graph1, graph2);

            //Node: Change name
            graph1.nodes[0].name = "cambio";
            Assert.Equal(graph1, graph2);


            //Node: Change Id
            graph1.nodes[0].id = 2;
            Assert.NotEqual(graph1, graph2);
            graph1.nodes[0].id = 0;
            Assert.Equal(graph1, graph2);

            //Node: Change uris
            graph1.nodes[0].uris = new[] { "uri1" };
            Assert.NotEqual(graph1, graph2);
            graph1.nodes[0].uris = new string[] { };
            Assert.Equal(graph1, graph2);

            //Node: Add node
            graph1.nodes = graph1.nodes.ToList().Append(new Node { id = 2, name = "new" }).ToArray();
            Assert.NotEqual(graph1, graph2);
            graph1.nodes = graph1.nodes.ToList().Take(2).ToArray();
            Assert.Equal(graph1, graph2);

            //Node: Remove Node
            graph1.nodes = graph1.nodes.ToList().Take(1).ToArray();
            Assert.NotEqual(graph1, graph2);
            graph1.nodes = graph1.nodes.ToList().Append(new Node { id = 1, name = "?varRange1" }).ToArray();
            Assert.Equal(graph1, graph2);

            //Edge: Change name
            graph1.edges[0].name = "cambio";
            Assert.Equal(graph1, graph2);

            //Edge: change Id
            graph1.edges[0].id = 2;
            Assert.NotEqual(graph1, graph2);
            graph1.edges[0].id = 0;
            Assert.Equal(graph1, graph2);

            //Edge: change source Id
            graph1.edges[0].sourceId = 2;
            Assert.NotEqual(graph1, graph2);
            graph1.edges[0].sourceId = 0;
            Assert.Equal(graph1, graph2);

            //Edge: change target Id
            graph1.edges[0].targetId = 2;
            Assert.NotEqual(graph1, graph2);
            graph1.edges[0].targetId = 1;
            Assert.Equal(graph1, graph2);

            //Edge: Change uris
            graph1.edges[0].uris = new string[] { };
            Assert.NotEqual(graph1, graph2);
            graph1.edges[0].uris = new[] { "http://www.wikidata.org/prop/direct/P27", "http://www.wikidata.org/prop/direct/P27" };
            Assert.NotEqual(graph1, graph2);
            graph1.edges[0].uris = new[] { "http://www.wikidata.org/prop/direct/P27" };
            Assert.Equal(graph1, graph2);

            //Edge: Add node
            graph1.edges = graph1.edges.ToList().Append(new Edge { id = 2, name = "new" }).ToArray();
            Assert.NotEqual(graph1, graph2);
            graph1.edges = graph1.edges.ToList().Take(2).ToArray();
            Assert.Equal(graph1, graph2);

            //Edge: Remove Node
            graph1.edges = graph1.edges.ToList().Take(1).ToArray();
            Assert.NotEqual(graph1, graph2);
            graph1.edges = graph1.edges.ToList().Append(new Edge { id = 1, name = "?warever", sourceId = 0, targetId = 0 }).ToArray();
            Assert.NotEqual(graph1, graph2);
            graph1.edges = graph1.edges.ToList().Take(1).ToArray();
            graph1.edges = graph1.edges.ToList().Append(new Edge { id = 1, name = "?warever", sourceId = 0, targetId = 1 }).ToArray();
            Assert.Equal(graph1, graph2);

            ////Selected
            //graph1.selected = new Selected { id = 0, isNode = true };
            //Assert.Equal(graph1, graph2);
            //graph1.selected = new Selected { id = 1, isNode = false };
            //Assert.Equal(graph1, graph2);
        }

        //[Fact]
        //public void TestPropertiesGoingToObamaShouldBeHumanProperties()
        //{
        //    var graph = new RDFExplorerGraph
        //    {
        //        nodes = new[]
        //        {
        //            new Node(0, "?var0"),
        //            new Node(1, "?var1",new[]{"http://www.wikidata.org/entity/Q76"} )
        //        },
        //        edges = new[]
        //        {
        //            new Edge (0, "?prop0", 0, 1)
        //        }
        //    };
        //    var queryGraph = new QueryGraph(graph);
        //    //Assert.Equal(QueryType.GivenObjectTypeQueryDirectlyEntities, queryGraph.Nodes[0].QueryType);
        //    //Assert.Equal(QueryType.GivenEntityTypeNoQuery, queryGraph.Nodes[1].QueryType);
        //    //Assert.Equal(QueryType.GivenObjectTypeDirectQueryIncomingProperties, queryGraph.Edges[0].QueryType);
        //}

        //[Fact]
        //public void TestUnknownSubjectKnownPredicateKnownObjectShouldThenNotBeInferred()
        //{
        //    var graph = new RDFExplorerGraph
        //    {
        //        nodes = new[]
        //        {
        //            new Node(0, "?siblingOfObama"),
        //            new Node(1, "OBAMA", new[]{"http://www.wikidata.org/entity/Q76"})
        //        },
        //        edges = new[]
        //        {
        //            new Edge(0, "sibling", 0, 1,  new[]{"http://www.wikidata.org/prop/direct/P3373"})
        //        }
        //    };
        //    var queryGraph = new QueryGraph(graph);
        //    Assert.Equal(QueryType.GivenObjectTypeQueryDirectlyEntities, queryGraph.Nodes[0].QueryType);
        //    Assert.Equal(QueryType.GivenEntityTypeNoQuery, queryGraph.Nodes[1].QueryType);
        //    Assert.Equal(QueryType.GivenPredicateTypeNoQuery, queryGraph.Edges[0].QueryType);
        //}

    }
}
