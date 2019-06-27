using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lucene.Net.Documents;
using SparqlForHumans.Lucene.Indexing.Relations;
using SparqlForHumans.Models.LuceneIndex;
using SparqlForHumans.RDF.Extensions;
using SparqlForHumans.Utilities;
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
            Assert.Equal(5, index.RelationIndex.Count);

            Assert.Equal(27, index.RelationIndex.Keys.ElementAt(0));
            Assert.Equal(555, index.RelationIndex.Keys.ElementAt(1));
            Assert.Equal(33, index.RelationIndex.Keys.ElementAt(2));
            Assert.Equal(44, index.RelationIndex.Keys.ElementAt(3));
            Assert.Equal(777, index.RelationIndex.Keys.ElementAt(4));

            //P27>Q30 (Q5)
            Assert.Single(index.RelationIndex.ElementAt(0).Value);
            Assert.Equal(5, index.RelationIndex.ElementAt(0).Value.ElementAt(0));

            //P555>Q555 (Q17)
            Assert.Equal(2, index.RelationIndex.ElementAt(1).Value.Length);
            Assert.Equal(5, index.RelationIndex.ElementAt(1).Value.ElementAt(0));
            Assert.Equal(17, index.RelationIndex.ElementAt(1).Value.ElementAt(1));

            //P777>Q777 (Q17)
            Assert.Single(index.RelationIndex.ElementAt(4).Value);
            Assert.Equal(17, index.RelationIndex.ElementAt(4).Value.ElementAt(0));
        }
    }
}
