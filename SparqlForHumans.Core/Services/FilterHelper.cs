using SparqlForHumans.Core.Utilities;
using System;
using System.Diagnostics;
using System.IO;
using VDS.RDF;

namespace SparqlForHumans.Core.Services
{
    public class FilterHelper
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public static void FilterTriples(string inputTriplesFilename, int triplesLimit)
        {
            var outputTriplesFilename = FileHelper.GetFilteredOutputFilename(inputTriplesFilename, triplesLimit);
            FilterTriples(inputTriplesFilename, outputTriplesFilename, triplesLimit);
        }

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
        /// <param name="inputTriplesFilename">Wikidata GZipped N-triples dump</param>
        /// <param name="outputTriplesFilename">Filtered Wikidata (Non-GZipped) N-triples dump</param>
        public static void FilterTriples(string inputTriplesFilename, string outputTriplesFilename, int triplesLimit)
        {

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            Options.InternUris = false;

            if (!new FileInfo(inputTriplesFilename).Exists) return;

            var outputFileInfo = new FileInfo(outputTriplesFilename);
            if (outputFileInfo.Directory != null && !outputFileInfo.Directory.Exists)
                outputFileInfo.Directory.Create();

            var notifyTicks = 100000;
            var readCount = 0;
            var writeCount = 0;

            var wikidataDumpLines = FileHelper.GetInputLines(inputTriplesFilename);

            using (var filteredStreamWriter = new StreamWriter(new FileStream(outputTriplesFilename, FileMode.Create)))
            {
                Logger.Debug("ElapsedTime,Read,Write");

                foreach (var line in wikidataDumpLines)
                {
                    readCount++;
                    if (readCount % notifyTicks == 0)
                        Logger.Debug($"{stopwatch.ElapsedMilliseconds},{readCount},{writeCount}");

                    try
                    {
                        var triple = line.GetTriple();

                        if (!IsValidTriple(triple, triplesLimit))
                            continue;

                        filteredStreamWriter.WriteLine(line);
                        writeCount++;
                    }
                    catch (Exception e)
                    {
                        Logger.Debug($"{stopwatch.ElapsedMilliseconds},{readCount},{line}");
                        Logger.Error(e);
                    }
                }
                Logger.Debug($"{stopwatch.ElapsedMilliseconds},{readCount},{writeCount}");
            }
            stopwatch.Stop();
        }

        public static bool IsValidTriple(Triple triple, int entityLimit)
        {
            var ntSubject = triple.Subject;
            var ntPredicate = triple.Predicate;
            var ntObject = triple.Object;

            if (!ntSubject.IsUriNode())
                return false;

            //Condition: Subject is not (Entity || Property): Skip
            if (!ntSubject.IsValidSubject())
                return false;

            //Condition: Subject is Entity and Q > triplesLimit: Skip
            if (ntSubject.IsEntity() && ntSubject.EntityQCode() > entityLimit)
                return false;

            //Condition: Object is Entity and Q > triplesLimit: Skip
            if (ntObject.IsEntity() && ntSubject.EntityQCode() > entityLimit)
                return false;

            //Condition: Object is Literal: Filter @en only
            if (!ntObject.IsValidLanguageLiteral())
                return false;

            return true;
        }

    }
}
