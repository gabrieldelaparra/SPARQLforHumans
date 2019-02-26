using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            var entityTypeTuple = entityGroups.Select(x => x.GetEntityTypes()).ToList();

            // Assert
            // 76, 77, 5, 17, 298, 414, 30461
            Assert.Equal(7, entityTypeTuple.Count);

            //Q76 -> Q5, Q30461 (Obama -> Human, President)
            Assert.Equal(76, entityTypeTuple[0].EntityId);
            Assert.Equal(2, entityTypeTuple[0].TypeIds.Length);
            Assert.Equal(5, entityTypeTuple[0].TypeIds[0]);
            Assert.Equal(30461, entityTypeTuple[0].TypeIds[1]);

            //Q77 -> Q5 (Other -> Human)
            Assert.Equal(77, entityTypeTuple[1].EntityId);
            Assert.Single(entityTypeTuple[1].TypeIds);
            Assert.Equal(5, entityTypeTuple[1].TypeIds[0]);

            //Q5 -> NONE (Human -> NONE)
            Assert.Equal(5, entityTypeTuple[2].EntityId);
            Assert.Empty(entityTypeTuple[2].TypeIds);
        }
    }
}
