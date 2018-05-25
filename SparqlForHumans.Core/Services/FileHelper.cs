﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace SparqlForHumans.Core.Services
{
    public static class FileHelper
    {
        public static int GetLineCount(string inputTriples)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var count = 0;
            using (var logStreamWriter = new StreamWriter(new FileStream("LineCountLog.txt", FileMode.Create)))
            {
                var notifyTicks = 100000;
                var lineCount = 0;

                var lines = GetInputLines(inputTriples);

                foreach (var item in lines)
                {
                    lineCount++;
                    if (lineCount % notifyTicks == 0)
                    {
                        logStreamWriter.WriteLine($"{stopwatch.ElapsedMilliseconds},{lineCount}");
                    }
                }
                logStreamWriter.WriteLine($"{stopwatch.ElapsedMilliseconds},{lineCount}");
                count = lineCount;
            }
            stopwatch.Stop();
            return count;
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
                    lines = GZipHandler.ReadGZip(inputTriples);
                    break;
                default:
                case FileType.Unkwown:
                    throw new ArgumentException("Not a valid file");
            }
            return lines;
        }

        public static IEnumerable<string> ReadLines(string filename)
        {
            using (StreamReader streamReader = new StreamReader(new FileStream(filename, FileMode.Open)))
                while (!streamReader.EndOfStream)
                {
                    yield return streamReader.ReadLine();
                }
        }

        public enum FileType
        {
            Unkwown,
            nTriples,
            gZip,
        }

        public static FileType GetFileInfoType(FileInfo filename)
        {
            if (!filename.Exists) throw new ArgumentException("Filename does not exists");

            return GetFileType(filename.Extension);
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
