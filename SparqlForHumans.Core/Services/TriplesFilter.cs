using System;
using System.IO;
using System.Reflection.Metadata.Ecma335;
using NLog;
using SparqlForHumans.Core.Utilities;
using VDS.RDF;

namespace SparqlForHumans.Core.Services
{
    public class TriplesFilter
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

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
        ///     - If Subject Q-Code > 2mill
        ///     - If Object Q-Code > 2mill
        ///     - If Language != EN
        ///     Else, Add the triple to a new NT File
        /// </summary>
        /// <param name="inputTriplesFilename">Wikidata GZipped N-triples dump</param>
        /// <param name="outputTriplesFilename">Filtered Wikidata (Non-GZipped) N-triples dump</param>
        public static void Filter(string inputTriplesFilename, string outputTriplesFilename, int triplesLimit)
        {
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

            using (var filteredStreamWriter = new StreamWriter(new FileStream(outputTriplesFilename, FileMode.Create)))
            {
                Logger.Info("Read,Write");

                foreach (var line in wikidataDumpLines)
                {
                    readCount++;

                    if (readCount % notifyTicks == 0)
                        Logger.Info($"{readCount},{writeCount}");

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
                        Logger.Error($"{readCount},{line}");
                        Logger.Error(e);
                    }
                }

                Logger.Info($"{readCount},{writeCount}");
            }
        }

        public static bool IsValidTriple(Triple triple, int entityLimit)
        {
            var ntSubject = triple.Subject;
            var ntPredicate = triple.Predicate;
            var ntObject = triple.Object;

            //Subject is not URI
            if (!ntSubject.IsUriNode())
                return false;

            //Condition: Subject is not (Entity || Property): Skip
            if (!ntSubject.IsEntity())
                return false;

            //Condition: Subject is Q-Entity and Q > triplesLimit: Skip
            //Condition: Object is Q-Entity and Q > triplesLimit: Skip
            if (ntSubject.IsEntityQ() && ntSubject.GetIntId() > entityLimit)
                return false;

            if (ntObject.IsEntityQ() && ntObject.GetIntId() > entityLimit)
                return false;

            if (ntSubject.IsEntityP() && ntPredicate.IsProperty())
                return false;

            switch (ntPredicate.GetPredicateType())
            {
                case RDFExtensions.PredicateType.Other:
                    return false;

                case RDFExtensions.PredicateType.Label:
                case RDFExtensions.PredicateType.Description:
                case RDFExtensions.PredicateType.AltLabel:
                    if (!ntObject.IsLiteral())
                        return false;
                    //Condition: Object is Literal: Filter @en only
                    else if (!ntObject.IsValidLanguageLiteral())
                        return false;
                    break;
            }

            //Condition: Predicate is Property (e.g. Population or Date)
            //And Object is literal (not an URI node) (e.g. 100 or 1998)
            //This rule filters out Population, birthdate, and stuff
            //TODO: This will be removed in the future to add search values.
            if (ntPredicate.IsProperty() && !ntObject.IsEntity())
                return false;

            return true;
        }
    }
}