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
            Assert.Equal(FileHelper.FileType.Unkwown, FileHelper.GetFilenameType("Resources/empty.txt"));
            Assert.Equal(FileHelper.FileType.nTriples, FileHelper.GetFilenameType("Resources/empty.nt"));
            Assert.Equal(FileHelper.FileType.gZip, FileHelper.GetFilenameType("Resources/empty.gz"));
            Assert.Equal(FileHelper.FileType.gZip, FileHelper.GetFilenameType("Resources/empty.nt.gz"));
        }

        [Fact]
        public void TestGetFilenameTypeNotExisting()
        {
            Assert.Throws<ArgumentException>(() => FileHelper.GetFilenameType("Resources/NotExisting.txt"));
        }

        [Fact]
        public void TestGetFileType()
        {
            Assert.Equal(FileHelper.FileType.Unkwown, FileHelper.GetFileType(".txt"));
            Assert.Equal(FileHelper.FileType.nTriples, FileHelper.GetFileType(".nt"));
            Assert.Equal(FileHelper.FileType.gZip, FileHelper.GetFileType(".gz"));
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
        public void TestGetLinesOther()
        {
            const string filename = "Resources/empty.txt";

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
            Assert.Equal(@"C:\a\b\c\filtered-TrimmedTestSet-500.nt", outputFilename);
        }

        [Fact]
        public void TestGetOutputTrimmedFilename()
        {
            const string filename = @"C:\a\b\c\TrimmedTestSet.nt";
            const int limit = 500;
            var outputFilename = FileHelper.GetTrimmedOutputFilename(filename, limit);
            Assert.Equal(@"C:\a\b\c\trimmed-TrimmedTestSet-500.nt", outputFilename);
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