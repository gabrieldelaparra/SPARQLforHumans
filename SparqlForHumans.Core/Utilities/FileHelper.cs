using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using NLog;

namespace SparqlForHumans.Core.Utilities
{
    public static class FileHelper
    {
        private static readonly NLog.Logger Logger = Utilities.Logger.Init();

        public enum FileType
        {
            Unkwown,
            nTriples,
            gZip
        }

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

        //TODO: Test
        public static DirectoryInfo GetOrCreateDirectory(string path)
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

        public static long GetLineCount(string filename)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            long lineCount = 0;
            using (var logStreamWriter = new StreamWriter(new FileStream("LineCountLog.txt", FileMode.Create)))
            {
                const int notifyTicks = 100000;

                var lines = GetInputLines(filename);

                foreach (var item in lines)
                {
                    lineCount++;
                    if (lineCount % notifyTicks == 0)
                        logStreamWriter.WriteLine($"{stopwatch.ElapsedMilliseconds},{lineCount}");
                }

                logStreamWriter.WriteLine($"{stopwatch.ElapsedMilliseconds},{lineCount}");
            }

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
                    //lines = GZipHandler.ReadGZip(inputTriples);
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

        public static IEnumerable<string> ReadLinesTrace(string filename, long maxLines)
        {
            const long count = 0;
            using (var streamReader = new StreamReader(new FileStream(filename, FileMode.Open)))
            {
                while (!streamReader.EndOfStream || count < maxLines)
                {
                    var line = streamReader.ReadLine();
                    Logger.Trace(line);
                    yield return line;

                }
            }
        }

        public static FileType GetFilenameType(string filename)
        {
            if (!File.Exists(filename)) throw new ArgumentException("Filename does not exists");

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