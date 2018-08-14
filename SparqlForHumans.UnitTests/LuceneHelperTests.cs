using System.IO;
using System.Linq;
using SparqlForHumans.Core.Services;
using SparqlForHumans.Core.Utilities;
using Xunit;

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

            if (Directory.Exists(outputPath))
                Directory.Delete(outputPath, true);

            Assert.False(Directory.Exists(outputPath));
            IndexBuilder.CreateEntitiesIndex(filename, outputPath);
            Assert.True(Directory.Exists(outputPath));

            Assert.Equal(entitiesCount, outputPath.GetLuceneDirectory().GetDocumentCount());
        }

        [Fact]
        public void TestGetLucenePath()
        {
            var path = "Temp";
            var luceneDirectory = path.GetLuceneDirectory();
            Assert.NotNull(luceneDirectory);
            Assert.IsAssignableFrom<Lucene.Net.Store.Directory>(luceneDirectory);

            Assert.True(Directory.Exists(path));
            Directory.Delete(path);
        }
    }
}