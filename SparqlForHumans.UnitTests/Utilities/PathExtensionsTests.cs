using System.IO;
using SparqlForHumans.Utilities;
using Xunit;

namespace SparqlForHumans.UnitTests.Utilities
{
    public class PathExtensionsTests
    {
        [Fact]
        public void TestDeleteIfExistsDirectory()
        {
            var path = "tempFolder";

            path.DeleteIfExists();
            Assert.False(Directory.Exists(path));
            
            Directory.CreateDirectory(path);
            Assert.True(Directory.Exists(path));

            path.DeleteIfExists();
            Assert.False(Directory.Exists(path));

            path.DeleteIfExists();
            Assert.False(Directory.Exists(path));
        }

        [Fact]
        public void TestDeleteIfExistsFile()
        {
            var path = "tempFile";

            path.DeleteIfExists();
            Assert.False(File.Exists(path));

            using (var file = File.Create(path))
            {
            }
            Assert.True(File.Exists(path));

            path.DeleteIfExists();
            Assert.False(File.Exists(path));

            path.DeleteIfExists();
            Assert.False(File.Exists(path));
        }
    }
}
