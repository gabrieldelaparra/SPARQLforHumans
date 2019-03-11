using System.Collections.Generic;
using System.Linq;
using SparqlForHumans.Lucene.Indexing;
using SparqlForHumans.Models.LuceneIndex;
using SparqlForHumans.Utilities;
using Xunit;

namespace SparqlForHumans.UnitTests
{
    public class EntityTypesTests
    {
        [Fact]
        public static void TestGetEntityType()
        {
            //Arrange
            var lines = new List<string>
            {
                @"<http://www.wikidata.org/entity/Q76> <http://www.wikidata.org/prop/direct/P31> <http://www.wikidata.org/entity/Q5> .",
                @"<http://www.wikidata.org/entity/Q76> <http://www.wikidata.org/prop/direct/P31> <http://www.wikidata.org/entity/Q30461> .",
                @"<http://www.wikidata.org/entity/Q77> <http://www.wikidata.org/prop/direct/P31> <http://www.wikidata.org/entity/Q5> .",
            };
            var entityGroups = lines.GroupBySubject();
            
            // Act
            var entityTypeTuples = entityGroups.Select(x => x.GetEntityTypes()).ToArray();

            // Assert
            // Expected (2): 76, 77
            Assert.Equal(2, entityTypeTuples.Length);

            //Q76 -> Q5, Q30461 (Obama -> Human, President)
            Assert.Equal(76, entityTypeTuples[0].EntityId);
            Assert.Equal(2, entityTypeTuples[0].TypeIds.Length);
            Assert.Equal(5, entityTypeTuples[0].TypeIds[0]);
            Assert.Equal(30461, entityTypeTuples[0].TypeIds[1]);

            //Q77 -> Q5 (Other -> Human)
            Assert.Equal(77, entityTypeTuples[1].EntityId);
            Assert.Single(entityTypeTuples[1].TypeIds);
            Assert.Equal(5, entityTypeTuples[1].TypeIds[0]);
        }

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
            // Expected (7): 76, 77, 5, 17, 298, 414, 30461
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

        [Fact]
        // Puede que falle si el orden es distinto en el ToDictionary underlaying.
        public static void TestGetEntityTypesDictionary()
        {
            // Arrange
            const string filename = "Resources/EntityTypes.nt";
            var lines = FileHelper.GetInputLines(filename);
            var entityGroups = lines.GroupBySubject();

            // Act
            var entityTypeTuples = entityGroups.GetEntityTypes().ToArray();

            // Assert
            // Expected (4): 76, 77, 298, 414,
            // Excluded: 5, 17, 30461
            Assert.Equal(4, entityTypeTuples.Length);

            //Q76 -> Q5, Q30461 (Obama -> Human, President)
            Assert.Equal(76, entityTypeTuples[0].Key);
            Assert.Equal(2, entityTypeTuples[0].Value.Length);
            Assert.Equal(5, entityTypeTuples[0].Value[0]);
            Assert.Equal(30461, entityTypeTuples[0].Value[1]);

            //Q77 -> Q5 (Other -> Human)
            Assert.Equal(77, entityTypeTuples[1].Key);
            Assert.Single(entityTypeTuples[1].Value);
            Assert.Equal(5, entityTypeTuples[1].Value[0]);

            //Q298 -> Q17 (Chile -> Country)
            Assert.Equal(298, entityTypeTuples[2].Key);
            Assert.Single(entityTypeTuples[2].Value);
        }
    }
}
