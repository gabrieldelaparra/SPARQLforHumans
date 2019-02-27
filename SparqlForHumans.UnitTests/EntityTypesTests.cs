using System.Linq;
using SparqlForHumans.Lucene.Extensions;
using SparqlForHumans.Lucene.Indexing;
using SparqlForHumans.Utilities;
using Xunit;

namespace SparqlForHumans.UnitTests
{
    public class EntityTypesTests
    {
        [Fact]
        public static void TestGetEntityTypesPairs()
        {
            // Arrange
            const string filename = "Resources/EntityTypes.nt";
            var lines = FileHelper.GetInputLines(filename);
            var entityGroups = lines.GroupBySubject();

            // Act
            var entityTypeTuples = entityGroups.Select(x => x.GetEntityTypes()).ToArray();

            // Assert
            // 76, 77, 5, 17, 298, 414, 30461
            Assert.Equal(7, entityTypeTuples.Length);

            //Q76 -> Q5, Q30461 (Obama -> Human, President)
            Assert.Equal(76, entityTypeTuples[0].EntityId);
            Assert.Equal(2, entityTypeTuples[0].TypeIds.Length);
            Assert.Equal(5, entityTypeTuples[0].TypeIds[0]);
            Assert.Equal(30461, entityTypeTuples[0].TypeIds[1]);

            //Q77 -> Q5 (Other -> Human)
            Assert.Equal(77, entityTypeTuples[1].EntityId);
            Assert.Single(entityTypeTuples[1].TypeIds);
            Assert.Equal(5, entityTypeTuples[1].TypeIds[0]);

            //Q5 -> NONE (Human -> NONE)
            Assert.Equal(5, entityTypeTuples[2].EntityId);
            Assert.Empty(entityTypeTuples[2].TypeIds);
        }

        [Fact]
        public static void TestTupleToString()
        {
            // Arrange
            const string filename = "Resources/EntityTypes.nt";
            var lines = FileHelper.GetInputLines(filename);
            var entityGroups = lines.GroupBySubject();

            // Act
            var entityTypeTuples = entityGroups.Select(x => x.GetEntityTypes()).ToArray();

            // Assert
            // Q76->Q5, Q30461
            const string expected76 = "76 5 30461";
            Assert.Equal(expected76, entityTypeTuples[0].ToEntityTypesString());

            //Q5 -> NONE (Human -> NONE)
            const string expected5 = "5";
            Assert.Equal(expected5, entityTypeTuples[2].ToEntityTypesString());
        }
    }
}
