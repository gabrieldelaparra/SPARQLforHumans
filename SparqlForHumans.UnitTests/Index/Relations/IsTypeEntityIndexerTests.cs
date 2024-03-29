﻿using System.Collections.Generic;
using System.Linq;
using Lucene.Net.Documents;
using SparqlForHumans.Lucene.Index.Fields;
using SparqlForHumans.Lucene.Index.Relations;
using SparqlForHumans.Models.LuceneIndex;
using SparqlForHumans.RDF.Extensions;
using SparqlForHumans.Utilities;
using Xunit;

namespace SparqlForHumans.UnitTests.Index.Relations
{
    public class IsTypeEntityIndexerTests
    {
        [Fact]
        public static void TestGetEntityType()
        {
            //Arrange
            var lines = new List<string> {
                @"<http://www.wikidata.org/entity/Q76> <http://www.wikidata.org/prop/direct/P31> <http://www.wikidata.org/entity/Q5> .",
                @"<http://www.wikidata.org/entity/Q76> <http://www.wikidata.org/prop/direct/P31> <http://www.wikidata.org/entity/Q30461> .",
                @"<http://www.wikidata.org/entity/Q77> <http://www.wikidata.org/prop/direct/P31> <http://www.wikidata.org/entity/Q5> ."
            };
            var entityGroups = lines.GroupBySubject();

            // Act

            var entityTypesDictionary = entityGroups.ToDictionary(
                key => key.IntId,
                values => new InstanceOfIndexer().TriplesToValue(values).Select(id => id.ToInt()).ToArray()).ToArray();

            // Assert
            // Expected (2): 76, 77
            Assert.Equal(2, entityTypesDictionary.Length);

            //Q76 -> Q5, Q30461 (Obama -> Human, President)
            Assert.Equal(76, entityTypesDictionary[0].Key);
            Assert.Equal(2, entityTypesDictionary[0].Value.Length);
            Assert.Equal(5, entityTypesDictionary[0].Value[0]);
            Assert.Equal(30461, entityTypesDictionary[0].Value[1]);

            //Q77 -> Q5 (Other -> Human)
            Assert.Equal(77, entityTypesDictionary[1].Key);
            Assert.Single(entityTypesDictionary[1].Value);
            Assert.Equal(5, entityTypesDictionary[1].Value[0]);
        }

        [Fact]
        public static void TestGetEntityTypesDictionary()
        {
            // Arrange
            const string filename = "Resources/EntityTypes.nt";
            var lines = FileHelper.GetInputLines(filename);
            var entityGroups = lines.GroupBySubject();

            // Act
            var entityTypesDictionary = entityGroups.ToDictionary(
                    key => key.IntId,
                    values => new InstanceOfIndexer().TriplesToValue(values).Select(id => id.ToInt()).ToArray())
                .ToArray();

            // Assert
            // Expected (4): 76, 77, 298, 414,
            // Excluded: 5, 17, 30461
            Assert.Equal(7, entityTypesDictionary.Length);

            //Let's just check those that have values:
            entityTypesDictionary = entityTypesDictionary.Where(x => x.Value.Any()).ToArray();

            //Q76 -> Q5, Q30461 (Obama -> Human, President)
            Assert.Equal(76, entityTypesDictionary[0].Key);
            Assert.Equal(2, entityTypesDictionary[0].Value.Length);
            Assert.Equal(5, entityTypesDictionary[0].Value[0]);
            Assert.Equal(30461, entityTypesDictionary[0].Value[1]);

            //Q77 -> Q5 (Other -> Human)
            Assert.Equal(77, entityTypesDictionary[1].Key);
            Assert.Single(entityTypesDictionary[1].Value);
            Assert.Equal(5, entityTypesDictionary[1].Value[0]);

            //Q298 -> Q17 (Chile -> Country)
            Assert.Equal(298, entityTypesDictionary[2].Key);
            Assert.Single(entityTypesDictionary[2].Value);

            //Q5 -> NONE (Human -> NONE)
            Assert.Empty(entityTypesDictionary.Where(x => x.Key.Equals(5)));
        }

