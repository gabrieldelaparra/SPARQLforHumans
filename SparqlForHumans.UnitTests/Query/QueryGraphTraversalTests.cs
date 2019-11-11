using System;
using System.Collections.Generic;
using System.Text;
using SparqlForHumans.Lucene.Queries.Graph;
using SparqlForHumans.Models.Query;
using Xunit;

namespace SparqlForHumans.UnitTests.Query
{
    public class QueryGraphTraversalTests
    {
        /// <summary>
        /// A single node.
        /// Should query for the top Entities
        /// </summary>
        [Fact]
        public void TestTraversal1Node_1Node0Edge()
        {
            var graph = new RDFExplorerGraph()
            {
                nodes = new[] { new Node(0, "?var0") },
            };
            var queryGraph = new QueryGraph(graph);
            Assert.Equal(QueryType.QueryTopEntities, queryGraph.Nodes[0].QueryType);
        }

        /// <summary>
        /// A single node. Given Type.
        /// Should return given entity types.
        /// </summary>
        [Fact]
        public void TestTraversal1NodeGivenType_1Node0Edge()
        {
            var graph = new RDFExplorerGraph()
            {
                nodes = new[] { new Node(0, "?varObama", new[] { "http://www.wikidata.org/entity/Q76" }) },
            };
            var queryGraph = new QueryGraph(graph);
            Assert.Equal(QueryType.GivenEntityTypeNoQuery, queryGraph.Nodes[0].QueryType);
        }

        /// <summary>
        /// Two isolated nodes, no properties between them.
        /// Both should be Top Entities.
        /// </summary>
        [Fact]
        public void TestTraversal2IsolatedNodes_2Nodes0Edge()
        {
            var graph = new RDFExplorerGraph()
            {
                nodes = new[]
                {
                    new Node(0, "?var0"),
                    new Node(1, "?var1"),
                },
            };
            var queryGraph = new QueryGraph(graph);
            Assert.Equal(QueryType.QueryTopEntities, queryGraph.Nodes[0].QueryType);
            Assert.Equal(QueryType.QueryTopEntities, queryGraph.Nodes[1].QueryType);
        }

        /// <summary>
        /// Two isolated nodes, no properties between them.
        /// Both should be given entity types.
        /// </summary>
        [Fact]
        public void TestTraversal2IsolatedNodesGivenTypes_2Nodes0Edge()
        {
            var graph = new RDFExplorerGraph()
            {
                nodes = new[]
                {
                    new Node(0, "?var0", new[]{"http://www.wikidata.org/entity/Q76"}),
                    new Node(1, "?var1", new[]{"http://www.wikidata.org/entity/Q49089"}),
                },
            };
            var queryGraph = new QueryGraph(graph);
            Assert.Equal(QueryType.GivenEntityTypeNoQuery, queryGraph.Nodes[0].QueryType);
            Assert.Equal(QueryType.GivenEntityTypeNoQuery, queryGraph.Nodes[1].QueryType);
        }

        /// <summary>
        /// ?var0 ?prop0 ?var1
        /// 
        /// Expected Results:
        /// All should return Top Entities and Properties;
        /// </summary>
        [Fact]
        public void TestTraversal2ConnectedNodesNoGivenTypes_2Nodes1Edge()
        {
            var graph = new RDFExplorerGraph()
            {
                nodes = new[]
                {
                    new Node(0, "?var0"),
                    new Node(1, "?var1"),
                },
                edges = new[]
                {
                    new Edge(0, "?prop0", 0, 1)
                },
            };
            var queryGraph = new QueryGraph(graph);
            Assert.Equal(QueryType.QueryTopEntities, queryGraph.Nodes[0].QueryType);
            Assert.Equal(QueryType.QueryTopEntities, queryGraph.Nodes[1].QueryType);
            Assert.Equal(QueryType.QueryTopProperties, queryGraph.Edges[0].QueryType);
        }

        /// <summary>
        /// ?var0 ?prop0 ?var1
        /// ?var0 is Obama
        /// 
        /// Expected Results:
        /// ?var1 are entities coming from Obama
        /// ?prop0 are properties coming from Obama
        /// </summary>
        [Fact]
        public void TestTraversal2ConnectedNodesSourceIsGivenType_2Nodes1Edge()
        {
            var graph = new RDFExplorerGraph()
            {
                nodes = new[]
                {
                    new Node(0, "?var0", new[]{"http://www.wikidata.org/entity/Q76"} ),
                    new Node(1, "?var1"),
                },
                edges = new[]
                {
                    new Edge(0, "?prop0", 0, 1)
                },
            };
            var queryGraph = new QueryGraph(graph);
            Assert.Equal(QueryType.GivenEntityTypeNoQuery, queryGraph.Nodes[0].QueryType);
            Assert.Equal(QueryType.GivenSubjectTypeQueryDirectly, queryGraph.Nodes[1].QueryType);
            Assert.Equal(QueryType.GivenSubjectTypeQueryOutgoingProperties, queryGraph.Edges[0].QueryType);
        }

