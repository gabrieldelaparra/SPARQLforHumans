using System.Collections.Generic;
using System.Linq;
using Lucene.Net.Documents;
using SparqlForHumans.Lucene.Indexing.Relations;
using SparqlForHumans.Models.LuceneIndex;
using SparqlForHumans.RDF.Extensions;
using Xunit;

namespace SparqlForHumans.UnitTests.Index.Relations
{
    public class IsTypeEntityIndexerTests
    {
        [Fact]
        public void TestGetField()
        {
            //Arrange
            var lines = new List<string>
            {
                "<http://www.wikidata.org/entity/Q145> <http://www.wikidata.org/prop/direct/P17> <http://www.wikidata.org/entity/Q126> .",
                "<http://www.wikidata.org/entity/Q26> <http://www.wikidata.org/prop/direct/P31> <http://www.wikidata.org/entity/Q145> ."
            };
            var subjectGroups = lines.GroupBySubject();
            var subjectGroup = subjectGroups.FirstOrDefault();
            var expected = new StringField(Labels.IsTypeEntity.ToString(), true.ToString(), Field.Store.YES);

            //Act
            var actual = new IsTypeIndexer(subjectGroups).GetField(subjectGroup);

            //Assert
            Assert.NotNull(actual);
            Assert.Equal(expected.FieldType, actual.FieldType);
            Assert.Equal(expected.Name, actual.Name);
            Assert.Equal(expected.GetStringValue(), actual.GetStringValue());
        }
    }

}
