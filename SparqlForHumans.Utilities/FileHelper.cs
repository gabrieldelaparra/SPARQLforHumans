using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace SparqlForHumans.Utilities
{
    public static class FileHelper
    {
        public enum FileType
        {
            Unkwown,
            nTriples,
            gZip
        }

        private static readonly NLog.Logger Logger = SparqlForHumans.Logger.Logger.Init();

        public static string GetFilteredOutputFilename(string inputFilename, int limit)
        {
            return GetCustomOutputFilename(inputFilename, limit, "filtered");
        }

        public static string GetTrimmedOutputFilename(string inputFilename, int limit)
        {
            return GetCustomOutputFilename(inputFilename, limit, "trimmed");
        }

        private static string GetCustomOutputFilename(string inputFilename, int limit, string customKeyword)
        {
            var filename = Path.GetFileNameWithoutExtension(inputFilename);
            return inputFilename.Replace(filename, $"{customKeyword}-{filename}-{limit}");
        }

        public static DirectoryInfo GetOrCreateDirectory(this string path)
        {
            var directoryInfo = new DirectoryInfo(path);

            if (!directoryInfo.Exists)
                directoryInfo.Create();

            return directoryInfo;
        }

        public static void TrimFile(string filename, string outputFilename, int lineLimit)
        {
            var lines = GetInputLines(filename);
            File.WriteAllLines(outputFilename, lines.Take(lineLimit));
        }

        public static long GetLineCount(string filename, int notifyTicks = 100000)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            long lineCount = 0;

            var lines = GetInputLines(filename);

            foreach (var item in lines)
            {
                lineCount++;
                if (lineCount % notifyTicks == 0)
                    Logger.Trace($"{stopwatch.ElapsedMilliseconds},{lineCount}");
            }

            Logger.Trace($"{stopwatch.ElapsedMilliseconds},{lineCount}");

            stopwatch.Stop();
            return lineCount;
        }

        public static IEnumerable<string> GetInputLines(string inputTriples)
        {
            IEnumerable<string> lines;
            switch (GetFilenameType(inputTriples))
            {
                case FileType.nTriples:
                    lines = ReadLines(inputTriples);
                    break;
                case FileType.gZip:
                    lines = SharpZipHandler.ReadGZip(inputTriples);
                    break;
                default:
                case FileType.Unkwown:
                    throw new ArgumentException("Not a valid file");
            }

            return lines;
        }

        public static IEnumerable<string> ReadLines(string filename)
        {
            using (var streamReader = new StreamReader(new FileStream(filename, FileMode.Open)))
            {
                while (!streamReader.EndOfStream)
                    yield return streamReader.ReadLine();
            }
        }

        public static FileType GetFilenameType(string filename)
        {
            if (!File.Exists(filename))
                throw new ArgumentException("Filename does not exists");

            return GetFileType(Path.GetExtension(filename));
        }

        public static FileType GetFileType(string filename)
        {
            var extension = Path.GetExtension(filename);
            switch (extension)
            {
                case ".nt":
                    return FileType.nTriples;
                case ".gz":
                    return FileType.gZip;
                default:
                    return FileType.Unkwown;
            }
        }
    }
}