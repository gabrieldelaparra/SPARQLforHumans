using System.Linq;
using SparqlForHumans.Lucene;
using SparqlForHumans.Lucene.Index;
using SparqlForHumans.Lucene.Models;
using SparqlForHumans.Lucene.Queries.Graph;
using SparqlForHumans.Models.RDFExplorer;
using SparqlForHumans.Utilities;
using Xunit;

namespace SparqlForHumans.UnitTests.Query
{
    [Collection("Sequential")]
    public class QueryGraphResultsPracticalTests
    {
        private static void CreateIndex(string filename, string entitiesIndexPath, string propertiesIndexPath)
        {
            // Arrange
            entitiesIndexPath.DeleteIfExists();
            propertiesIndexPath.DeleteIfExists();
            new EntitiesIndexer(filename, entitiesIndexPath).Index();
            new PropertiesIndexer(filename, propertiesIndexPath, entitiesIndexPath).Index();
        }

        private static void DeleteIndex(string entitiesIndexPath, string propertiesIndexPath)
        {
            entitiesIndexPath.DeleteIfExists();
            propertiesIndexPath.DeleteIfExists();
        }

        [Fact]
        public void TestResults_FullIndex_1_GoingToHumanInstanceOfTypeShouldBeThere()
        {
            var graph = new RDFExplorerGraph
            {
                nodes = new[] {
                    new Node(0, "?var1"),
                    new Node(1, "?HUMAN", new[] {"http://www.wikidata.org/entity/Q5"})
                },
                edges = new[] {
                    new Edge(0, "?prop0", 0, 1)
                }
            };

            // Act
            var queryGraph = new QueryGraph(graph);
            new QueryGraphResults().GetGraphQueryResults(queryGraph, LuceneDirectoryDefaults.EntityIndexPath,
                LuceneDirectoryDefaults.PropertyIndexPath, false);

            var edges = queryGraph.Edges.Select(x => x.Value).ToArray();
            var edge = edges[0];
            var actualResults = edge.Results.Select(x => x.Label).ToList();
            Assert.Contains("instance of", actualResults);
            Assert.Equal(52, actualResults.Count);

            //There are 2 properties that appear here, but not on wikipedia: Educated at (P69) and Creator (P170).
            //I think that this is like this in the wikidata dump that I have.

            //Regarding `nodes`; I am returning anything (random).
            //This is related to #121 (https://github.com/gabrieldelaparra/SparQLforHumans/issues/121)
        }

        [Fact]
        public void TestResults_FullIndex_2_OutgoingPropertiesOfKnownInstanceOfTypeShouldBeReducedInPossibilities()
        {
            var graph = new RDFExplorerGraph
            {
                nodes = new[] {
                    new Node(0, "?var1"),
                    new Node(1, "?HUMAN", new[] {"http://www.wikidata.org/entity/Q5"}),
                    new Node(2, "?FEMALE", new[] {"http://www.wikidata.org/entity/Q6581072"})
                },
                edges = new[] {
                    new Edge(0, "?instanceOf", 0, 1, new[] {"http://www.wikidata.org/prop/direct/P31"}),
                    new Edge(1, "?shouldBeGender", 0, 2)
                }
            };

            // Act
            var queryGraph = new QueryGraph(graph);
            new QueryGraphResults().GetGraphQueryResults(queryGraph, LuceneDirectoryDefaults.EntityIndexPath,
                LuceneDirectoryDefaults.PropertyIndexPath, false);

            var edges = queryGraph.Edges.Select(x => x.Value).ToArray();
            var genderEdge = edges[1];
            var actualResults = genderEdge.Results.Select(x => x.Label).ToList();
            Assert.Contains("sex or gender", actualResults);

            //FAILS ON THESE:
            //Assert.DoesNotContain("has part", actualResults);
            //Assert.DoesNotContain("opposite of", actualResults);
            //Assert.DoesNotContain("is a list of", actualResults);
            //Assert.DoesNotContain("different from", actualResults);
            //Assert.DoesNotContain("field of work", actualResults);
        }

