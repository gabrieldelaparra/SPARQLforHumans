using SparqlForHumans.RDF.Extensions;
using SparqlForHumans.RDF.Models;
using System;
using System.Configuration;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using SparqlForHumans.Utilities;
using VDS.RDF;
using static SparqlForHumans.Utilities.FileHelper;

namespace SparqlForHumans.RDF.Filtering
{
    public class TriplesFilter
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
        public static void Filter(string inputTriplesFilename, string outputTriplesFilename = "", int triplesLimit = -1)
        {
            if (string.IsNullOrWhiteSpace(outputTriplesFilename))
                outputTriplesFilename = GetFilteredOutputFilename(inputTriplesFilename, triplesLimit);

            Options.InternUris = false;

            if (!new FileInfo(inputTriplesFilename).Exists)
                return;

            outputTriplesFilename.CreatePathIfNotExists();

            const int notifyTicks = 100000;
            long readCount = 0;
            long writeCount = 0;

            var wikidataDumpLines = GetInputLines(inputTriplesFilename);

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
                        if (!IsValidLine(line, triplesLimit))
                            continue;
                        //var triple = line.ToTriple();

                        //if (!IsValidTriple(triple, triplesLimit))
                        //    continue;

                        var data = Encoding.UTF8.GetBytes($"{line}{Environment.NewLine}");
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
                    var objectId = objct.Substring(subjectPrefix.Length, objct.Length - subjectPrefix.Length-1);
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
        //public static bool IsValidTriple(Triple triple, int entityLimit)
        //{
        //    if (triple == null)
        //        return false;

        //    var ntSubject = triple.Subject;
        //    var ntPredicate = triple.Predicate;
        //    var ntObject = triple.Object;

        //    //Subject is not URI
        //    if (!ntSubject.IsUriNode())
        //        return false;

        //    //Condition: Subject is not (Entity || Property): Skip
        //    if (!ntSubject.IsEntity())
        //        return false;

        //    //Condition: Subject is Q-Entity and Q > triplesLimit: Skip
        //    //Condition: Object is Q-Entity and Q > triplesLimit: Skip
        //    if (entityLimit > 0 && ntSubject.IsEntityQ() && ntSubject.GetIntId() > entityLimit)
        //        return false;

        //    if (entityLimit > 0 && ntObject.IsEntityQ() && ntObject.GetIntId() > entityLimit)
        //        return false;

        //    if (ntSubject.IsEntityP() && ntPredicate.IsProperty())
        //        return false;

        //    switch (ntPredicate.GetPredicateType())
        //    {
        //        case PredicateType.Other:
        //            return false;

        //        case PredicateType.Label:
        //        case PredicateType.Description:
        //        case PredicateType.AltLabel:
        //            if (!ntObject.IsLiteral())
        //                return false;
        //            //Condition: Object is Literal: Filter [@en, ...] only
        //            else if (!ntObject.IsValidLanguageLiteral())
        //                return false;
        //            break;
        //    }

        //    //Condition: Predicate is Property (e.g. Population or Date)
        //    //And Object is literal (not an URI node) (e.g. 100 or 1998)
        //    //This rule filters out Population, birthdate, and stuff
        //    //TODO: This will be removed in the future to add search values.
        //    if (ntPredicate.IsProperty() && !ntObject.IsEntity())
        //        return false;

        //    return true;
        //}
    }
}