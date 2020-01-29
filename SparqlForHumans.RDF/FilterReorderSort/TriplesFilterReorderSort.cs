using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using SparqlForHumans.RDF.Extensions;
using SparqlForHumans.RDF.Filtering;
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
                outputTriplesFilename = FileHelper.GetFilterReorderOutputFilename(inputTriplesFilename);

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
            using (var gZipStream = new GZipStream(outputFileStream, CompressionMode.Compress, true))
            {
                Logger.Info("Read,Write");
                foreach (var line in wikidataDumpLines)
                {
                    readCount++;

                    if (readCount % notifyTicks == 0)
                        Logger.Info($"{readCount:N0};{writeCount:N0}");

                    try
                    {
                        if(!TriplesFilter.IsValidLine(line, triplesLimit))
                            continue;

                        var data = Encoding.UTF8.GetBytes($"{line}{Environment.NewLine}");
                        gZipStream.Write(data, 0, data.Length);
                        writeCount++;

                        var triple = line.ToTriple();
                        if(!IsValidReorderTriple(triple))
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
            process.StartInfo = startInfo;
            process.Start();

            Logger.Info("Finished sorting.");
        }

        public static bool IsValidReorderTriple(Triple triple)
        {
            //Object must be URI
            return triple.Object.IsUriNode() && triple.Object.IsEntityQ();
        }

    }
}
