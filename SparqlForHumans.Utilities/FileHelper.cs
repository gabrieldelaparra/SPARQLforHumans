using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using HeyRed.Mime;

namespace SparqlForHumans.Utilities
{
    public static class FileHelper
    {
        public enum FileType
        {
            Unknown,
            nTriples,
            gZip
        }

        private static readonly NLog.Logger Logger = SparqlForHumans.Logger.Logger.Init();

        public static void CreatePathIfNotExists(this string filepath)
        {
            var fileInfo = new FileInfo(filepath);
            if (!fileInfo.Directory.Exists) fileInfo.Directory.Create();
        }

        public static FileType GetFilenameType(string filePath)
        {
            if (!File.Exists(filePath)) throw new ArgumentException("Filename does not exists");

            return GetFileType(filePath);
        }

        public static FileType GetFileType(string filePath)
        {
            var mimeType = MimeGuesser.GuessMimeType(filePath);

            return mimeType switch {
                "text/plain" => FileType.nTriples,
                "application/x-gzip" => FileType.gZip,
                _ => FileType.Unknown
            };
        }

        public static string GetFilteredOutputFilename(string inputFilename, int limit = -1)
        {
            var filename = GetOutputFileName(inputFilename);

            return $"{filename}.filter{GetReducedLimitName(limit)}.gz";
        }

        public static IEnumerable<string> GetInputLines(string inputTriples)
        {
            return GetFilenameType(inputTriples) switch {
                FileType.nTriples => ReadLines(inputTriples),
                FileType.gZip => SharpZipHandler.ReadGZip(inputTriples),
                _ => throw new ArgumentException("Not a valid file")
            };
        }

        public static long GetLineCount(string filename, int notifyTicks = 100000)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            long lineCount = 0;

            var lines = GetInputLines(filename);

            foreach (var item in lines) {
                lineCount++;
                if (lineCount % notifyTicks == 0) Logger.Trace($"{stopwatch.ElapsedMilliseconds},{lineCount}");
            }

            Logger.Trace($"{stopwatch.ElapsedMilliseconds},{lineCount}");

            stopwatch.Stop();
            return lineCount;
        }

        public static DirectoryInfo GetOrCreateDirectory(this string path)
        {
            var directoryInfo = new DirectoryInfo(path);

            if (!directoryInfo.Exists) directoryInfo.Create();

            return directoryInfo;
        }

        public static string GetReducedLimitName(int limit)
        {
            if (limit < 0) return "All";

            string[] sizes = {"", "K", "M", "G", "T"};
            var order = 0;
            const int splitter = 1000;
            while (limit >= splitter && order < sizes.Length - 1) {
                order++;
                limit = limit / splitter;
            }

            // Adjust the format string to your preferences. For example "{0:0.#}{1}" would
            // show a single decimal place, and no space.
            return $"{limit}{sizes[order]}";
        }

        public static string GetReorderedOutputFilename(string inputFilename)
        {
            var filename = GetOutputFileName(inputFilename);
            return $"{filename}.reordered.nt";
        }

        public static IEnumerable<string> ReadLines(string filename)
        {
            using var streamReader = new StreamReader(new FileStream(filename, FileMode.Open));
            while (!streamReader.EndOfStream) yield return streamReader.ReadLine();
        }

        public static void TrimFile(string filename, string outputFilename, int lineLimit)
        {
            var lines = GetInputLines(filename);
            File.WriteAllLines(outputFilename, lines.Take(lineLimit));
        }

        private static string GetOutputFileName(string inputFilename)
        {
            var filename = Path.GetFileNameWithoutExtension(inputFilename);
            var split = filename.Split('.');
            if (split.Length > 1) filename = split[0];

            return filename;
        }
    }
}