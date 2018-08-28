using Lucene.Net.Index;
using SparqlForHumans.Core.Properties;
using SparqlForHumans.Core.Services;
using SparqlForHumans.Core.Utilities;
using System.IO;
using System.Linq;
using Xunit;

namespace SparqlForHumans.UnitTests
{
    public class PropertyIndexTests {
        [Fact]
        public void TestCreateBasicIndex () {

            const string filename = @"Resources/PropertyIndex.nt";
            Assert.True (File.Exists (filename));

            const string outputPath = "PropertyIndex";
            if (Directory.Exists (outputPath))
                Directory.Delete (outputPath, true);

            Assert.False(Directory.Exists(outputPath));
            
            IndexBuilder.CreatePropertyIndex (filename, outputPath, true);

            Assert.True(Directory.Exists(outputPath));

            using (var reader = IndexReader.Open(outputPath.GetLuceneDirectory(), true))
            {
                var docCount = reader.MaxDoc;

                Assert.Equal(7, docCount);

                var doc = reader.Document(0);
                Assert.NotNull(doc);
                //Id
                Assert.Equal("P17", doc.GetField(Labels.Id.ToString()).StringValue);

                //Label
                Assert.Equal("country", doc.GetField(Labels.Label.ToString()).StringValue);

                //Alt-Label:
                Assert.Equal(4, doc.GetFields(Labels.AltLabel.ToString()).Length);

                Assert.Equal("sovereign state", doc.GetField(Labels.AltLabel.ToString()).StringValue);

                Assert.Equal("sovereign state", doc.GetFields(Labels.AltLabel.ToString())[0].StringValue);
                Assert.Equal("state", doc.GetFields(Labels.AltLabel.ToString())[1].StringValue);
                Assert.Equal("land", doc.GetFields(Labels.AltLabel.ToString())[2].StringValue);

                //Description
                Assert.Equal("sovereign state of this item; don't use on humans", doc.GetField(Labels.Description.ToString()).StringValue);

                //Frequency
                Assert.Equal("3", doc.GetField(Labels.Frequency.ToString()).StringValue);
            }

            if (Directory.Exists(outputPath))
                Directory.Delete(outputPath, true);
        }
    }
}