        /// <summary>
        /// ?var0 ?prop0 ?var1
        /// ?var1 is Obama
        /// 
        /// Expected Results:
        /// ?var0 are types going to Obama
        /// ?prop0 are properties going to Obama
        /// </summary>
        [Fact]
        public void TestTraversal2ConnectedNodesTargetIsGivenType_2Nodes1Edge()
        {
            var graph = new RDFExplorerGraph()
            {
                nodes = new[]
                {
                    new Node(0, "?var0" ),
                    new Node(1, "?var1", new[]{"http://www.wikidata.org/entity/Q76"}),
                },
                edges = new[]
                {
                    new Edge(0, "?prop0", 0, 1)
                },
            };
            var queryGraph = new QueryGraph(graph);
            Assert.Equal(QueryType.GivenObjectTypeQueryDirectly, queryGraph.Nodes[0].QueryType);
            Assert.Equal(QueryType.GivenEntityTypeNoQuery, queryGraph.Nodes[1].QueryType);
            Assert.Equal(QueryType.GivenObjectTypeQueryIncomingProperties, queryGraph.Edges[0].QueryType);
        }

        /// <summary>
        /// ?var0 ?prop0 ?var1
        /// ?var0 is Obama
        /// ?var1 is USA
        /// 
        /// Expected Results:
        /// ?prop0 are properties from Obama going to USA
        /// </summary>
        [Fact]
        public void TestTraversal2ConnectedNodesSourceAndTargetIsGivenType_2Nodes1Edge()
        {
            var graph = new RDFExplorerGraph()
            {
                nodes = new[]
                {
                    new Node(0, "?var0", new[]{"http://www.wikidata.org/entity/Q76"} ),
                    new Node(1, "?var1", new[]{"http://www.wikidata.org/entity/Q30"}),
                },
                edges = new[]
                {
                    new Edge(0, "?prop0", 0, 1)
                },
            };
            var queryGraph = new QueryGraph(graph);
            Assert.Equal(QueryType.GivenEntityTypeNoQuery, queryGraph.Nodes[0].QueryType);
            Assert.Equal(QueryType.GivenEntityTypeNoQuery, queryGraph.Nodes[1].QueryType);
            Assert.Equal(QueryType.GivenSubjectAndObjectTypeQueryIntersectOutInProperties, queryGraph.Edges[0].QueryType);
        }

