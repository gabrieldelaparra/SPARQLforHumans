using SparqlForHumans.Lucene;
using SparqlForHumans.Lucene.Extensions;
using SparqlForHumans.Lucene.Index;
using SparqlForHumans.Lucene.Queries;
using SparqlForHumans.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CommandLine;
using Lucene.Net.Index;
using Lucene.Net.Store;
using NaturalSort.Extension;
using NLog;
using NLog.Targets;
using SparqlForHumans.Logger;
using SparqlForHumans.Lucene.Queries.Fields;
using SparqlForHumans.Lucene.Queries.Graph;
using SparqlForHumans.RDF.FilterReorderSort;
using SparqlForHumans.RDF.Reordering;
using VDS.RDF;

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
            Logger.Info("Buckle up...");
            var fileTarget = (FileTarget)LogManager.Configuration.FindTargetByName("logfile");
            var logEventInfo = new LogEventInfo { TimeStamp = DateTime.Now };
            var logFilename = fileTarget.FileName.Render(logEventInfo);

            Parser.Default.ParseArguments<Options>(args)
                .WithParsed<Options>(o =>
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
                        Logger.Info($"Starting properties index...");
                        if (o.Overwrite)
                            propertiesOutputPath.DeleteIfExists();
                        new PropertiesIndexer(inputFilename, propertiesOutputPath, entitiesOutputPath).Index();
                        Logger.Info($"Properties index is done: {propertiesOutputPath}");
                    }
                });

            //Options.InternUris = false;
            //CreatePropertiesHistogram();
            //QueryForSomeZeroFrequencyProperties();
            //FilterReorderSortAll();
            //FilterReorderSort500();
            //CreateEntitiesIndex(@"C:\Users\admin\Desktop\DCC\SparqlforHumans\SparqlForHumans.CLI\bin\x64\Debug\netcoreapp2.1\filtered-All-PostFilter-Sorted.nt", true);
            //CreatePropertiesIndex(@"C:\Users\admin\Desktop\DCC\SparqlforHumans\SparqlForHumans.CLI\bin\x64\Debug\netcoreapp2.1\filtered-All-PostFilter-Sorted.nt.gz", true);
            //ReorderAll();
            //FilterAll();
            //Filter5k();
            //Filter500k();
            //CreateIndex("filtered-All-5k.nt", true);
            //var cli = new CommandLineInterface();
            //Filter500();
            ////Filter500k();
            //Filter2MM();
            //CreateIndex("filtered-All-5k.nt", true);
            //CreateIndex("filtered-All-2MM.nt", true);
            //CreateIndex("filtered-All-500k.nt", true);
            //CreateIndex(@"C:\Users\admin\Desktop\DCC\SparqlforHumans\SparqlForHumans.CLI\bin\Debug\netcoreapp2.1\filtered-All.nt", true);
            //QueryEntities("life");
            //QueryEntities("hum");
            //QueryEntities("person");
            //QueryEntities("city");
            //QueryEntities("michelle obama");
            //QueryProperties("educated");

            //Console.Read();
            //Console.WriteLine(dictionary.Count);

            //TestBuilderHelper.GetFirst20ObamaTriplesGroups();

            //Filter5k();
            //FilterAll();
            //Filter2MM();
            //CreateIndex2MM(true);
            //CreatePropertyIndex(true);
            //IndexBuilder.CreateTypesIndex();
        }

        //public static void QueryForSomeZeroFrequencyProperties()
        //{
        //    var query = new BatchIdEntityPropertiesQuery(LuceneDirectoryDefaults.EntityIndexPath, new []{"P10"}).Query();
        //}

        //public static void CreatePropertiesHistogram()
        //{
        //    var list = new List<string>();
        //    using (var luceneDirectory = FSDirectory.Open(LuceneDirectoryDefaults.PropertyIndexPath))
        //    using (var luceneDirectoryReader = DirectoryReader.Open(luceneDirectory))
        //    {
        //        var docCount = luceneDirectoryReader.MaxDoc;
        //        for (var i = 0; i < docCount; i++)
        //        {
        //            var doc = luceneDirectoryReader.Document(i);
        //            var property = doc.MapProperty();
        //            list.Add($"{property.Id},{property.Label.Replace(',', ' ')},{property.Rank},{property.Domain.Count},{property.Range.Count}");
        //        }
        //    }

        //    File.WriteAllLines("PropertyDomainRangeHistogram.csv", list.OrderBy(x=>x, StringComparer.OrdinalIgnoreCase.WithNaturalSort()));
        //}

        //public static void CreateEntitiesIndex(string filename, bool overwrite = false)
        //{
        //    var entitiesOutputPath = LuceneDirectoryDefaults.EntityIndexPath;
        //    entitiesOutputPath.DeleteIfExists(overwrite);
        //    new EntitiesIndexer(filename, entitiesOutputPath).Index();
        //}
        //public static void CreatePropertiesIndex(string filename, bool overwrite = false)
        //{
        //    var propertyOutputPath = LuceneDirectoryDefaults.PropertyIndexPath;
        //    propertyOutputPath.DeleteIfExists(overwrite);
        //    new PropertiesIndexer(filename, propertyOutputPath).Index();
        //}

        //private static void Filter2MM()
        //{
        //    var inputFilename = @"C:\Users\admin\Desktop\DCC\SparQLforHumans.Dataset\latest-truthy.nt.gz";
        //    var outputFilename = "filtered-All-2MM.nt";
        //    TriplesFilterReorderSort.FilterReorderSort(inputFilename, outputFilename, 2000000);
        //}

        //private static void Filter500k()
        //{
        //    var inputFilename = @"filtered-All-2MM.nt";
        //    var outputFilename = "filtered-All-500k.nt";
        //    TriplesFilterReorderSort.FilterReorderSort(inputFilename, outputFilename, 500000);
        //}

        //private static void Filter5k()
        //{
        //    var inputFilename = @"filtered-All-2MM.nt";
        //    var outputFilename = "filtered-All-5k.nt";
        //    TriplesFilterReorderSort.FilterReorderSort(inputFilename, outputFilename, 5000);
        //}

        //private static void Filter500()
        //{
        //    var inputFilename = @"filtered-All-5k.nt";
        //    var outputFilename = "filtered-All-500.nt";
        //    TriplesFilterReorderSort.FilterReorderSort(inputFilename, outputFilename, 500);
        //}

        //private static void FilterAll()
        //{
        //    var inputFilename = @"C:\Users\admin\Desktop\DCC\SparQLforHumans.Dataset\latest-truthy.nt.gz";
        //    var outputFilename = "filtered-All.nt.gz";
        //    TriplesFilterReorderSort.FilterReorderSort(inputFilename, outputFilename, -1);
        //}

        //private static void FilterReorderSort500()
        //{
        //    var inputFilename = @"EntityIndexMultipleInstanceReverse.nt";
        //    TriplesFilterReorderSort.FilterReorderSort(inputFilename);
        //}

        //private static void FilterReorderSortAll()
        //{
        //    var inputFilename = @"C:\Users\admin\Desktop\DCC\SparQLforHumans.Dataset\latest-truthy.nt.gz";
        //    var output = @"filtered-All-FilterReorder.nt.gz";
        //    TriplesFilterReorderSort.FilterReorderSort(inputFilename, output);
        //}
        //private static void ReorderAll()
        //{
        //    var inputFilename = "filtered-All.nt";
        //    TriplesReordering.Reorder(inputFilename);
        //}



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