using System.Linq;
using Lucene.Net.Index;
using Lucene.Net.Store;
using SparqlForHumans.Lucene.Extensions;
using SparqlForHumans.Lucene.Indexing;
using SparqlForHumans.Utilities;
using Xunit;
using Directory = System.IO.Directory;

namespace SparqlForHumans.UnitTests
{
    public class LuceneHelperTests
    {
        [Fact]
        public void TestCountDocuments()
        {
            var filename = "Resources/filtered.nt";
            var lines = FileHelper.GetInputLines(filename);
            var groups = lines.GroupBySubject();
            var entitiesCount = groups.Count();

            var outputPath = "Index";

            outputPath.DeleteIfExists();

            Assert.False(Directory.Exists(outputPath));

            using (var luceneIndexDirectory = FSDirectory.Open(outputPath.GetOrCreateDirectory()))
            {
                EntitiesIndex.CreateEntitiesIndex(filename, luceneIndexDirectory);
                using (var reader = DirectoryReader.Open(luceneIndexDirectory))
                {
                    Assert.True(Directory.Exists(outputPath));
                    Assert.Equal(entitiesCount, reader.MaxDoc);
                }
            }

            outputPath.DeleteIfExists();
        }
    }
}