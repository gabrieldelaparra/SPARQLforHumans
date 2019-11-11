using System;
using System.Collections.Generic;
using System.Text;
using SparqlForHumans.Lucene.Index;
using SparqlForHumans.Lucene.Queries.Graph;
using SparqlForHumans.Models.Query;
using SparqlForHumans.Utilities;
using Xunit;

namespace SparqlForHumans.UnitTests.Query
{
    public class QueryGraphTypesTests
    {
        const string EntitiesIndexPath = "QueryGraphEntities";
        const string PropertiesIndexPath = "QueryGraphProperties";

        public static void CreateIndex()
        {
            // Arrange
            const string filename = @"Resources/QueryGraph.nt";
            EntitiesIndexPath.DeleteIfExists();
            PropertiesIndexPath.DeleteIfExists();
            new EntitiesIndexer(filename, EntitiesIndexPath).Index();
            new PropertiesIndexer(filename, PropertiesIndexPath).Index();
        }

        public static void DeleteIndex()
        {
            EntitiesIndexPath.DeleteIfExists();
            PropertiesIndexPath.DeleteIfExists();
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
        public void TestTypes2ConnectedNodesSourceIsGivenType_2Nodes1Edge()
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
            // Arrange
            CreateIndex();

            var queryGraph = new QueryGraph(graph);
            Assert.Empty(queryGraph.Nodes[0].Types);
            Assert.Empty(queryGraph.Nodes[1].Types);
            Assert.Empty(queryGraph.Edges[0].Domain);
            Assert.Empty(queryGraph.Edges[0].Range);

            //Act
            queryGraph.SetTypesDomainsAndRanges(EntitiesIndexPath, PropertiesIndexPath);

            //Assert
            Assert.Single(queryGraph.Nodes[0].Types);
            Assert.Contains("http://www.wikidata.org/entity/Q76", queryGraph.Nodes[0].Types);
            Assert.Empty(queryGraph.Nodes[1].Types);

            Assert.Single(queryGraph.Edges[0].Domain);
            Assert.Contains("http://www.wikidata.org/entity/Q76", queryGraph.Edges[0].Domain);
            Assert.Empty(queryGraph.Edges[0].Range);

            //Cleanup
            DeleteIndex();
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
        public void TestTypes2ConnectedNodesTargetIsGivenType_2Nodes1Edge()
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
            // Arrange
            CreateIndex();

            var queryGraph = new QueryGraph(graph);
            Assert.Empty(queryGraph.Nodes[0].Types);
            Assert.Empty(queryGraph.Nodes[1].Types);
            Assert.Empty(queryGraph.Edges[0].Domain);
            Assert.Empty(queryGraph.Edges[0].Range);

            //Act
            queryGraph.SetTypesDomainsAndRanges(EntitiesIndexPath, PropertiesIndexPath);

            //Assert
            Assert.Empty(queryGraph.Nodes[0].Types);
            Assert.Single(queryGraph.Nodes[1].Types);
            Assert.Contains("http://www.wikidata.org/entity/Q76", queryGraph.Nodes[1].Types);

            Assert.Empty(queryGraph.Edges[0].Domain);
            Assert.Single(queryGraph.Edges[0].Range);
            Assert.Contains("http://www.wikidata.org/entity/Q76", queryGraph.Edges[0].Range);

            //Cleanup
            DeleteIndex();
        }

        /// <summary>
        /// ?var0 P31 ?var1
        /// ?var1 is Human
        /// 
        /// Expected Results:
        /// ?var0 are instances of Human
        /// </summary>
        [Fact]
        public void TestTypes2ConnectedNodesSubjectIsInstanceOfType_2Nodes1Edge()
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
            // Arrange
            CreateIndex();

            var queryGraph = new QueryGraph(graph);
            Assert.Empty(queryGraph.Nodes[0].Types);
            Assert.Empty(queryGraph.Nodes[1].Types);
            Assert.Empty(queryGraph.Edges[0].Domain);
            Assert.Empty(queryGraph.Edges[0].Range);

