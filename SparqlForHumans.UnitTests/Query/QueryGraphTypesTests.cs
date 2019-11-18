using SparqlForHumans.Lucene.Index;
using SparqlForHumans.Lucene.Queries.Graph;
using SparqlForHumans.Models.RDFExplorer;
using SparqlForHumans.Utilities;
using Xunit;

namespace SparqlForHumans.UnitTests.Query
{
    public class QueryGraphTypesTests
    {
        const string EntitiesIndexPath = "QueryGraphTypesEntities";
        const string PropertiesIndexPath = "QueryGraphTypesProperties";

        public static void CreateIndex()
        {
            // Arrange
            const string filename = @"Resources/QueryGraphTypes.nt";
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
            //Assert.Empty(queryGraph.Edges[0].Domain);
            //Assert.Empty(queryGraph.Edges[0].Range);

            //Act
            queryGraph.SetTypesDomainsAndRanges(EntitiesIndexPath, PropertiesIndexPath);

            //Assert
            Assert.Single(queryGraph.Nodes[0].Types);
            Assert.Contains("http://www.wikidata.org/entity/Q76", queryGraph.Nodes[0].Types);
            Assert.Empty(queryGraph.Nodes[1].Types);

            //Assert.Single(queryGraph.Edges[0].Domain);
            //Assert.Contains("http://www.wikidata.org/entity/Q76", queryGraph.Edges[0].Domain);
            //Assert.Empty(queryGraph.Edges[0].Range);

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
            //Assert.Empty(queryGraph.Edges[0].Domain);
            //Assert.Empty(queryGraph.Edges[0].Range);

            //Act
            queryGraph.SetTypesDomainsAndRanges(EntitiesIndexPath, PropertiesIndexPath);

            //Assert
            Assert.Empty(queryGraph.Nodes[0].Types);
            Assert.Single(queryGraph.Nodes[1].Types);
            Assert.Contains("http://www.wikidata.org/entity/Q76", queryGraph.Nodes[1].Types);

            //Assert.Empty(queryGraph.Edges[0].Domain);
            //Assert.Single(queryGraph.Edges[0].Range);
            //Assert.Contains("http://www.wikidata.org/entity/Q76", queryGraph.Edges[0].Range);

            //Cleanup
            DeleteIndex();
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
        public void TestTypes2ConnectedNodesSourceAndTargetIsGivenType_2Nodes1Edge()
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
            // Arrange
            CreateIndex();

            var queryGraph = new QueryGraph(graph);
            Assert.Empty(queryGraph.Nodes[0].Types);
            Assert.Empty(queryGraph.Nodes[1].Types);
            //Assert.Empty(queryGraph.Edges[0].Domain);
            //Assert.Empty(queryGraph.Edges[0].Range);

            //Act
            queryGraph.SetTypesDomainsAndRanges(EntitiesIndexPath, PropertiesIndexPath);

            //Assert
            Assert.Contains("http://www.wikidata.org/entity/Q76", queryGraph.Nodes[0].Types);
            Assert.Contains("http://www.wikidata.org/entity/Q30", queryGraph.Nodes[1].Types);

            //Assert.Contains("http://www.wikidata.org/entity/Q76", queryGraph.Edges[0].Domain);
            //Assert.Contains("http://www.wikidata.org/entity/Q30", queryGraph.Edges[0].Range);

            //Cleanup
            DeleteIndex();
        }

