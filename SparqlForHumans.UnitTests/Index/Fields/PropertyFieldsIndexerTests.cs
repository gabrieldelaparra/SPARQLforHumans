using System.Collections.Generic;
using System.Linq;
using Lucene.Net.Documents;
using SparqlForHumans.Lucene.Indexing.Fields;
using SparqlForHumans.Models.LuceneIndex;
using SparqlForHumans.RDF.Extensions;
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
            Assert.Equal(expected.FieldType, actual.FieldType);
            Assert.Equal(expected.Name, actual.Name);
            Assert.Equal(expected.GetStringValue(), actual.GetStringValue());
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
            var expected = new StringField(Labels.Property.ToString(), "P17##P18", Field.Store.YES);

            //Act
            var actual = new EntityPropertiesIndexer().GetField(subjectGroup);

            //Assert
            Assert.NotNull(actual);
            Assert.Equal(expected.FieldType, actual.FieldType);
            Assert.Equal(expected.Name, actual.Name);
            Assert.Equal(expected.GetStringValue(), actual.GetStringValue());
        }
    }
}
