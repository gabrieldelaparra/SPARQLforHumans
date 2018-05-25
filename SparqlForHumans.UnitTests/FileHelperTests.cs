using SparqlForHumans.Core.Services;
using System.IO;
using System.Linq;
using Xunit;

namespace SparqlForHumans.UnitTests
{
    public class FileHelperTests
    {
        [Fact]
        public void TestGetFileType()
        {
            Assert.Equal(FileHelper.FileType.Unkwown, FileHelper.GetFileType(".txt"));
            Assert.Equal(FileHelper.FileType.nTriples, FileHelper.GetFileType(".nt"));
            Assert.Equal(FileHelper.FileType.gZip, FileHelper.GetFileType(".gz"));
        }

        [Fact]
        public void TestGetFilenameType()
        {
            Assert.Equal(FileHelper.FileType.Unkwown, FileHelper.GetFilenameType("Resources/empty.txt"));
            Assert.Equal(FileHelper.FileType.nTriples, FileHelper.GetFilenameType("Resources/empty.nt"));
            Assert.Equal(FileHelper.FileType.gZip, FileHelper.GetFilenameType("Resources/empty.gz"));
            Assert.Equal(FileHelper.FileType.gZip, FileHelper.GetFilenameType("Resources/empty.nt.gz"));
        }

        [Fact]
        public void TestReadLines()
        {
            string filename = "Resources/TenLines.nt";

            Assert.True(File.Exists(filename));

            Assert.NotNull(FileHelper.ReadLines(filename));
            Assert.NotEmpty(FileHelper.ReadLines(filename));
            Assert.Equal(10, FileHelper.ReadLines(filename).Count());
        }

        [Fact]
        public void TestCountLines()
        {
            var filename = @"Resources/TenLines.nt";
            Assert.NotEqual(0, FileHelper.GetLineCount(filename));
            Assert.Equal(10, FileHelper.GetLineCount(filename));
            Assert.True(File.Exists("LineCountLog.txt"));
        }

        [Fact]
        public void TestTrimFile()
        {
            var outputFilename = "trimmedFile.nt";

            if (File.Exists(outputFilename))
                File.Delete(outputFilename);

            Assert.False(File.Exists(outputFilename));

            var filename = @"Resources/TenLines.nt";
            Assert.Equal(10, FileHelper.GetLineCount(filename));

            FileHelper.TrimFile(filename, outputFilename, 5);

            Assert.True(File.Exists(outputFilename));
            Assert.Equal(5, FileHelper.GetLineCount(outputFilename));
        }

        [Fact]
        public void TestTrimLargeNTFile()
        {
            var filename = @"C:\Users\admin\Desktop\DCC\latest-truthy.nt-gz\latest-truthy.nt";
            var outputFilename = "TrimmedTestSet.nt";

            if (File.Exists(outputFilename))
                File.Delete(outputFilename);

            int limit = 500000;

            FileHelper.TrimFile(filename, outputFilename, limit);

            Assert.True(File.Exists(outputFilename));
            Assert.Equal(limit, FileHelper.GetLineCount(outputFilename));
        }

        [Fact]
        public void TestTrimLargeGZipFile()
        {
            var filename = @"C:\Users\admin\Desktop\DCC\latest-truthy.nt-gz\latest-truthy.nt.gz";
            var outputFilename = "TrimmedTestSet.nt";

            if (File.Exists(outputFilename))
                File.Delete(outputFilename);

            FileHelper.TrimFile(filename, outputFilename, 500000);

            Assert.True(File.Exists(outputFilename));
            Assert.Equal(500000, FileHelper.GetLineCount(outputFilename));
        }
    }
}