        /// <summary>
        /// ?var0 P27 ?var1
        /// ?var0 is Obama
        /// ?var1 is USA
        /// 
        /// Expected Results:
        /// All is given, should behave like that.
        /// </summary>
        [Fact]
        public void TestTypes2ConnectedNodesSourceAndTargetIsGivenType_E0N0P27E1_2Nodes1Edge()
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
                    new Edge(0, "?prop0", 0, 1, new[]{"http://www.wikidata.org/prop/direct/P27"})
                },
            };
            // Arrange
            CreateIndex();

            var queryGraph = new QueryGraph(graph);
            Assert.Empty(queryGraph.Nodes[0].Types);
            Assert.Empty(queryGraph.Nodes[1].Types);
            //Assert.Empty(queryGraph.Edges[0].Domain);
            //Assert.Empty(queryGraph.Edges[0].Range);

            //Act
            queryGraph.SetTypesDomainsAndRanges(EntitiesIndexPath, PropertiesIndexPath);

            //Assert
            Assert.Contains("http://www.wikidata.org/entity/Q76", queryGraph.Nodes[0].Types);
            Assert.Contains("http://www.wikidata.org/entity/Q30", queryGraph.Nodes[1].Types);

            //Assert.Contains("http://www.wikidata.org/entity/Q76", queryGraph.Edges[0].Domain);
            //Assert.Contains("http://www.wikidata.org/entity/Q30", queryGraph.Edges[0].Range);

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
            //Assert.Empty(queryGraph.Edges[0].Domain);
            //Assert.Empty(queryGraph.Edges[0].Range);

            //Act
            queryGraph.SetTypesDomainsAndRanges(EntitiesIndexPath, PropertiesIndexPath);

            //Assert
            Assert.Single(queryGraph.Nodes[0].Types);
            Assert.Contains("http://www.wikidata.org/entity/Q5", queryGraph.Nodes[0].Types);
            Assert.Single(queryGraph.Nodes[1].Types);
            Assert.Contains("http://www.wikidata.org/entity/Q5", queryGraph.Nodes[1].Types);
            
            //Assert.Equal(8, queryGraph.Edges[0].Domain.Count);
            //Assert.Contains("http://www.wikidata.org/entity/Q5", queryGraph.Edges[0].Domain);
            ////TODO/FIX: Check if this is right. This is P31. Range should be available
            ////TODO/FIX: InstanceOf Edge. Domain contains Obama. Range contains HUMAN.
            //Assert.Empty(queryGraph.Edges[0].Range);

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
            //Assert.Empty(queryGraph.Edges[0].Domain);
            //Assert.Empty(queryGraph.Edges[0].Range);
            //Assert.Empty(queryGraph.Edges[1].Domain);
            //Assert.Empty(queryGraph.Edges[1].Range);

            //Act
            queryGraph.SetTypesDomainsAndRanges(EntitiesIndexPath, PropertiesIndexPath);

            //Assert
            Assert.Single(queryGraph.Nodes[0].Types);
            Assert.Contains("http://www.wikidata.org/entity/Q5", queryGraph.Nodes[0].Types);

            Assert.Single(queryGraph.Nodes[1].Types);
            Assert.Contains("http://www.wikidata.org/entity/Q5", queryGraph.Nodes[1].Types);

            Assert.Empty(queryGraph.Nodes[2].Types);

            //Assert.Equal(8, queryGraph.Edges[0].Domain.Count);
            //Assert.Contains("http://www.wikidata.org/entity/Q5", queryGraph.Edges[0].Domain);
            ////TODO/FIX: Check if this is right. This is P31. Range should be available
            ////TODO/FIX: InstanceOf Edge. Domain contains Obama. Range contains HUMAN.
            //Assert.Empty(queryGraph.Edges[0].Range);

            ////Since E1 source is HUMAN, the properties available, should be HUMAN properties only.
            //Assert.Single(queryGraph.Edges[1].Domain);
            //Assert.Contains("http://www.wikidata.org/entity/Q5", queryGraph.Edges[1].Domain);
            //Assert.Empty(queryGraph.Edges[1].Range);

            //Cleanup
            DeleteIndex();
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
        public void TestTypes3ConnectedNodes_N0InstanceOfN1_E1RangeN0_3Nodes2Edge()
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


            // Arrange
            CreateIndex();

            var queryGraph = new QueryGraph(graph);
            Assert.Empty(queryGraph.Nodes[0].Types);
            Assert.Empty(queryGraph.Nodes[1].Types);
            Assert.Empty(queryGraph.Nodes[2].Types);
            //Assert.Empty(queryGraph.Edges[0].Domain);
            //Assert.Empty(queryGraph.Edges[0].Range);
            //Assert.Empty(queryGraph.Edges[1].Domain);
            //Assert.Empty(queryGraph.Edges[1].Range);

            //Act
            queryGraph.SetTypesDomainsAndRanges(EntitiesIndexPath, PropertiesIndexPath);

            //Assert
            Assert.Single(queryGraph.Nodes[0].Types);
            Assert.Contains("http://www.wikidata.org/entity/Q5", queryGraph.Nodes[0].Types);

            Assert.Single(queryGraph.Nodes[1].Types);
            Assert.Contains("http://www.wikidata.org/entity/Q5", queryGraph.Nodes[1].Types);

            Assert.Empty(queryGraph.Nodes[2].Types);

            //Assert.Equal(8, queryGraph.Edges[0].Domain.Count);
            //Assert.Contains("http://www.wikidata.org/entity/Q5", queryGraph.Edges[0].Domain);
            ////TODO/FIX: Check if this is right. This is P31. Range should be available
            ////TODO/FIX: InstanceOf Edge. Domain contains Obama. Range contains HUMAN.
            //Assert.Empty(queryGraph.Edges[0].Range);

            //Assert.Empty(queryGraph.Edges[1].Domain);
            //Assert.Single(queryGraph.Edges[1].Range);
            //Assert.Contains("http://www.wikidata.org/entity/Q5", queryGraph.Edges[1].Range);

            //Cleanup
            DeleteIndex();
        }


        /// <summary>
        /// ?human ?prop0 ?country
        /// ?human P31 HUMAN
        /// ?country P31 COUNTRY
        ///
        /// Expected:
        /// ?human IsInstanceOf HUMAN
        /// ?city IsInstanceOf COUNTRY
        /// ?prop0 Intersect Domain HUMAN Range COUNTRY
        /// </summary>
        [Fact]
        public void TestTypes4ConnectedNodes_N1InstanceOfN3_N2InstanceOfN4_N1E1N2_E1DomainN1RangeN2_4Nodes3Edge()
        {
            var graph = new RDFExplorerGraph
            {
                nodes = new[]
                {
                    new Node(0, "?human"),
                    new Node(1, "?country"),
                    new Node(2, "human", new[]{"http://www.wikidata.org/entity/Q5"}),
                    new Node(3, "country", new[]{"http://www.wikidata.org/entity/Q6256"}),
                },
                edges = new[]
                {
                    new Edge(0, "?prop0", 0, 1),
                    new Edge(1, "?type1", 0, 2, new[]{"http://www.wikidata.org/prop/direct/P31"}),
                    new Edge(2, "?type2", 1, 3, new[]{"http://www.wikidata.org/prop/direct/P31"}),
                }
            };

            // Arrange
            CreateIndex();

            var queryGraph = new QueryGraph(graph);
            Assert.Empty(queryGraph.Nodes[0].Types);
            Assert.Empty(queryGraph.Nodes[1].Types);
            Assert.Empty(queryGraph.Nodes[2].Types);
            //Assert.Empty(queryGraph.Edges[0].Domain);
            //Assert.Empty(queryGraph.Edges[0].Range);
            //Assert.Empty(queryGraph.Edges[1].Domain);
            //Assert.Empty(queryGraph.Edges[1].Range);

            //Act
            queryGraph.SetTypesDomainsAndRanges(EntitiesIndexPath, PropertiesIndexPath);

            //Assert
            Assert.Single(queryGraph.Nodes[0].Types);
            Assert.Contains("http://www.wikidata.org/entity/Q5", queryGraph.Nodes[0].Types);

            Assert.Single(queryGraph.Nodes[1].Types);
            Assert.Contains("http://www.wikidata.org/entity/Q6256", queryGraph.Nodes[1].Types);

            Assert.Single(queryGraph.Nodes[2].Types);
            Assert.Contains("http://www.wikidata.org/entity/Q5", queryGraph.Nodes[2].Types);

            Assert.Single(queryGraph.Nodes[3].Types);
            Assert.Contains("http://www.wikidata.org/entity/Q6256", queryGraph.Nodes[3].Types);

            ////From InstanceOf HUMAN to InstanceOf COUNTRY
            //Assert.Single(queryGraph.Edges[0].Domain);
            //Assert.Contains("http://www.wikidata.org/entity/Q5", queryGraph.Edges[0].Domain);
            //Assert.Single(queryGraph.Edges[0].Range);
            //Assert.Contains("http://www.wikidata.org/entity/Q6256", queryGraph.Edges[0].Range);

            ////InstanceOf Edge. Domain contains ?human (Obama). Range contains HUMAN.
            //Assert.Equal(8, queryGraph.Edges[1].Domain.Count);
            //Assert.Contains("http://www.wikidata.org/entity/Q5", queryGraph.Edges[1].Domain);
            ////TODO/FIX: Check if this is right. This is P31. Range should be available.
            ////TODO/FIX: InstanceOf Edge. Domain contains ?human (Obama). Range contains HUMAN.
            //Assert.Empty(queryGraph.Edges[1].Range);

            ////InstanceOf Edge. Domain contains ?city. Range contains COUNTRY.
            //Assert.Equal(8, queryGraph.Edges[2].Domain.Count);
            //Assert.Contains("http://www.wikidata.org/entity/Q6256", queryGraph.Edges[2].Domain);
            ////TODO/FIX: Check if this is right. This is P31. Range should be available.
            ////TODO/FIX: InstanceOf Edge. Domain contains ?country (USA). Range contains COUNTRY.
            //Assert.Empty(queryGraph.Edges[2].Range);

            //Cleanup
            DeleteIndex();
        }
    }
}