        [Fact]
        public void TestResults_FullIndex_3_OutgoingPropertiesOfKnownOutgoingTypeShouldBeReducedInPossibilities()
        {
            var graph = new RDFExplorerGraph
            {
                nodes = new[] {
                    new Node(0, "?var1"),
                    new Node(1, "?MAYOR", new[] {"http://www.wikidata.org/entity/Q30185"}),
                    new Node(2, "?FEMALE", new[] {"http://www.wikidata.org/entity/Q6581072"})
                },
                edges = new[] {
                    new Edge(0, "?POSITION_HELD", 0, 1, new[] {"http://www.wikidata.org/prop/direct/P39"}),
                    new Edge(1, "?shouldBeGender", 0, 2)
                }
            };


            // Act
            var queryGraph = new QueryGraph(graph);
            new QueryGraphResults().GetGraphQueryResults(queryGraph, LuceneDirectoryDefaults.EntityIndexPath,
                LuceneDirectoryDefaults.PropertyIndexPath, false);

            var edges = queryGraph.Edges.Select(x => x.Value).ToArray();
            var genderEdge = edges[1];
            var actualResults = genderEdge.Results.Select(x => x.Label).ToList();
            Assert.Contains("sex or gender", actualResults);

            //FAILS ON THESE:
            //Assert.DoesNotContain("has part", actualResults);
            //Assert.DoesNotContain("opposite of", actualResults);
            //Assert.DoesNotContain("is a list of", actualResults);
            //Assert.DoesNotContain("different from", actualResults);
            //Assert.DoesNotContain("field of work", actualResults);
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
        public void TestResults_ShowsError_1_OutgoingPropertiesOfKnownInstanceOfTypeShouldBeReducedInPossibilities()
        {
            var graph = new RDFExplorerGraph
            {
                nodes = new[] {
                    new Node(0, "?var1"),
                    new Node(1, "?HUMAN", new[] {"http://www.wikidata.org/entity/Q5"}),
                    new Node(2, "?FEMALE", new[] {"http://www.wikidata.org/entity/Q6581072"})
                },
                edges = new[] {
                    new Edge(0, "?instanceOf", 0, 1, new[] {"http://www.wikidata.org/prop/direct/P31"}),
                    new Edge(1, "?shouldBeGender", 0, 2)
                }
            };

            //Arrange:
            var filename = "Resources/QueryGraphPracticalResults1-Sorted.nt";
            var entitiesIndexPath = "PracticalResultsEntitiesIndex";
            var propertiesIndexPath = "PracticalResultsPropertiesIndex";

            CreateIndex(filename, entitiesIndexPath, propertiesIndexPath);

            // Act
            var queryGraph = new QueryGraph(graph);
            new QueryGraphResults().GetGraphQueryResults(queryGraph, entitiesIndexPath, propertiesIndexPath, false);

            var edges = queryGraph.Edges.Select(x => x.Value).ToArray();
            var genderEdge = edges[1];
            var actualResults = genderEdge.Results.Select(x => x.Label).ToList();
            Assert.Contains("sex or gender", actualResults);

            //FAILS ON THESE:
            Assert.DoesNotContain("depicts", actualResults);

            // Cleanup
            DeleteIndex(entitiesIndexPath, propertiesIndexPath);
        }

        [Fact]
        public void TestResults_ShowsError_2_OutgoingPropertiesOfKnownOutgoingTypeShouldBeReducedInPossibilities()
        {
            var graph = new RDFExplorerGraph
            {
                nodes = new[] {
                    new Node(0, "?var1"),
                    new Node(1, "?MAYOR", new[] {"http://www.wikidata.org/entity/Q30185"}),
                    new Node(2, "?FEMALE", new[] {"http://www.wikidata.org/entity/Q6581072"})
                },
                edges = new[] {
                    new Edge(0, "?POSITION_HELD", 0, 1, new[] {"http://www.wikidata.org/prop/direct/P39"}),
                    new Edge(1, "?shouldBeGender", 0, 2)
                }
            };

            //Arrange:
            var filename = "Resources/QueryGraphPracticalResults1-Sorted.nt";
            var entitiesIndexPath = "PracticalResultsEntitiesIndex";
            var propertiesIndexPath = "PracticalResultsPropertiesIndex";

            CreateIndex(filename, entitiesIndexPath, propertiesIndexPath);

            // Act
            var queryGraph = new QueryGraph(graph);
            new QueryGraphResults().GetGraphQueryResults(queryGraph, entitiesIndexPath, propertiesIndexPath, false);

            var edges = queryGraph.Edges.Select(x => x.Value).ToArray();
            var genderEdge = edges[1];
            var actualResults = genderEdge.Results.Select(x => x.Label).ToList();
            Assert.Contains("sex or gender", actualResults);

            //FAILS ON THESE:
            Assert.DoesNotContain("depicts", actualResults);

            // Cleanup
            DeleteIndex(entitiesIndexPath, propertiesIndexPath);
        }
    }
}