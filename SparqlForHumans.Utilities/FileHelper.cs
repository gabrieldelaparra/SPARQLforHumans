using HeyRed.Mime;
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

        public static string GetFilteredOutputFilename(string inputFilename, int limit = -1)
        {
            var filename = GetOutputFileName(inputFilename);

            return $"{filename}.filter{GetReducedLimitName(limit)}.gz";
        }

        private static string GetOutputFileName(string inputFilename)
        {
            var filename = Path.GetFileNameWithoutExtension(inputFilename);
            var split = filename.Split('.');
            if (split.Length > 1)
            {
                filename = split[0];
            }

            return filename;
        }

        public static DirectoryInfo GetOrCreateDirectory(this string path)
        {
            var directoryInfo = new DirectoryInfo(path);

            if (!directoryInfo.Exists)
            {
                directoryInfo.Create();
            }

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
                {
                    Logger.Trace($"{stopwatch.ElapsedMilliseconds},{lineCount}");
                }
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
                {
                    yield return streamReader.ReadLine();
                }
            }
        }

        public static FileType GetFilenameType(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new ArgumentException("Filename does not exists");
            }

            return GetFileType(filePath);
        }

        public static FileType GetFileType(string filePath)
        {
            var mimeType = MimeGuesser.GuessMimeType(filePath);

            switch (mimeType)
            {
                case "text/plain":
                    return FileType.nTriples;
                case "application/x-gzip":
                    return FileType.gZip;
                default:
                    return FileType.Unkwown;
            }
        }


        public static string GetReducedLimitName(int limit)
        {
            if (limit < 0)
            {
                return "All";
            }

            string[] sizes = { "", "K", "M" };
            var order = 0;
            const int splitter = 1000;
            while (limit >= splitter && order < sizes.Length - 1)
            {
                order++;
                limit = limit / splitter;
            }

            // Adjust the format string to your preferences. For example "{0:0.#}{1}" would
            // show a single decimal place, and no space.
            return $"{limit}{sizes[order]}";
        }
    }
}