        /// <summary>
        /// ?var0 P31 ?var1
        /// ?var1 is Human
        /// 
        /// Expected Results:
        /// ?var0 are instances of Human
        /// </summary>
        [Fact]
        public void TestTraversal2ConnectedNodesSubjectIsInstanceOfType_2Nodes1Edge()
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
                    new Edge(0, "?prop0", 0, 1,new []{"http://www.wikidata.org/prop/direct/P31"})
                },
            };
            var queryGraph = new QueryGraph(graph);
            Assert.Equal(QueryType.SubjectIsInstanceOfTypeQueryEntities, queryGraph.Nodes[0].QueryType);
            Assert.Equal(QueryType.GivenEntityTypeNoQuery, queryGraph.Nodes[1].QueryType);
            Assert.Equal(QueryType.GivenPredicateTypeNoQuery, queryGraph.Edges[0].QueryType);
        }

        /// <summary>
        /// ?var0 P31 ?var1
        /// ?var1 is Human
        /// ?var0 ?prop0 ?var2
        /// 
        /// Expected Results:
        /// ?var0 are instances of Human
        /// ?var2 are //TODO: TBD. For the moment, TopEntities;
        /// ?prop1 are properties with domain in Human 
        /// </summary>
        [Fact]
        public void TestTraversal3ConnectedNodes_N0InstanceOfN1_E1DomainN0_3Nodes2Edge()
        {
            var graph = new RDFExplorerGraph
            {
                nodes = new[]
                {
                    new Node(0, "?var0"),
                    new Node(1, "?var1", new[]{"http://www.wikidata.org/entity/Q5"}),
                    new Node(2, "?var2"),
                },
                edges = new[]
                {
                    new Edge(0, "?prop0", 0, 1, new[]{"http://www.wikidata.org/prop/direct/P31"}),
                    new Edge(1, "?prop1", 0, 2),
                }
            };
            var queryGraph = new QueryGraph(graph);

            Assert.Equal(QueryType.SubjectIsInstanceOfTypeQueryEntities, queryGraph.Nodes[0].QueryType);
            Assert.Equal(QueryType.GivenEntityTypeNoQuery, queryGraph.Nodes[1].QueryType);
            //TODO: Not implemented, Currently returns top. Should return entities that have target on humans.
            //TODO: I think that this case should not be covered for now, just top entities, until the edge is defined.
            //TODO: On the other hand, the edge will have some 
            Assert.Equal(QueryType.QueryTopEntities, queryGraph.Nodes[2].QueryType);

            Assert.Equal(QueryType.GivenPredicateTypeNoQuery, queryGraph.Edges[0].QueryType);
            Assert.Equal(QueryType.KnownSubjectTypeQueryDomainProperties, queryGraph.Edges[1].QueryType);
        }

        /// <summary>
        /// ?var0 P31 ?var1
        /// ?var1 is Human
        /// ?var2 ?prop0 ?var0
        /// 
        /// Expected Results:
        /// ?var0 are instances of Human
        /// ?var2 are //TODO: TBD. For the moment, TopEntities;
        /// ?prop1 are properties with range in Human 
        /// </summary>
        [Fact]
        public void TestTraversal3ConnectedNodes_N0InstanceOfN1_E1RangeN0_3Nodes2Edge()
        {
            var graph = new RDFExplorerGraph
            {
                nodes = new[]
                {
                    new Node(0, "?var0"),
                    new Node(1, "?var1", new[]{"http://www.wikidata.org/entity/Q5"}),
                    new Node(2, "?var2"),
                },
                edges = new[]
                {
                    new Edge(0, "?prop0", 0, 1, new[]{"http://www.wikidata.org/prop/direct/P31"}),
                    new Edge(1, "?prop1", 2, 0),
                }
            };
            var queryGraph = new QueryGraph(graph);

            Assert.Equal(QueryType.SubjectIsInstanceOfTypeQueryEntities, queryGraph.Nodes[0].QueryType);
            Assert.Equal(QueryType.GivenEntityTypeNoQuery, queryGraph.Nodes[1].QueryType);
            //TODO: Not implemented, Currently returns top. Should return entities that have target on humans.
            //TODO: I think that this case should not be covered for now, just top entities, until the edge is defined.
            //TODO: On the other hand, the edge will have some 
            Assert.Equal(QueryType.QueryTopEntities, queryGraph.Nodes[2].QueryType);

            Assert.Equal(QueryType.GivenPredicateTypeNoQuery, queryGraph.Edges[0].QueryType);
            Assert.Equal(QueryType.KnownObjectTypeQueryRangeProperties, queryGraph.Edges[1].QueryType);
        }

        /// <summary>
        /// ?human ?prop0 ?city
        /// ?human P31 HUMAN
        /// ?city P31 CITY
        ///
        /// Expected:
        /// ?human IsInstanceOf HUMAN
        /// ?city IsInstanceOf CITY
        /// ?prop0 Intersect Domain HUMAN Range CITY
        /// </summary>
        [Fact]
        public void TestTraversal4ConnectedNodes_N1InstanceOfN3_N2InstanceOfN4_N1E1N2_E1DomainN1RangeN2_4Nodes3Edge()
        {
            var graph = new RDFExplorerGraph
            {
                nodes = new[]
                {
                    new Node(0, "?human"),
                    new Node(1, "?city"),
                    new Node(2, "human", new[]{"http://www.wikidata.org/entity/Q5"}),
                    new Node(3, "city", new[]{"http://www.wikidata.org/entity/Q515"}),
                },
                edges = new[]
                {
                    new Edge(0, "?prop0", 0,1),
                    new Edge(1, "?type1", 0, 2, new[]{"http://www.wikidata.org/prop/direct/P31"}),
                    new Edge(2, "?type2", 1, 3, new[]{"http://www.wikidata.org/prop/direct/P31"}),
                }
            };
            var queryGraph = new QueryGraph(graph);
            
            Assert.Equal(QueryType.SubjectIsInstanceOfTypeQueryEntities, queryGraph.Nodes[0].QueryType);
            Assert.Equal(QueryType.SubjectIsInstanceOfTypeQueryEntities, queryGraph.Nodes[1].QueryType);
            Assert.Equal(QueryType.GivenEntityTypeNoQuery, queryGraph.Nodes[2].QueryType);
            Assert.Equal(QueryType.GivenEntityTypeNoQuery, queryGraph.Nodes[3].QueryType);

            Assert.Equal(QueryType.KnownSubjectAndObjectTypesIntersectDomainRangeProperties, queryGraph.Edges[0].QueryType);
            Assert.Equal(QueryType.GivenPredicateTypeNoQuery, queryGraph.Edges[1].QueryType);
            Assert.Equal(QueryType.GivenPredicateTypeNoQuery, queryGraph.Edges[2].QueryType);
        }
    }
}
