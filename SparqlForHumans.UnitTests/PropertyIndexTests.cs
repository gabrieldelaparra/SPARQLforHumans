using System.IO;
using System.Linq;
using Lucene.Net.Index;
using Lucene.Net.Store;
using SparqlForHumans.Lucene.Extensions;
using SparqlForHumans.Lucene.Indexing;
using SparqlForHumans.Lucene.Queries;
using SparqlForHumans.Models;
using SparqlForHumans.Models.LuceneIndex;
using SparqlForHumans.Utilities;
using Xunit;
using Directory = System.IO.Directory;

namespace SparqlForHumans.UnitTests
{
    public class PropertyIndexTests
    {
        [Fact]
        public void TestCreateBasicIndex()
        {
            const string filename = @"Resources/PropertyIndex.nt";
            Assert.True(File.Exists(filename));

            const string outputPath = "PropertyIndex";

            outputPath.DeleteIfExists();

            Assert.False(Directory.Exists(outputPath));

            using (var propertiesDirectory = FSDirectory.Open(outputPath.GetOrCreateDirectory()))
            {
                //ACT
                PropertiesIndex.CreatePropertiesIndex(filename, propertiesDirectory, true);

                //ASSERT
                Assert.True(Directory.Exists(outputPath));

                using (var reader = DirectoryReader.Open(propertiesDirectory))
                {
                    var docCount = reader.MaxDoc;

                    Assert.Equal(7, docCount);

                    var doc = reader.Document(0);
                    Assert.NotNull(doc);
                    //Id
                    Assert.Equal("P17", doc.GetValue(Labels.Id));

                    //Label
                    Assert.Equal("country", doc.GetValue(Labels.Label));

                    //Alt-Label:
                    Assert.Equal(4, doc.GetValues(Labels.AltLabel).Length);

                    Assert.Equal("sovereign state", doc.GetValue(Labels.AltLabel));

                    Assert.Equal("sovereign state", doc.GetValues(Labels.AltLabel)[0]);
                    Assert.Equal("state", doc.GetValues(Labels.AltLabel)[1]);
                    Assert.Equal("land", doc.GetValues(Labels.AltLabel)[2]);

                    //Description
                    Assert.Equal("sovereign state of this item; don't use on humans",
                        doc.GetValue(Labels.Description));

                    //Frequency
                    Assert.Equal("3", doc.GetValue(Labels.Rank));
                }
            }

            outputPath.DeleteIfExists();
        }
    }
}