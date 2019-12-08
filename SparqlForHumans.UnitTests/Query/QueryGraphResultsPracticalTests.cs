using System.Linq;
using SparqlForHumans.Lucene;
using SparqlForHumans.Lucene.Index;
using SparqlForHumans.Lucene.Queries.Graph;
using SparqlForHumans.Models.RDFExplorer;
using SparqlForHumans.Utilities;
using Xunit;

namespace SparqlForHumans.UnitTests.Query
{
    public class QueryGraphResultsPracticalTests
    {
        private static void CreateIndex(string filename, string entitiesIndexPath, string propertiesIndexPath)
        {
            // Arrange
            entitiesIndexPath.DeleteIfExists();
            propertiesIndexPath.DeleteIfExists();
            new EntitiesIndexer(filename, entitiesIndexPath).Index();
            new SimplePropertiesIndexer(filename, propertiesIndexPath).Index();
        }

        private static void DeleteIndex(string entitiesIndexPath, string propertiesIndexPath)
        {
            entitiesIndexPath.DeleteIfExists();
            propertiesIndexPath.DeleteIfExists();
        }

        /// <summary>
        /// The following query has no results.
        /// P180 should not be in the given results
        /// PREFIX wdt: <http://www.wikidata.org/prop/direct/>
        ///    SELECT *
        ///        WHERE {
        ///        ?var2 wdt:P180 wd:Q6581072 .
        ///        ?var2 wdt:P31 wd:Q5 .
        ///        SERVICE wikibase:label { bd:serviceParam wikibase:language "[AUTO_LANGUAGE],en". }
        ///}
        ///LIMIT 100
        /// </summary>
        [Fact]
        public void TestResults_Practical_OutgoingPropertiesOfKnownInstanceOfTypeShouldBeReducedInPossibilities()
        {
            var graph = new RDFExplorerGraph
            {
                nodes = new[]
                {
                    new Node(0, "?var1"),
                    new Node(1, "?HUMAN", new[]{"http://www.wikidata.org/entity/Q5"}),
                    new Node(2, "?FEMALE",new[]{"http://www.wikidata.org/entity/Q6581072"}),
                },
                edges = new[]
                {
                    new Edge(0, "?instanceOf", 0, 1, new[]{"http://www.wikidata.org/prop/direct/P31"}),
                    new Edge(1, "?shouldBeGender", 0, 2),
                }
            };

            //Arrange:
            var filename = "Resources/QueryGraphPracticalResults1-Sorted.nt";
            var entitiesIndexPath = "PracticalResultsEntitiesIndex";
            var propertiesIndexPath = "PracticalResultsPropertiesIndex";

            CreateIndex(filename, entitiesIndexPath, propertiesIndexPath);

            // Act
            var queryGraph = new QueryGraph(graph);
            queryGraph.GetGraphQueryResults(entitiesIndexPath, propertiesIndexPath, false);

            var nodes = queryGraph.Nodes.Select(x=>x.Value).ToArray();
            var edges = queryGraph.Edges.Select(x=>x.Value).ToArray();
            var genderEdge = edges[1];
            var actualResults = genderEdge.Results.Select(x => x.Label).ToList();
            Assert.Contains("sex or gender", actualResults);

            Assert.DoesNotContain("depicts", actualResults);

            // Cleanup
            DeleteIndex(entitiesIndexPath, propertiesIndexPath);
        }

        //TODO: Create Sample for testing. Do not run on full Index;
        [Fact]
        public void TestResults_Practical_OutgoingPropertiesOfKnownOutgoingTypeShouldBeReducedInPossibilities()
        {
            var graph = new RDFExplorerGraph
            {
                nodes = new[]
                {
                    new Node(0, "?var1"),
                    new Node(1, "?MAYOR", new[]{"http://www.wikidata.org/entity/Q30185"}),
                    new Node(2, "?FEMALE",new[]{"http://www.wikidata.org/entity/Q6581072"}),
                },
                edges = new[]
                {
                    new Edge(0, "?POSITION_HELD", 0, 1, new[]{"http://www.wikidata.org/prop/direct/P39"}),
                    new Edge(1, "?shouldBeGender", 0, 2),
                }
            };

            //Arrange:
            var filename = "Resources/QueryGraphPracticalResults1-Sorted.nt";
            var entitiesIndexPath = "PracticalResultsEntitiesIndex";
            var propertiesIndexPath = "PracticalResultsPropertiesIndex";

            CreateIndex(filename, entitiesIndexPath, propertiesIndexPath);

            // Act
            var queryGraph = new QueryGraph(graph);
            queryGraph.GetGraphQueryResults(entitiesIndexPath, propertiesIndexPath, false);

            var nodes = queryGraph.Nodes.Select(x=>x.Value).ToArray();
            var edges = queryGraph.Edges.Select(x=>x.Value).ToArray();
            var genderEdge = edges[1];
            var actualResults = genderEdge.Results.Select(x => x.Label).ToList();
            Assert.Contains("sex or gender", actualResults);

            Assert.DoesNotContain("depicts", actualResults);

            // Cleanup
            DeleteIndex(entitiesIndexPath, propertiesIndexPath);
        }

