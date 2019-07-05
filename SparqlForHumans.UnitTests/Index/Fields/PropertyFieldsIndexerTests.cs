using Lucene.Net.Documents;
using SparqlForHumans.Lucene.Indexing.Fields;
using SparqlForHumans.Models.LuceneIndex;
using SparqlForHumans.RDF.Extensions;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace SparqlForHumans.UnitTests.Index.Fields
{
    public class PropertyFieldsIndexerTests
    {
        [Fact]
        public void TestGetField()
        {
            //Arrange
            var lines = new List<string>
            {
                "<http://www.wikidata.org/entity/Q26> <http://www.wikidata.org/prop/direct/P17> <http://www.wikidata.org/entity/Q145> .",
            };
            var subjectGroup = lines.GroupBySubject().FirstOrDefault();
            var expected = new StringField(Labels.Property.ToString(), "P17", Field.Store.YES);

            //Act
            var actual = new EntityPropertiesIndexer().GetField(subjectGroup);

            //Assert
            Assert.NotNull(actual);
            Assert.Equal(expected.FieldType, actual[0].FieldType);
            Assert.Equal(expected.Name, actual[0].Name);
            Assert.Equal(expected.GetStringValue(), actual[0].GetStringValue());
        }

        [Fact]
        public void TestGetFieldConcatenated()
        {
            //Arrange
            var lines = new List<string>
            {
                "<http://www.wikidata.org/entity/Q26> <http://www.wikidata.org/prop/direct/P17> <http://www.wikidata.org/entity/Q145> .",
                "<http://www.wikidata.org/entity/Q26> <http://www.wikidata.org/prop/direct/P18> <http://www.wikidata.org/entity/Q145> .",
            };
            var subjectGroup = lines.GroupBySubject().FirstOrDefault();
            var expected0 = new StringField(Labels.Property.ToString(), "P17", Field.Store.YES);
            var expected1 = new StringField(Labels.Property.ToString(), "P18", Field.Store.YES);

            //Act
            var actual = new EntityPropertiesIndexer().GetField(subjectGroup);

            //Assert
            Assert.NotNull(actual);
            Assert.Equal(expected0.FieldType, actual[0].FieldType);
            Assert.Equal(expected0.Name, actual[0].Name);
            Assert.Equal(expected0.GetStringValue(), actual[0].GetStringValue());

            Assert.Equal(expected1.FieldType, actual[1].FieldType);
            Assert.Equal(expected1.Name, actual[1].Name);
            Assert.Equal(expected1.GetStringValue(), actual[1].GetStringValue());
        }
    }
}