        [Fact]
        public void TestGetField()
        {
            //Arrange
            var lines = new List<string> {
                "<http://www.wikidata.org/entity/Q145> <http://www.wikidata.org/prop/direct/P17> <http://www.wikidata.org/entity/Q126> .",
                "<http://www.wikidata.org/entity/Q26> <http://www.wikidata.org/prop/direct/P31> <http://www.wikidata.org/entity/Q145> ."
            };
            var subjectGroups = lines.GroupBySubject();
            var subjectGroup = subjectGroups.FirstOrDefault();
            var expected = new StringField(Labels.IsTypeEntity.ToString(), true.ToString(), Field.Store.YES);

            //Act
            var actual = new EntityIsTypeIndexer(subjectGroups).GetField(subjectGroup).ToArray();

            //Assert
            Assert.NotNull(actual);
            Assert.Equal(expected.FieldType, actual[0].FieldType);
            Assert.Equal(expected.Name, actual[0].Name);
            Assert.Equal(expected.GetStringValue(), actual[0].GetStringValue());
        }

        //[Fact]
        //public static void TestGetTypeEntitiesDictionary()
        //{
        //    // Arrange
        //    const string filename = "Resources/EntityTypes.nt";
        //    var lines = FileHelper.GetInputLines(filename);
        //    var entityGroups = lines.GroupBySubject();

        //    // Act
        //    var typeEntitiesDictionary = new TypeToEntitiesRelationMapper(entityGroups).RelationIndex.ToArray();

        //    // Assert
        //    // Expected (3): 5, 17, 30461
        //    // Excluded: 76, 77, 298, 414
        //    Assert.Equal(3, typeEntitiesDictionary.Length);

        //    //Q5 -> Q76, Q77
        //    Assert.Equal(5, typeEntitiesDictionary[0].Key);
        //    Assert.Equal(2, typeEntitiesDictionary[0].Value.Length);
        //    Assert.Equal(76, typeEntitiesDictionary[0].Value[0]);
        //    Assert.Equal(77, typeEntitiesDictionary[0].Value[1]);

        //    //Q30461 -> Q76
        //    Assert.Equal(30461, typeEntitiesDictionary[1].Key);
        //    Assert.Single(typeEntitiesDictionary[1].Value);
        //    Assert.Equal(76, typeEntitiesDictionary[1].Value[0]);

        //    //Q17 -> Q298, Q414
        //    Assert.Equal(17, typeEntitiesDictionary[2].Key);
        //    Assert.Equal(2, typeEntitiesDictionary[2].Value.Length);
        //    Assert.Equal(298, typeEntitiesDictionary[2].Value[0]);
        //    Assert.Equal(414, typeEntitiesDictionary[2].Value[1]);

        //    //Q76 -> NONE (Obama -> NONE)
        //    Assert.Empty(typeEntitiesDictionary.Where(x => x.Key.Equals(76)));
        //}

        //[Fact]
        //public static void TestGetTypeEntity()
        //{
        //    //Arrange
        //    var lines = new List<string>
        //    {
        //        @"<http://www.wikidata.org/entity/Q76> <http://www.wikidata.org/prop/direct/P31> <http://www.wikidata.org/entity/Q5> .",
        //        @"<http://www.wikidata.org/entity/Q76> <http://www.wikidata.org/prop/direct/P31> <http://www.wikidata.org/entity/Q30461> .",
        //        @"<http://www.wikidata.org/entity/Q77> <http://www.wikidata.org/prop/direct/P31> <http://www.wikidata.org/entity/Q5> ."
        //    };
        //    var entityGroups = lines.GroupBySubject();

        //    // Act
        //    var typeEntitiesDictionary = new TypeToEntitiesRelationMapper(entityGroups).RelationIndex.ToArray();

        //    // Assert
        //    // Expected (2): 5
        //    Assert.Equal(2, typeEntitiesDictionary.Length);

        //    //Q5 -> Q76, Q77
        //    Assert.Equal(5, typeEntitiesDictionary[0].Key);
        //    Assert.Equal(2, typeEntitiesDictionary[0].Value.Length);
        //    Assert.Equal(76, typeEntitiesDictionary[0].Value[0]);
        //    Assert.Equal(77, typeEntitiesDictionary[0].Value[1]);

        //    //Q30461 -> Q76
        //    Assert.Equal(30461, typeEntitiesDictionary[1].Key);
        //    Assert.Single(typeEntitiesDictionary[1].Value);
        //    Assert.Equal(76, typeEntitiesDictionary[1].Value[0]);
        //}
    }
}