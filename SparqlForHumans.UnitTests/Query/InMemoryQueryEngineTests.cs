using SparqlForHumans.Lucene.Index;
using SparqlForHumans.Lucene.Queries.Graph;
using SparqlForHumans.Utilities;
using Xunit;

namespace SparqlForHumans.UnitTests.Query
{
    public class InMemoryQueryEngineTests
    {
        [Fact]
        public void TestQueryDomain()
        {
            // Arrange
            const string filename = @"Resources/QueryGraph.nt";
            const string entitiesIndexPath = "QueryGraphEntities";
            const string propertiesIndexPath = "QueryGraphProperties";
            entitiesIndexPath.DeleteIfExists();
            propertiesIndexPath.DeleteIfExists();
            new EntitiesIndexer(filename, entitiesIndexPath).Index();
            new PropertiesIndexer(filename, propertiesIndexPath).Index();

            // Act
            InMemoryQueryEngine.Init(entitiesIndexPath, propertiesIndexPath);
            var actual = InMemoryQueryEngine.BatchIdPropertyDomainQuery(new[] {"http://www.wikidata.org/entity/Q5"});

            Assert.Contains("http://www.wikidata.org/prop/direct/P25", actual);
            Assert.Contains("http://www.wikidata.org/prop/direct/P27", actual);
            Assert.Contains("http://www.wikidata.org/prop/direct/P777", actual);

            // Cleanup
            entitiesIndexPath.DeleteIfExists();
            propertiesIndexPath.DeleteIfExists();
        }

        [Fact]
        public void TestQueryRange()
        {
            // Arrange
            const string filename = @"Resources/QueryGraph.nt";
            const string entitiesIndexPath = "QueryGraphEntities";
            const string propertiesIndexPath = "QueryGraphProperties";
            entitiesIndexPath.DeleteIfExists();
            propertiesIndexPath.DeleteIfExists();
            new EntitiesIndexer(filename, entitiesIndexPath).Index();
            new PropertiesIndexer(filename, propertiesIndexPath).Index();

            // Act
            InMemoryQueryEngine.Init(entitiesIndexPath, propertiesIndexPath);
            var actual = InMemoryQueryEngine.BatchIdPropertyRangeQuery(new[] {"http://www.wikidata.org/entity/Q5"});

            Assert.Contains("http://www.wikidata.org/prop/direct/P25", actual);

            // Cleanup
            entitiesIndexPath.DeleteIfExists();
            propertiesIndexPath.DeleteIfExists();
        }
    }
}
