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
        public static void TestGetEntityType()
        {
            Assert.False(true);
        }

        [Fact]
        public static void TestGetEntityTypesPairs()
        {
            // Arrange
            const string filename = "Resources/EntityTypes.nt";
            var lines = FileHelper.GetInputLines(filename);
            var entityGroups = lines.GroupBySubject();

            // Act
            var entityTypeTuples = entityGroups.Select(x => x.GetEntityTypes()).ToDictionary().ToArray();

            // Assert
            // 76, 77, 5, 17, 298, 414, 30461
            Assert.Equal(7, entityTypeTuples.Length);

            //Q76 -> Q5, Q30461 (Obama -> Human, President)
            Assert.Equal(76, entityTypeTuples[0].Key);
            Assert.Equal(2, entityTypeTuples[0].Value.Length);
            Assert.Equal(5, entityTypeTuples[0].Value[0]);
            Assert.Equal(30461, entityTypeTuples[0].Value[1]);

            //Q77 -> Q5 (Other -> Human)
            Assert.Equal(77, entityTypeTuples[1].Key);
            Assert.Single(entityTypeTuples[1].Value);
            Assert.Equal(5, entityTypeTuples[1].Value[0]);

            //Q5 -> NONE (Human -> NONE)
            Assert.Equal(5, entityTypeTuples[2].Key);
            Assert.Empty(entityTypeTuples[2].Value);
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

        [Fact]
        // Puede que falle si el orden es distinto en el ToDictionary underlaying.
        public static void TestCompareToDictionary()
        {
            // Arrange
            const string filename = "Resources/EntityTypes.nt";
            var lines = FileHelper.GetInputLines(filename);
            var entityGroups = lines.GroupBySubject();

            // Act
            var tuples = entityGroups.Select(x => x.GetEntityTypes()).ToList();
            var expected = tuples.ToArray();
            var actual = tuples.ToDictionary().ToArray();

            // Assert
            Assert.Equal(expected.Length, actual.Length);
            for (var i = 0; i < expected.Length; i++)
            {
                Assert.Equal(expected[i].EntityId, actual[i].Key);
                Assert.Equal(expected[i].TypeIds.Length, actual[i].Value.Length);
                for (var j = 0; j < expected[i].TypeIds.Length; j++)
                {
                    Assert.Equal(expected[i].TypeIds[j], actual[i].Value[j]);
                }
            }
        }
    }
}
