using SparqlForHumans.Lucene;
using SparqlForHumans.Lucene.Index;
using SparqlForHumans.Utilities;
using System;
using CommandLine;
using NLog;
using NLog.Targets;
using SparqlForHumans.RDF.FilterReorderSort;
using Directory = System.IO.Directory;

namespace SparqlForHumans.CLI
{
    public class Program
    {
        private static readonly NLog.Logger Logger = SparqlForHumans.Logger.Logger.Init();

        private class Options
        {
            [Option('f', "filter",
                Required = false,
                Default = false,
                HelpText = "Filter and sort an input file.")]
            public bool Filter { get; set; }

            [Option('e', "index-entities",
                Required = false,
                Default = false,
                HelpText = "Index entities for a preprocessed input file.")]
            public bool IndexEntities { get; set; }

            [Option('p', "index-properties",
                Required = false,
                Default = false,
                HelpText = "Index properties for a preprocessed input file.")]
            public bool IndexProperties { get; set; }

            [Option('i', "input",
                Required = true,
                HelpText = "Input file to be processed.")]
            public string InputFilename { get; set; }

            [Option('o', "output",
                Required = false,
                HelpText = "Display output paths.")]
            public bool DisplayOutputPathsOnly { get; set; }

            [Option('l', "limit",
                Required = false,
                Default = -1,
                HelpText = "Limit of Q values. For testing purposes.")]
            public int Limit { get; set; }

            [Option('o', "overwrite",
                Required = false,
                Default = true,
                HelpText = "Overwrite existing index contents.")]
            public bool Overwrite { get; set; }
        }

        public static void Main(string[] args)
        {
            Logger.Debug("SparqlForHumans");
            var fileTarget = (FileTarget)LogManager.Configuration.FindTargetByName("logfile");
            var logEventInfo = new LogEventInfo { TimeStamp = DateTime.Now };
            var logFilename = fileTarget.FileName.Render(logEventInfo);

            Parser.Default.ParseArguments<Options>(args)
                .WithParsed(o =>
                {
                    Logger.Info($"Running with the following arguments: {string.Join(',', args)}");
                    Logger.Info($"DisplayOutputPathsOnly: {o.DisplayOutputPathsOnly}");
                    Logger.Info($"Filter: {o.Filter}");
                    Logger.Info($"IndexEntities: {o.IndexEntities}");
                    Logger.Info($"IndexProperties: {o.IndexProperties}");
                    Logger.Info($"InputFilename: {o.InputFilename}");
                    Logger.Info($"Limit: {o.Limit}");
                    Logger.Info($"Overwrite: {o.Overwrite}");
                    Logger.Info($"Logs output: {logFilename}");

                    var entitiesOutputPath = LuceneDirectoryDefaults.EntityIndexPath;
                    var propertiesOutputPath = LuceneDirectoryDefaults.PropertyIndexPath;
                    var outputPreprocessedFile = FileHelper.GetFilteredOutputFilename(o.InputFilename, o.Limit);
                    var inputFilename = o.Filter ? outputPreprocessedFile : o.InputFilename;

                    Logger.Info($"Entities Output Path: {entitiesOutputPath}");
                    Logger.Info($"Properties Output Path: {propertiesOutputPath}");
                    Logger.Info($"Filtered Output filename (when Filter option): {outputPreprocessedFile}");
                    Logger.Info($"Index Input filename (Filter ? out : args.In): {inputFilename}");

                    if (o.Filter)
                    {
                        TriplesFilterReorderSort.FilterReorderSort(o.InputFilename, outputPreprocessedFile, o.Limit);
                    }

                    if (o.IndexEntities)
                    {
                        Logger.Info($"Starting entities index...");
                        if (o.Overwrite)
                            entitiesOutputPath.DeleteIfExists();
                        new EntitiesIndexer(inputFilename, entitiesOutputPath).Index();
                        Logger.Info($"Entities index is done: {entitiesOutputPath}");
                    }

                    if (o.IndexProperties)
                    {
                        if (!Directory.Exists(entitiesOutputPath)) {
                            Logger.Info($"Please build entitites index first"); 
                            return;
                        }
                        Logger.Info($"Starting properties index...");
                        if (o.Overwrite)
                            propertiesOutputPath.DeleteIfExists();
                        new PropertiesIndexer(inputFilename, propertiesOutputPath, entitiesOutputPath).Index();
                        Logger.Info($"Properties index is done: {propertiesOutputPath}");
                    }
                });
        }

        //private static void QueryEntities(string query)
        //{
        //    Console.WriteLine($"Query Entity: {query}\n");
        //    //var results = MultiDocumentQueries.QueryEntitiesByLabel(query).ToList();
        //    var results = new MultiLabelEntityQuery(LuceneDirectoryDefaults.EntityIndexPath, query).Query();
        //    MappingExtensions.AddProperties(results, LuceneDirectoryDefaults.EntityIndexPath);
        //    foreach (var result in results)
        //    {
        //        Console.WriteLine(result.ToRankedString());
        //        Console.WriteLine($"     Props: {string.Join(",", result.Properties.OrderBy(x => x.Rank).Select(x => $"{x.Id}:{x.Label}").Distinct())}");
        //    }
        //}

        //private static void QueryProperties(string query)
        //{
        //    Console.WriteLine($"Query Property: {query}\n");
        //    var results = new MultiLabelPropertyQuery(LuceneDirectoryDefaults.PropertyIndexPath, query).Query();
        //    //var results = MultiDocumentQueries.QueryPropertiesByLabel(query);
        //    foreach (var result in results)
        //    {
        //        Console.WriteLine(result.ToRankedString());
        //        Console.WriteLine($"     Domains: {string.Join(",", result.Domain.Select(x => $"{x}").Distinct())}");
        //    }
        //}

    }
}