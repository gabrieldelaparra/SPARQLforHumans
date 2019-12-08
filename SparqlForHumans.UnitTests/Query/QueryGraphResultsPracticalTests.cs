using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        //TODO: Create Sample for testing. Do not run on full Index;
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

            //Arrange:
            var filename = "";
            var entitiesIndexPath = "";
            var propertiesIndexPath = "";
            CreateIndex(filename, entitiesIndexPath, propertiesIndexPath);

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

            // Cleanup
            DeleteIndex(entitiesIndexPath, propertiesIndexPath);
        }

        //TODO: Create Sample for testing. Do not run on full Index;
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
        

        /// <summary>
        /// ?mother P25 ?son
        /// ?mother ?prop ?var2
        /// </summary>
        [Fact]
        public void TestResults_IntersectDomain_3ConnectedNodes_IntersectDomains_3Nodes2Edge()
        {
            var graph = new RDFExplorerGraph
            {
                nodes = new[]
                {
                    new Node(0, "?intersect"),
                    new Node(1, "?COUNTRY", new[]{"http://www.wikidata.org/entity/Q6256"}),
                    new Node(2, "?CITY", new[]{"http://www.wikidata.org/entity/Q515"}),
                },
                edges = new[]
                {
                    new Edge(0, "?prop1", 0, 1),
                    new Edge(1, "?prop2", 0, 2),
                }
            };

            // Arrange
            var queryGraph = new QueryGraph(graph);
            
            //Act
            queryGraph.GetGraphQueryResults(LuceneDirectoryDefaults.EntityIndexPath, LuceneDirectoryDefaults.PropertyIndexPath, false);

            var nodes = queryGraph.Nodes.Select(x=>x.Value).ToArray();
            var edges = queryGraph.Edges.Select(x=>x.Value).ToArray();
            var genderEdge = edges[1];
            var actualResults = genderEdge.Results.Select(x => x.Label).ToList();

            //Assert
            //Assert.Equal(2, queryGraph.Nodes[0].InferredBaseTypes.Count);
            //Assert.Contains("Q5", queryGraph.Nodes[0].InferredBaseTypes);

            //Assert.Equal(5, queryGraph.Nodes[1].InferredBaseTypes.Count);
            //Assert.Contains("Q5", queryGraph.Nodes[1].InferredBaseTypes);

            //Assert.Empty(queryGraph.Nodes[2].InferredBaseTypes);

            //Assert.Equal(2, queryGraph.Edges[0].DomainBaseTypes.Count);
            //Assert.Contains("Q5", queryGraph.Edges[0].DomainBaseTypes);
            //Assert.Equal(5, queryGraph.Edges[0].RangeBaseTypes.Count);
            //Assert.Contains("Q5", queryGraph.Edges[0].RangeBaseTypes);

            ////Since E1 source is HUMAN, Domain HUMAN
            //Assert.Equal(2, queryGraph.Edges[1].DomainBaseTypes.Count);
            //Assert.Contains("Q5", queryGraph.Edges[1].DomainBaseTypes);
            //Assert.Empty(queryGraph.Edges[1].RangeBaseTypes);
        }
    }
}
