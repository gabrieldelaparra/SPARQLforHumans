using System;
using System.IO;
using System.Linq;
using SparqlForHumans.Utilities;
using Xunit;

namespace SparqlForHumans.UnitTests
{
    public class FileHelperTests
    {
        [Fact]
        public void TestCountLines()
        {
            const string filename = @"Resources/TenLines.nt";
            Assert.NotEqual(0, FileHelper.GetLineCount(filename));
            Assert.Equal(10, FileHelper.GetLineCount(filename));
        }

        [Fact]
        public void TestGetFilenameType()
        {
            Assert.Equal(FileHelper.FileType.Unkwown, FileHelper.GetFilenameType("Resources/IndexSingle/segments.gen"));
            Assert.Equal(FileHelper.FileType.nTriples, FileHelper.GetFilenameType("Resources/empty.nt"));
            Assert.Equal(FileHelper.FileType.nTriples, FileHelper.GetFilenameType("Resources/empty.nt.gz"));
            Assert.Equal(FileHelper.FileType.gZip, FileHelper.GetFilenameType("Resources/TenLines.nt.gz"));
        }

        [Fact]
        public void TestGetFilenameTypeNotExisting()
        {
            Assert.Throws<ArgumentException>(() => FileHelper.GetFilenameType("Resources/NotExisting.txt"));
        }

        [Fact]
        public void TestGetLinesCompressed()
        {
            const string filename = "Resources/TenLines.nt.gz";

            Assert.True(File.Exists(filename));

            Assert.NotNull(FileHelper.GetInputLines(filename));
            Assert.NotEmpty(FileHelper.GetInputLines(filename));
            Assert.Equal(10, FileHelper.GetInputLines(filename).Count());
        }

        [Fact]
        public void TestGetLinesUnknownThrowsException()
        {
            const string filename = "Resources/IndexSingle/segments.gen";

            Assert.True(File.Exists(filename));

            Assert.Throws<ArgumentException>(() => FileHelper.GetInputLines(filename));
        }

        [Fact]
        public void TestGetOrCreateDirectory()
        {
            const string path = "TestGetOrCreate";

            if (Directory.Exists(path))
                Directory.Delete(path);

            Assert.False(Directory.Exists(path));

            var dir = path.GetOrCreateDirectory();

            Assert.NotNull(dir);
            Assert.IsAssignableFrom<DirectoryInfo>(dir);

            Assert.True(Directory.Exists(path));
            Directory.Delete(path);
        }

        [Fact]
        public void TestGetOutputFilteredFilename()
        {
            const string filename = @"C:\a\b\c\TrimmedTestSet.nt";
            const int limit = 500;
            var outputFilename = FileHelper.GetFilteredOutputFilename(filename, limit);
            Assert.Equal(@"TrimmedTestSet.filter500.gz", outputFilename);
        }

        [Fact]
        public void TestGetOutputFilteredFilenameReduceLimitK()
        {
            const string filename = @"C:\a\b\c\TrimmedTestSet.nt";
            const int limit = 5000;
            var outputFilename = FileHelper.GetFilteredOutputFilename(filename, limit);
            Assert.Equal(@"TrimmedTestSet.filter5K.gz", outputFilename);
        }

        [Fact]
        public void TestGetOutputFilteredFilenameReduceLimitM()
        {
            const string filename = @"C:\a\b\c\TrimmedTestSet.nt";
            const int limit = 50000000;
            var outputFilename = FileHelper.GetFilteredOutputFilename(filename, limit);
            Assert.Equal(@"TrimmedTestSet.filter50M.gz", outputFilename);
        }

        [Fact]
        public void TestGetOutputFilename_nt()
        {
            const string filename = @"C:\a\b\c\TrimmedTestSet.nt";
            const int limit = 500;
            var outputFilename = FileHelper.GetFilteredOutputFilename(filename, limit);
            Assert.Equal(@"TrimmedTestSet.filter500.gz", outputFilename);
        }

        [Fact]
        public void TestGetOutputFilename_ntgz()
        {
            const string filename = @"C:\a\b\c\TrimmedTestSet.nt.gz";
            const int limit = 500;
            var outputFilename = FileHelper.GetFilteredOutputFilename(filename, limit);
            Assert.Equal(@"TrimmedTestSet.filter500.gz", outputFilename);
        }

        [Fact]
        public void TestGetOutputFilename_filtered_ntgz()
        {
            const string filename = @"C:\a\b\c\TrimmedTestSet.filtered2K.nt.gz";
            const int limit = 500;
            var outputFilename = FileHelper.GetFilteredOutputFilename(filename, limit);
            Assert.Equal(@"TrimmedTestSet.filter500.gz", outputFilename);
        }

        [Fact]
        public void TestGetOutputFilename_All()
        {
            const string filename = @"C:\a\b\c\TrimmedTestSet.nt.gz";
            const int limit = -1;
            var outputFilename = FileHelper.GetFilteredOutputFilename(filename, limit);
            Assert.Equal(@"TrimmedTestSet.filterAll.gz", outputFilename);
        }

        [Fact]
        public void TestGetOutputFilename_NoLimit()
        {
            const string filename = @"C:\a\b\c\TrimmedTestSet.nt.gz";
            var outputFilename = FileHelper.GetFilteredOutputFilename(filename);
            Assert.Equal(@"TrimmedTestSet.filterAll.gz", outputFilename);
        }

        [Fact]
        public void TestGetOutputFilename_Zero()
        {
            const string filename = @"C:\a\b\c\TrimmedTestSet.nt.gz";
            const int limit = 0;
            var outputFilename = FileHelper.GetFilteredOutputFilename(filename, limit);
            Assert.Equal(@"TrimmedTestSet.filter0.gz", outputFilename);
        }

        [Fact]
        public void TestReadLines()
        {
            const string filename = "Resources/TenLines.nt";

            Assert.True(File.Exists(filename));

            Assert.NotNull(FileHelper.ReadLines(filename));
            Assert.NotEmpty(FileHelper.ReadLines(filename));
            Assert.Equal(10, FileHelper.ReadLines(filename).Count());
        }

        [Fact]
        public void TestReadLinesCompressedNotExisting()
        {
            const string filename = "Resources/NotExisting.nt.gz";
            Assert.Empty(SharpZipHandler.ReadGZip(filename));
        }

        [Fact]
        public void TestTrimFile()
        {
            const string outputFilename = "trimmedFile.nt";

            if (File.Exists(outputFilename))
                File.Delete(outputFilename);

            Assert.False(File.Exists(outputFilename));

            const string filename = @"Resources/TenLines.nt";
            Assert.Equal(10, FileHelper.GetLineCount(filename));

            FileHelper.TrimFile(filename, outputFilename, 5);

            Assert.True(File.Exists(outputFilename));
            Assert.Equal(5, FileHelper.GetLineCount(outputFilename));
        }
    }
}