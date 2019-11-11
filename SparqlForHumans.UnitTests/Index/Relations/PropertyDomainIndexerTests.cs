using Lucene.Net.Documents;
using SparqlForHumans.Lucene.Index.Relations;
using SparqlForHumans.Models.LuceneIndex;
using SparqlForHumans.RDF.Extensions;
using SparqlForHumans.Utilities;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace SparqlForHumans.UnitTests.Index.Relations
{
    /// <summary>
    ///     The CreateTypesAndPropertiesDictionary creates the following dictionary:
    ///     (Human) Q5 (Obama + OtherHuman): P31, P27, P555, P33, P44
    ///     (Country) Q17 (Chile): P31, P555, P777
    ///     The InvertDictionary dictionary should return the following:
    ///     P31: Q5, Q17
    ///     P27: Q5
    ///     P555: Q5, Q17
    ///     P33: Q5
    ///     P44: Q5
    ///     P777: Q17
    /// </summary>
    public class PropertyDomainIndexerTests
    {
        [Fact]
        public void TestGetField()
        {
            //Arrange
            const string filename = "Resources/TypeProperties.nt";
            var subjectGroups = FileHelper.GetInputLines(filename).GroupBySubject();
            var subjectGroup = subjectGroups.FirstOrDefault(x => x.Id.Equals("P27"));
            var expected = new StringField(Labels.DomainType.ToString(), "5", Field.Store.YES);

            //Act
            var index = new PropertyDomainIndexer(subjectGroups);
            var actual = index.GetField(subjectGroup);

            //Assert
            Assert.NotNull(actual);
            Assert.Equal(expected.FieldType, actual[0].FieldType);
            Assert.Equal(expected.Name, actual[0].Name);
            Assert.Equal(expected.GetStringValue(), actual[0].GetStringValue());

            // Assert
            Assert.Equal(6, index.RelationIndex.Count);

            Assert.Equal(31, index.RelationIndex.Keys.ElementAt(0));
            Assert.Equal(27, index.RelationIndex.Keys.ElementAt(1));
            Assert.Equal(555, index.RelationIndex.Keys.ElementAt(2));
            Assert.Equal(33, index.RelationIndex.Keys.ElementAt(3));
            Assert.Equal(44, index.RelationIndex.Keys.ElementAt(4));
            Assert.Equal(777, index.RelationIndex.Keys.ElementAt(5));

            //P31
            Assert.Equal(2, index.RelationIndex.ElementAt(0).Value.Length);

            //P27>Q30 (Q5)
            Assert.Single(index.RelationIndex.ElementAt(1).Value);
            Assert.Equal(5, index.RelationIndex.ElementAt(1).Value.ElementAt(0));

            //P555>Q555 (Q17)
            Assert.Equal(2, index.RelationIndex.ElementAt(2).Value.Length);
            Assert.Equal(5, index.RelationIndex.ElementAt(2).Value.ElementAt(0));
            Assert.Equal(17, index.RelationIndex.ElementAt(2).Value.ElementAt(1));

            //P777>Q777 (Q17)
            Assert.Single(index.RelationIndex.ElementAt(5).Value);
            Assert.Equal(17, index.RelationIndex.ElementAt(5).Value.ElementAt(0));
        }

        [Fact]
        public void TestGetPropertyDomainMultipleDomain()
        {
            //Arrange
            var lines = new List<string>
            {
                "<http://www.wikidata.org/entity/Q76> <http://www.wikidata.org/prop/direct/P31> <http://www.wikidata.org/entity/Q5> .",
                "<http://www.wikidata.org/entity/Q76> <http://www.wikidata.org/prop/direct/P27> <http://www.wikidata.org/entity/Q30> .",
                "<http://www.wikidata.org/entity/Q76> <http://www.wikidata.org/prop/direct/P555> <http://www.wikidata.org/entity/Q556> .",
                "<http://www.wikidata.org/entity/Q298> <http://www.wikidata.org/prop/direct/P31> <http://www.wikidata.org/entity/Q17> .",
                "<http://www.wikidata.org/entity/Q298> <http://www.wikidata.org/prop/direct/P555> <http://www.wikidata.org/entity/Q31> .",
                "<http://www.wikidata.org/entity/Q298> <http://www.wikidata.org/prop/direct/P777> <http://www.wikidata.org/entity/Q32> ."
            };
            var entityGroups = lines.GroupBySubject();

            var propertyDomainTypes = new PropertyDomainIndexer(entityGroups).RelationIndex.ToArray();

            // P31, P27, P555, P777
            Assert.Equal(4, propertyDomainTypes.Length);

            // P27 -> 5
            Assert.Equal(27, propertyDomainTypes[1].Key);
            Assert.Single(propertyDomainTypes[1].Value);
            Assert.Equal(5, propertyDomainTypes[1].Value[0]);

            // P555 -> 5, 17
            Assert.Equal(555, propertyDomainTypes[2].Key);
            Assert.Equal(2, propertyDomainTypes[2].Value.Length);
            Assert.Equal(5, propertyDomainTypes[2].Value[0]);
            Assert.Equal(17, propertyDomainTypes[2].Value[1]);

            // P777 -> 17
            Assert.Equal(777, propertyDomainTypes[3].Key);
            Assert.Single(propertyDomainTypes[3].Value);
            Assert.Equal(17, propertyDomainTypes[3].Value[0]);
        }

        [Fact]
        public void TestGetPropertyDomainMultipleTypesAndDomain()
        {
            //Arrange
            var lines = new List<string>
            {
                "<http://www.wikidata.org/entity/Q76> <http://www.wikidata.org/prop/direct/P31> <http://www.wikidata.org/entity/Q5> .",
                "<http://www.wikidata.org/entity/Q76> <http://www.wikidata.org/prop/direct/P27> <http://www.wikidata.org/entity/Q30> .",
                "<http://www.wikidata.org/entity/Q76> <http://www.wikidata.org/prop/direct/P555> <http://www.wikidata.org/entity/Q556> .",
                "<http://www.wikidata.org/entity/Q298> <http://www.wikidata.org/prop/direct/P31> <http://www.wikidata.org/entity/Q17> .",
                "<http://www.wikidata.org/entity/Q298> <http://www.wikidata.org/prop/direct/P555> <http://www.wikidata.org/entity/Q31> .",
                "<http://www.wikidata.org/entity/Q298> <http://www.wikidata.org/prop/direct/P777> <http://www.wikidata.org/entity/Q32> .",
                "<http://www.wikidata.org/entity/Q77> <http://www.wikidata.org/prop/direct/P31> <http://www.wikidata.org/entity/Q5> .",
                "<http://www.wikidata.org/entity/Q77> <http://www.wikidata.org/prop/direct/P31> <http://www.wikidata.org/entity/Q6> .",
                "<http://www.wikidata.org/entity/Q77> <http://www.wikidata.org/prop/direct/P27> <http://www.wikidata.org/entity/Q30> .",
                "<http://www.wikidata.org/entity/Q77> <http://www.wikidata.org/prop/direct/P555> <http://www.wikidata.org/entity/Q556> ."
            };
            var entityGroups = lines.GroupBySubject();

            var propertyDomainTypes = new PropertyDomainIndexer(entityGroups).RelationIndex.ToArray();

            // P31, P27, P555, P777
            Assert.Equal(4, propertyDomainTypes.Length);

            // P27 -> 5, 6
            Assert.Equal(27, propertyDomainTypes[1].Key);
            Assert.Equal(2, propertyDomainTypes[1].Value.Length);
            Assert.Equal(5, propertyDomainTypes[1].Value[0]);
            Assert.Equal(6, propertyDomainTypes[1].Value[1]);

            // P555 -> 5, 17, 6
            Assert.Equal(555, propertyDomainTypes[2].Key);
            Assert.Equal(3, propertyDomainTypes[2].Value.Length);
            Assert.Equal(5, propertyDomainTypes[2].Value[0]);
            Assert.Equal(17, propertyDomainTypes[2].Value[1]);
            Assert.Equal(6, propertyDomainTypes[2].Value[2]);

            // P777 -> 17
            Assert.Equal(777, propertyDomainTypes[3].Key);
            Assert.Single(propertyDomainTypes[3].Value);
            Assert.Equal(17, propertyDomainTypes[3].Value[0]);
        }

        [Fact]
        public void TestGetPropertyDomainSingleDomain()
        {
            //Arrange
            var lines = new List<string>
            {
                "<http://www.wikidata.org/entity/Q76> <http://www.wikidata.org/prop/direct/P31> <http://www.wikidata.org/entity/Q5> .",
                "<http://www.wikidata.org/entity/Q76> <http://www.wikidata.org/prop/direct/P27> <http://www.wikidata.org/entity/Q30> .",
                "<http://www.wikidata.org/entity/Q76> <http://www.wikidata.org/prop/direct/P555> <http://www.wikidata.org/entity/Q556> ."
            };
            var entityGroups = lines.GroupBySubject();

            var propertyDomainTypes = new PropertyDomainIndexer(entityGroups).RelationIndex.ToArray();

            // P31, P27, P555
            Assert.Equal(3, propertyDomainTypes.Length);

            // P27 -> P5
            Assert.Equal(27, propertyDomainTypes[1].Key);
            Assert.Single(propertyDomainTypes[1].Value);
            Assert.Equal(5, propertyDomainTypes[1].Value[0]);

            // P555 -> P5
            Assert.Equal(555, propertyDomainTypes[2].Key);
            Assert.Single(propertyDomainTypes[2].Value);
            Assert.Equal(5, propertyDomainTypes[2].Value[0]);
        }
    }
}