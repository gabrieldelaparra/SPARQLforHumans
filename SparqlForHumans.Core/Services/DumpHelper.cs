﻿using SparqlForHumans.Core.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using VDS.RDF;
using VDS.RDF.Parsing;

namespace SparqlForHumans.Core.Services
{
    public class DumpHelper
    {
        /// <summary>
        /// Reads an Wikidata GZipped N-triples dump.
        /// Foreach line in the Triples file:
        /// - Parse the RDF triple
        /// - For the following cases, skip:
        ///      - If Subject Q-Code > 2mill
        ///      - If Object Q-Code > 2mill
        ///      - If Language != EN
        /// Else, Add the triple to a new NT File
        /// </summary>
        /// <param name="inputTriples">Wikidata GZipped N-triples dump</param>
        /// <param name="outputTriples">Filtered Wikidata (Non-GZipped) N-triples dump</param>
        static void FilterTriples(string inputTriples, string outputTriples)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            if (!new FileInfo(inputTriples).Exists) return;

            var outputFileInfo = new FileInfo(outputTriples);
            if (outputFileInfo.Directory != null && !outputFileInfo.Directory.Exists)
                outputFileInfo.Directory.Create();

            Options.InternUris = false;
            //Read GZip File
            //var wikidataDumpLines = GZipHandler.ReadGZip(inputTriples);
            var wikidataDumpLines = FileHelper.ReadLines(inputTriples);
            var notifyTicks = 100000;
            var readCount = 0;
            var writeCount = 0;

            using (var logStreamWriter = new StreamWriter(new FileStream("ProgressLog.txt", FileMode.Create)))
            using (var errorStreamWriter = new StreamWriter(new FileStream("ErrorsLog.txt", FileMode.Create)))
            using (var filteredStreamWriter = new StreamWriter(new FileStream(outputTriples, FileMode.Create)))
            {
                logStreamWriter.AutoFlush = true;
                errorStreamWriter.AutoFlush = true;
                filteredStreamWriter.AutoFlush = true;

                logStreamWriter.WriteLine("ElapsedTime,Read,Write");
                foreach (var line in wikidataDumpLines)
                {
                    readCount++;
                    if (readCount % notifyTicks == 0)
                    {
                        logStreamWriter.WriteLine($"{stopwatch.ElapsedMilliseconds},{readCount},{writeCount}");
                        Console.WriteLine($"{stopwatch.ElapsedMilliseconds},{readCount},{((double)readCount / (double)447622549) * 100}");
                        //DEBUG:
                        //if (readCount == 100 * notifyTicks) break;
                    }
                    try
                    {
                        var g = new NonIndexedGraph();
                        StringParser.Parse(g, line);
                        var statement = g.Triples.Last();

                        if (statement.Subject.NodeType != NodeType.Uri) continue;

                        var ntSubject = (UriNode)statement.Subject;
                        var ntPredicate = statement.Predicate;
                        var ntObject = statement.Object;

                        //Condition: Subject is not Entity: Skip
                        if (!ntSubject.ToSafeString().Contains(IndexProperties.entityIRI)) continue;

                        //Condition: Subject is Entity and Q > 2.000.000: Skip
                        if (ntSubject.Uri.Segments.Last().Contains(IndexProperties.entityPrefix))
                        {
                            var index = ntSubject.Uri.Segments.Last().Replace(IndexProperties.entityPrefix, string.Empty);
                            int.TryParse(index, out int indexInt);
                            if (indexInt > 2000000) continue;
                        }

                        //Condition: Object is Entity and Q > 2.000.000: Skip
                        if (ntObject.NodeType == NodeType.Uri && ((UriNode)ntObject).Uri.Segments.Count() > 0 && ((UriNode)ntObject).Uri.Segments.Last().Contains(IndexProperties.entityPrefix))
                        {
                            var index = ((UriNode)ntObject).Uri.Segments.Last().Replace(IndexProperties.entityPrefix, string.Empty);
                            int.TryParse(index, out int indexInt);
                            if (indexInt > 2000000) continue;
                        }

                        //Condition: Object is Literal: Filter @en only
                        if (ntObject.NodeType == NodeType.Literal)
                            if (!((LiteralNode)ntObject).Language.Equals("en")) continue;

                        filteredStreamWriter.WriteLine(line);
                        writeCount++;
                    }
                    catch (Exception e)
                    {
                        errorStreamWriter.WriteLine($"{stopwatch.ElapsedMilliseconds},{readCount},{line},{e.Message}");
                        //errorStreamWriter.Flush();
                    }
                }
                logStreamWriter.WriteLine($"{stopwatch.ElapsedMilliseconds},{readCount},{writeCount}");
            }
            stopwatch.Stop();
        }

        public static void GetLineCount(string inputTriples)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            using (var logStreamWriter = new StreamWriter(new FileStream("LineCountLog.txt", FileMode.Create)))
            {
                var notifyTicks = 100000;
                var lineCount = 0;
                var lines = GZipHandler.ReadGZip(inputTriples);
                foreach (var item in lines)
                {
                    lineCount++;
                    if (lineCount % notifyTicks == 0)
                    {
                        logStreamWriter.WriteLine($"{stopwatch.ElapsedMilliseconds},{lineCount}");
                    }
                }
                logStreamWriter.WriteLine($"{stopwatch.ElapsedMilliseconds},{lineCount}");
            }
            stopwatch.Stop();
        }
    }
}
