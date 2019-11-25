using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using SparqlForHumans.RDF.Extensions;
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

                        if (!IsValidFilterTriple(triple, triplesLimit))
                            continue;

                        var data = Encoding.UTF8.GetBytes($"{line}{Environment.NewLine}");
                        outputFileStream.Write(data, 0, data.Length);
                        writeCount++;

                        if(!IsValidReorderTriple(triple))
                            continue;

                        var newTriple = triple.ReorderTriple();
                        var newLine = newTriple.ToString(new NTriplesFormatter());

                        data = Encoding.UTF8.GetBytes($"{newLine}{Environment.NewLine}");
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
            Logger.Info("Finished Filtering and reordering. Sorting via external sort. No debugging messages available.");

            var process = new System.Diagnostics.Process();
            var startInfo = new System.Diagnostics.ProcessStartInfo
            {
                WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden,
                FileName = @"C:\Program Files\Git\usr\bin\sort.exe",
                Arguments = $"{outputTriplesFilename} -g -o {outputTriplesFilename.GetSortOutputFilename()}"
            };
            process.StartInfo = startInfo;
            process.Start();

            Logger.Info("Finished sorting.");
        }

        public static bool IsValidFilterTriple(Triple triple, int entityLimit)
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
            if (entityLimit > 0 && ntSubject.IsEntityQ() && ntSubject.GetIntId() > entityLimit)
                return false;

            if (entityLimit > 0 && ntObject.IsEntityQ() && ntObject.GetIntId() > entityLimit)
                return false;

            if (ntSubject.IsEntityP() && ntPredicate.IsProperty())
                return false;

            switch (ntPredicate.GetPredicateType())
            {
                case PredicateType.Other:
                    return false;

                case PredicateType.AltLabel:
                case PredicateType.Description:
                case PredicateType.Label:
                    if (!ntObject.IsLiteral())
                        return false;
                    //Condition: Object is Literal: Filter [@en, ...] only
                    else if (!ntObject.IsValidLanguageLiteral())
                        return false;
                    break;
            }

            //Condition: Predicate is Property (e.g. Population or Date)
            //And Object is literal (not an URI node) (e.g. 100 or 1998)
            //This rule filters out Population, birthdate, and stuff
            if (ntPredicate.IsProperty() && !ntObject.IsEntity())
                return false;

            return true;
        }

        public static bool IsValidReorderTriple(Triple triple)
        {
            //Object must be URI
            return triple.Object.IsUriNode() && triple.Object.IsEntityQ();
        }

    }
}
