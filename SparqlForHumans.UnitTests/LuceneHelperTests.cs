using SparqlForHumans.Core.Services;
using SparqlForHumans.Core.Utilities;
using System.IO;
using System.Linq;
using Xunit;

namespace SparqlForHumans.UnitTests
{
    public class LuceneHelperTests
    {
        [Fact]
        public void TestGetLucenePath()
        {
            var path = "Temp";
            var luceneDirectory = LuceneHelper.GetLuceneDirectory(path);
            Assert.NotNull(luceneDirectory);
            Assert.IsAssignableFrom<Lucene.Net.Store.Directory>(luceneDirectory);

            Assert.True(Directory.Exists(path));
        }

        [Fact]
        public void TestCountDocuments()
        {
            var filename = "Resources/filtered.nt";
            var lines = FileHelper.GetInputLines(filename);
            var groups = lines.GetSameEntityGroups();
            var entitiesCount = groups.Count();

            var outputPath = "Index";

            if (Directory.Exists(outputPath))
                Directory.Delete(outputPath, true);

            Assert.False(Directory.Exists(outputPath));
            IndexBuilder.CreateIndex(filename, outputPath);
            Assert.True(Directory.Exists(outputPath));

            Assert.Equal(entitiesCount, LuceneHelper.GetDocumentCount(LuceneHelper.GetLuceneDirectory(outputPath)));
        }
    }
}
