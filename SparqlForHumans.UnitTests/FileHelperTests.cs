using System.IO;
using System.Linq;
using SparqlForHumans.Core.Utilities;
using Xunit;

namespace SparqlForHumans.UnitTests
{
    public class FileHelperTests
    {
        [Fact]
        public void TestCountLines()
        {
            var filename = @"Resources/TenLines.nt";
            Assert.NotEqual(0, FileHelper.GetLineCount(filename));
            Assert.Equal(10, FileHelper.GetLineCount(filename));
            Assert.True(File.Exists("LineCountLog.txt"));
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
        public void TestGetFileType()
        {
            Assert.Equal(FileHelper.FileType.Unkwown, FileHelper.GetFileType(".txt"));
            Assert.Equal(FileHelper.FileType.nTriples, FileHelper.GetFileType(".nt"));
            Assert.Equal(FileHelper.FileType.gZip, FileHelper.GetFileType(".gz"));
        }

        [Fact]
        public void TestGetOutputFilteredFilename()
        {
            var filename = @"C:\a\b\c\TrimmedTestSet.nt";
            var limit = 500;
            var outputFilename = FileHelper.GetFilteredOutputFilename(filename, limit);
            Assert.Equal(@"C:\a\b\c\filtered-TrimmedTestSet-500.nt", outputFilename);
        }

        [Fact]
        public void TestGetOutputTrimmedFilename()
        {
            var filename = @"C:\a\b\c\TrimmedTestSet.nt";
            var limit = 500;
            var outputFilename = FileHelper.GetTrimmedOutputFilename(filename, limit);
            Assert.Equal(@"C:\a\b\c\trimmed-TrimmedTestSet-500.nt", outputFilename);
        }

        [Fact]
        public void TestReadLines()
        {
            var filename = "Resources/TenLines.nt";

            Assert.True(File.Exists(filename));

            Assert.NotNull(FileHelper.ReadLines(filename));
            Assert.NotEmpty(FileHelper.ReadLines(filename));
            Assert.Equal(10, FileHelper.ReadLines(filename).Count());
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

        //[Fact]
        //public void TestTrimLargeGZipFile()
        //{
        //    var filename = @"C:\Users\admin\Desktop\DCC\latest-truthy.nt-gz\latest-truthy.nt.gz";
        //    var outputFilename = "TrimmedTestSet.nt";

        //    if (File.Exists(outputFilename))
        //        File.Delete(outputFilename);

        //    var limit = 50000;

        //    FileHelper.TrimFile(filename, outputFilename, limit);

        //    Assert.True(File.Exists(outputFilename));
        //    Assert.Equal(limit, FileHelper.GetLineCount(outputFilename));
        //}

        //[Fact]
        //public void TestTrimLargeNTFile()
        //{
        //    var filename = @"C:\Users\admin\Desktop\DCC\latest-truthy.nt-gz\latest-truthy.nt";
        //    var outputFilename = "TrimmedTestSet.nt";

        //    if (File.Exists(outputFilename))
        //        File.Delete(outputFilename);

        //    var limit = 50000;

        //    FileHelper.TrimFile(filename, outputFilename, limit);

        //    Assert.True(File.Exists(outputFilename));
        //    Assert.Equal(limit, FileHelper.GetLineCount(outputFilename));
        //}

        [Fact]
        public void TestGetOrCreateDirectory()
        {
            var path = "TestGetOrCreate";

            if(Directory.Exists(path))
                Directory.Delete(path);

            Assert.False(Directory.Exists(path));

            var dir = FileHelper.GetOrCreateDirectory(path);

            Assert.NotNull(dir);
            Assert.IsAssignableFrom<DirectoryInfo>(dir);

            Assert.True(Directory.Exists(path));
            Directory.Delete(path);
        }
    }
}