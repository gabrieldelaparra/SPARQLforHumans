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
    public class AltLabelIndexerTests
    {
        [Fact]
        public void TestGetField()
        {
            //Arrange
            var lines = new List<string>
            {
                "<http://www.wikidata.org/entity/Q27> <http://www.w3.org/2004/02/skos/core#altLabel> \"NIR\"@en ."
            };
            var subjectGroup = lines.GroupBySubject().FirstOrDefault();
            var expected = new TextField(Labels.AltLabel.ToString(), "NIR", Field.Store.YES);

            //Act
            var actual = new AltLabelIndexer().GetField(subjectGroup).ToArray();

            //Assert
            Assert.NotNull(actual);
            Assert.Equal(expected.FieldType, actual[0].FieldType);
            Assert.Equal(expected.Name, actual[0].Name);
            Assert.Equal(expected.GetStringValue(), actual[0].GetStringValue());
        }
    }
}