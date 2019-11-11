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
            Assert.Empty(queryGraph.Edges[0].Domain);
            Assert.Empty(queryGraph.Edges[0].Range);

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

    }
}
