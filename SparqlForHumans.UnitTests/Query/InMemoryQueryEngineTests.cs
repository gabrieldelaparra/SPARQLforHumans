using System.Linq;
using SparqlForHumans.Lucene.Index;
using SparqlForHumans.Lucene.Queries.Graph;
using SparqlForHumans.Utilities;
using Xunit;

namespace SparqlForHumans.UnitTests.Query
{
    public class InMemoryQueryEngineTests
    {
        [Fact]
        public void TestOutgoingProperties()
        {
            // Arrange
            const string filename = @"Resources/QueryGraphInMemoryEngine.nt";
            const string entitiesIndexPath = "QueryGraphInMemoryEngineEntities";
            const string propertiesIndexPath = "QueryGraphInMemoryEngineProperties";
            entitiesIndexPath.DeleteIfExists();
            propertiesIndexPath.DeleteIfExists();
            new EntitiesIndexer(filename, entitiesIndexPath).Index();
            new SimplePropertiesIndexer(filename, propertiesIndexPath).Index();

            // Act
            InMemoryQueryEngine.Init(entitiesIndexPath, propertiesIndexPath);
            var actual = InMemoryQueryEngine.BatchEntityIdOutgoingPropertiesQuery(new[] {"http://www.wikidata.org/entity/Q5"}).ToArray();

            Assert.Contains("P25", actual);
            Assert.Contains("P27", actual);
            Assert.Contains("P777", actual);

            // Cleanup
            entitiesIndexPath.DeleteIfExists();
            propertiesIndexPath.DeleteIfExists();
        }

        [Fact]
        public void TestIncomingProperties()
        {
            // Arrange
            const string filename = @"Resources/QueryGraphInMemoryEngine.nt";
            const string entitiesIndexPath = "QueryGraphInMemoryEngineEntities";
            const string propertiesIndexPath = "QueryGraphInMemoryEngineProperties";
            entitiesIndexPath.DeleteIfExists();
            propertiesIndexPath.DeleteIfExists();
            new EntitiesIndexer(filename, entitiesIndexPath).Index();
            new SimplePropertiesIndexer(filename, propertiesIndexPath).Index();

            // Act
            InMemoryQueryEngine.Init(entitiesIndexPath, propertiesIndexPath);
            var actual = InMemoryQueryEngine.BatchEntityIdIncomingPropertiesQuery(new[] {"http://www.wikidata.org/entity/Q5"});

            Assert.Contains("P25", actual);

            // Cleanup
            entitiesIndexPath.DeleteIfExists();
            propertiesIndexPath.DeleteIfExists();
        }

        [Fact]
        public void TestDomainEntities()
        {
            // Arrange
            const string filename = @"Resources/QueryGraphInMemoryEngine.nt";
            const string entitiesIndexPath = "QueryGraphInMemoryEngineEntities";
            const string propertiesIndexPath = "QueryGraphInMemoryEngineProperties";
            entitiesIndexPath.DeleteIfExists();
            propertiesIndexPath.DeleteIfExists();
            new EntitiesIndexer(filename, entitiesIndexPath).Index();
            new SimplePropertiesIndexer(filename, propertiesIndexPath).Index();

            // Act
            InMemoryQueryEngine.Init(entitiesIndexPath, propertiesIndexPath);
            var actual = InMemoryQueryEngine.BatchPropertyIdDomainTypesQuery(new[] {"http://www.wikidata.org/prop/direct/P25"}).ToArray();

            Assert.Contains("http://www.wikidata.org/entity/Q5", actual);
            Assert.Contains("http://www.wikidata.org/entity/Q49088", actual);

            // Cleanup
            entitiesIndexPath.DeleteIfExists();
            propertiesIndexPath.DeleteIfExists();
        }

        [Fact]
        public void TestRangeEntities()
        {
            // Arrange
            const string filename = @"Resources/QueryGraphInMemoryEngine.nt";
            const string entitiesIndexPath = "QueryGraphInMemoryEngineEntities";
            const string propertiesIndexPath = "QueryGraphInMemoryEngineProperties";
            entitiesIndexPath.DeleteIfExists();
            propertiesIndexPath.DeleteIfExists();
            new EntitiesIndexer(filename, entitiesIndexPath).Index();
            new SimplePropertiesIndexer(filename, propertiesIndexPath).Index();

            // Act
            InMemoryQueryEngine.Init(entitiesIndexPath, propertiesIndexPath);
            var actual = InMemoryQueryEngine.BatchPropertyIdRangeTypesQuery(new[] {"http://www.wikidata.org/prop/direct/P27"});

            Assert.Contains("http://www.wikidata.org/entity/Q6256", actual);

            // Cleanup
            entitiesIndexPath.DeleteIfExists();
            propertiesIndexPath.DeleteIfExists();
        }
    }
}