        [Fact]
        public void TestResults_SampleFullIndex_1_GoingToHumanInstanceOfTypeShouldBeThere()
        {
            var graph = new RDFExplorerGraph
            {
                nodes = new[]
                {
                    new Node(0, "?var1"),
                    new Node(1, "?HUMAN", new[]{"http://www.wikidata.org/entity/Q5"}),
                },
                edges = new[]
                {
                    new Edge(0, "?instanceOf", 0, 1),
                }
            };

            // Act
            var queryGraph = new QueryGraph(graph);
            queryGraph.GetGraphQueryResults(LuceneDirectoryDefaults.EntityIndexPath, LuceneDirectoryDefaults.PropertyIndexPath, false);

            var edges = queryGraph.Edges.Select(x=>x.Value).ToArray();
            var edge = edges[0];
            var actualResults = edge.Results.Select(x => x.Label).ToList();
            Assert.Contains("instance of", actualResults);
        }

        [Fact]
        public void TestResults_SampleFullIndex_OutgoingPropertiesOfKnownOutgoingTypeShouldBeReducedInPossibilities()
        {
            var graph = new RDFExplorerGraph
            {
                nodes = new[]
                {
                    new Node(0, "?var1"),
                    new Node(1, "?MAYOR", new[]{"http://www.wikidata.org/entity/Q30185"}),
                    new Node(2, "?FEMALE",new[]{"http://www.wikidata.org/entity/Q6581072"}),
                },
                edges = new[]
                {
                    new Edge(0, "?POSITION_HELD", 0, 1, new[]{"http://www.wikidata.org/prop/direct/P39"}),
                    new Edge(1, "?shouldBeGender", 0, 2),
                }
            };


            // Act
            var queryGraph = new QueryGraph(graph);
            queryGraph.GetGraphQueryResults(LuceneDirectoryDefaults.EntityIndexPath, LuceneDirectoryDefaults.PropertyIndexPath, false);

            var nodes = queryGraph.Nodes.Select(x=>x.Value).ToArray();
            var edges = queryGraph.Edges.Select(x=>x.Value).ToArray();
            var genderEdge = edges[1];
            var actualResults = genderEdge.Results.Select(x => x.Label).ToList();
            Assert.Contains("sex or gender", actualResults);

            Assert.DoesNotContain("has part", actualResults);
            Assert.DoesNotContain("opposite of", actualResults);
            Assert.DoesNotContain("is a list of", actualResults);

            //Assert.DoesNotContain("different from", actualResults);
            //Assert.DoesNotContain("field of work", actualResults);

        }
        
        [Fact]
        public void TestResults_SampleFullIndex_OutgoingPropertiesOfKnownInstanceOfTypeShouldBeReducedInPossibilities()
        {
            var graph = new RDFExplorerGraph
            {
                nodes = new[]
                {
                    new Node(0, "?var1"),
                    new Node(1, "?HUMAN", new[]{"http://www.wikidata.org/entity/Q5"}),
                    new Node(2, "?FEMALE",new[]{"http://www.wikidata.org/entity/Q6581072"}),
                },
                edges = new[]
                {
                    new Edge(0, "?instanceOf", 0, 1, new[]{"http://www.wikidata.org/prop/direct/P31"}),
                    new Edge(1, "?shouldBeGender", 0, 2),
                }
            };

            // Act
            var queryGraph = new QueryGraph(graph);
            queryGraph.GetGraphQueryResults(LuceneDirectoryDefaults.EntityIndexPath, LuceneDirectoryDefaults.PropertyIndexPath, false);

            var nodes = queryGraph.Nodes.Select(x=>x.Value).ToArray();
            var edges = queryGraph.Edges.Select(x=>x.Value).ToArray();
            var genderEdge = edges[1];
            var actualResults = genderEdge.Results.Select(x => x.Label).ToList();
            Assert.Contains("sex or gender", actualResults);

            Assert.DoesNotContain("has part", actualResults);
            Assert.DoesNotContain("opposite of", actualResults);
            Assert.DoesNotContain("is a list of", actualResults);

            //FAILS ON THESE:
            Assert.DoesNotContain("different from", actualResults);
            Assert.DoesNotContain("field of work", actualResults);
        }

    }
}
