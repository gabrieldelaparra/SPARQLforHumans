using Lucene.Net.Documents;
using SparqlForHumans.Lucene.Index.Fields;
using SparqlForHumans.Models.LuceneIndex;
using SparqlForHumans.RDF.Extensions;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace SparqlForHumans.UnitTests.Index.Fields
{
    [Collection("Sequential")]
    public class IdIndexerTests
    {
        [Fact]
        public void TestGetField()
        {
            //Arrange
            var lines = new List<string>
            {
                "<http://www.wikidata.org/entity/Q27> <http://schema.org/description> \"Ireland Description\"@en ."
            };
            var subjectGroup = lines.GroupBySubject().FirstOrDefault();
            var expected = new StringField(Labels.Id.ToString(), "Q27", Field.Store.YES);

            //Act
            var actual = new IdIndexer().GetField(subjectGroup).ToArray();

            //Assert
            Assert.NotNull(actual);
            Assert.Equal(expected.FieldType, actual[0].FieldType);
            Assert.Equal(expected.Name, actual[0].Name);
            Assert.Equal(expected.GetStringValue(), actual[0].GetStringValue());
        }
    }
}