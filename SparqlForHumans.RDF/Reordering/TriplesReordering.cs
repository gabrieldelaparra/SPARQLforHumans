using System;
using System.IO;
using System.Text;
using SparqlForHumans.RDF.Extensions;
using VDS.RDF;
using SparqlForHumans.Utilities;
using VDS.RDF.Writing.Formatting;

namespace SparqlForHumans.RDF.Reordering
{
    public class TriplesReordering
    {
        private static readonly NLog.Logger Logger = SparqlForHumans.Logger.Logger.Init();

        //public static void Filter(string inputTriplesFilename, int triplesLimit)
        //{
        //    var outputTriplesFilename = FileHelper.GetFilteredOutputFilename(inputTriplesFilename, triplesLimit);
        //    Filter(inputTriplesFilename, outputTriplesFilename, triplesLimit);
        //}

        /// <summary>
        ///     Reads an Wikidata GZipped N-triples dump.
        ///     Foreach line in the Triples file:
        ///     - Parse the RDF triple
        ///     - For the following cases, skip:
        ///     - If (triplesLimit > 0) && (Subject Q-Code > triplesLimit)
        ///     - If (triplesLimit > 0) && (Object Q-Code > triplesLimit)
        ///     - If Language != EN
        ///     Else, Add the triple to a new NT File
        /// </summary>
        /// <param name="inputTriplesFilename">Wikidata GZipped N-triples dump</param>
        /// <param name="outputTriplesFilename">Filtered Wikidata (Non-GZipped) N-triples dump</param>
        /// <param name="triplesLimit">Limit of Q-Id to filter. Value of -1 means no filtering for Q-Id</param>
        public static void Reorder(string inputTriplesFilename, string outputTriplesFilename = "")
        {
            if (string.IsNullOrWhiteSpace(outputTriplesFilename))
                outputTriplesFilename = FileHelper.GetReorderedOutputFilename(inputTriplesFilename);

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

            using (var outputFileStream = File.Create(outputTriplesFilename))
            {
                Logger.Info("Read,Write");
                foreach (var line in wikidataDumpLines)
                {
                    readCount++;

                    if (readCount % notifyTicks == 0)
                        Logger.Info($"{readCount:N0};{writeCount:N0}");

                    try
                    {
                        var triple = line.ToTriple();
                        
                        if (!IsValidTriple(triple))
                            continue;

                        var newTriple = triple.ReorderTriple();
                        var newLine = newTriple.ToString(new NTriplesFormatter());

                        var data = Encoding.UTF8.GetBytes($"{newLine}{Environment.NewLine}");
                        outputFileStream.Write(data, 0, data.Length);
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
        }

        public static bool IsValidTriple(Triple triple)
        {
            //Object must be URI
            return triple.Object.IsUriNode();
        }
    }
}