            //Act
            queryGraph.SetTypesDomainsAndRanges(EntitiesIndexPath, PropertiesIndexPath);

            //Assert
            Assert.Single(queryGraph.Nodes[0].Types);
            Assert.Contains("http://www.wikidata.org/entity/Q5", queryGraph.Nodes[0].Types);
            Assert.Single(queryGraph.Nodes[1].Types);
            Assert.Contains("http://www.wikidata.org/entity/Q5", queryGraph.Nodes[1].Types);
            Assert.Equal(8, queryGraph.Edges[0].Domain.Count);
            Assert.Contains("http://www.wikidata.org/entity/Q5", queryGraph.Edges[0].Domain);
            Assert.Empty(queryGraph.Edges[0].Range);

            //Cleanup
            DeleteIndex();
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
        public void TestTypes3ConnectedNodes_N0InstanceOfN1_E1DomainN0_3Nodes2Edge()
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

            // Arrange
            CreateIndex();

            var queryGraph = new QueryGraph(graph);
            Assert.Empty(queryGraph.Nodes[0].Types);
            Assert.Empty(queryGraph.Nodes[1].Types);
            Assert.Empty(queryGraph.Nodes[2].Types);
            Assert.Empty(queryGraph.Edges[0].Domain);
            Assert.Empty(queryGraph.Edges[0].Range);
            Assert.Empty(queryGraph.Edges[1].Domain);
            Assert.Empty(queryGraph.Edges[1].Range);

            //Act
            queryGraph.SetTypesDomainsAndRanges(EntitiesIndexPath, PropertiesIndexPath);

            //Assert
            Assert.Single(queryGraph.Nodes[0].Types);
            Assert.Contains("http://www.wikidata.org/entity/Q5", queryGraph.Nodes[0].Types);
            Assert.Single(queryGraph.Nodes[1].Types);
            Assert.Contains("http://www.wikidata.org/entity/Q5", queryGraph.Nodes[1].Types);
            Assert.Empty(queryGraph.Nodes[2].Types);

            Assert.Equal(8, queryGraph.Edges[0].Domain.Count);
            Assert.Contains("http://www.wikidata.org/entity/Q5", queryGraph.Edges[0].Domain);
            Assert.Empty(queryGraph.Edges[0].Range);
            Assert.Equal(8, queryGraph.Edges[0].Domain.Count);
            Assert.Contains("http://www.wikidata.org/entity/Q5", queryGraph.Edges[0].Domain);
            Assert.Empty(queryGraph.Edges[0].Range);

            //Since E1 source is HUMAN, the properties available, should be properties only.
            Assert.Single(queryGraph.Edges[1].Domain);
            Assert.Contains("http://www.wikidata.org/entity/Q5", queryGraph.Edges[1].Domain);
            Assert.Empty(queryGraph.Edges[1].Range);

            //Cleanup
            DeleteIndex();

            //var queryGraph = new QueryGraph(graph);

            //Assert.Equal(QueryType.SubjectIsInstanceOfTypeQueryEntities, queryGraph.Nodes[0].QueryType);
            //Assert.Equal(QueryType.GivenEntityTypeNoQuery, queryGraph.Nodes[1].QueryType);
            ////TODO: Not implemented, Currently returns top. Should return entities that have target on humans.
            ////TODO: I think that this case should not be covered for now, just top entities, until the edge is defined.
            ////TODO: On the other hand, the edge will have some 
            //Assert.Equal(QueryType.QueryTopEntities, queryGraph.Nodes[2].QueryType);

            //Assert.Equal(QueryType.GivenPredicateTypeNoQuery, queryGraph.Edges[0].QueryType);
            //Assert.Equal(QueryType.KnownSubjectTypeQueryDomainProperties, queryGraph.Edges[1].QueryType);
        }

    }
}
