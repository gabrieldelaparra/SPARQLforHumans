using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using SparqlForHumans.RDF.Extensions;
//using SparqlForHumans.RDF.Filtering;
using SparqlForHumans.RDF.Models;
using VDS.RDF;
using SparqlForHumans.Utilities;
using VDS.RDF.Writing.Formatting;

namespace SparqlForHumans.RDF.FilterReorderSort
{
    public static class TriplesFilterReorderSort
    {
        private static readonly NLog.Logger Logger = SparqlForHumans.Logger.Logger.Init();
        public static void FilterReorderSort(string inputTriplesFilename, string outputTriplesFilename = "", int triplesLimit = -1)
        {
            if (string.IsNullOrWhiteSpace(outputTriplesFilename))
                outputTriplesFilename = FileHelper.GetFilteredOutputFilename(inputTriplesFilename, triplesLimit);

            Options.InternUris = false;

            if (!new FileInfo(inputTriplesFilename).Exists)
                return;

            var outputFileInfo = new FileInfo(outputTriplesFilename);
            if (outputFileInfo.Directory != null && !outputFileInfo.Directory.Exists)
                outputFileInfo.Directory.Create();

            const int notifyTicks = 100000;
            long readCount = 0;
            long writeCount = 0;

            var wikidataDumpLines = FileHelper.GetInputLines(inputTriplesFilename);

            //using (var outputFileStream = File.Create(outputTriplesFilename))
            using (var outputFileStream = File.Create(outputTriplesFilename))
            {
                using var gZipStream = new GZipStream(outputFileStream, CompressionMode.Compress, true);
                Logger.Info("Read,Write");
                foreach (var line in wikidataDumpLines)
                {
                    readCount++;

                    if (readCount % notifyTicks == 0)
                        Logger.Info($"{readCount:N0};{writeCount:N0}");

                    try
                    {
                        if (!IsValidLine(line, triplesLimit))
                            continue;

                        var data = Encoding.UTF8.GetBytes($"{line}{Environment.NewLine}");
                        gZipStream.Write(data, 0, data.Length);
                        writeCount++;

                        var triple = line.ToTriple();
                        if (!IsValidReorderTriple(triple))
                            continue;

                        var newTriple = triple.ReorderTriple();
                        var newLine = newTriple.ToString(new NTriplesFormatter());

                        data = Encoding.UTF8.GetBytes($"{newLine}{Environment.NewLine}");
                        gZipStream.Write(data, 0, data.Length);
                        writeCount++;
                    }
                    catch (Exception e)
                    {
                        Logger.Error($"{readCount:N0};{line}");
                        Logger.Error(e);
                    }
                }

                Logger.Info($"{readCount:N0};{writeCount:N0}");
            }

            Logger.Info("Finished Filtering and reordering. Sorting via external sort. No debugging/progress messages available.");

            var process = new System.Diagnostics.Process();
            var startInfo = new System.Diagnostics.ProcessStartInfo
            {
                WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden,
                FileName = @"C:\Program Files\Git\usr\bin\sort.exe",
                Arguments = $"{outputTriplesFilename} -g -o {outputTriplesFilename}"
            };
            //gzip -dc filtered-All-FilterReorder.nt.gz | LANG=C sort -S 200M --parallel=4 -T tmp/ --compress-program=gzip | gzip > filtered-All-PostFilter-Sorted.nt.gz  
            process.StartInfo = startInfo;
            process.Start();

            Logger.Info("Finished sorting.");
        }

        public static bool IsValidReorderTriple(Triple triple)
        {
            //Object must be URI
            return triple.Object.IsUriNode() && triple.Object.IsEntityQ();
        }

        public static bool IsValidLine(string line, int entityLimit = -1)
        {
            const char space = ' ';
            var subjectIndex = line.IndexOf(space);
            var subject = line.Substring(0, subjectIndex - 1);

            var rest = line.Substring(subjectIndex + 1);
            var predicateIndex = rest.IndexOf(space);
            var predicate = rest.Substring(1, predicateIndex - 2);

            var objct = rest.Substring(predicateIndex + 1, rest.Length - predicateIndex - 3);

            //Subject Part:
            const string subjectPrefix = "<http://www.wikidata.org/entity/";
            if (!subject.StartsWith(subjectPrefix)) return false;
            var subjectId = subject.Substring(subjectPrefix.Length, subject.Length - subjectPrefix.Length);
            if (!subjectId.StartsWith("Q") && !subjectId.StartsWith("P")) return false;
            if (entityLimit > 0)
            {
                var subjectIntId = subjectId.ToInt();
                if (subjectId.StartsWith("Q") && subjectIntId > entityLimit) return false;
            }

            const string label = "http://www.w3.org/2000/01/rdf-schema#label";
            const string description = "http://schema.org/description";
            const string altLabel = "http://www.w3.org/2004/02/skos/core#altLabel";
            const string propDirect = "http://www.wikidata.org/prop/direct/";

            const string literalPrefix = "\"";
            const string literalSufix = "\"@en";

            //Predicate Part:
            //if(!predicate.StartsWith(label))
            if (predicate.StartsWith(propDirect))
            {
                if (!subjectId.StartsWith("Q"))
                    return false;

                if (!objct.StartsWith(subjectPrefix))
                {
                    return false;
                }
                else
                {
                    var objectId = objct.Substring(subjectPrefix.Length, objct.Length - subjectPrefix.Length - 1);
                    if (!objectId.StartsWith("Q"))
                        return false;
                    else
                    {
                        if (entityLimit > 0)
                        {
                            var objectIntId = objectId.ToInt();
                            if (objectIntId > entityLimit)
                                return false;
                        }
                    }
                }
            }
            else if (predicate.StartsWith(label) || predicate.StartsWith(description) || predicate.StartsWith(altLabel))
            {
                if (!objct.StartsWith(literalPrefix) || !objct.EndsWith(literalSufix))
                    return false;
            }
            else
            {
                return false;
            }

            return true;
        }

    }
}
