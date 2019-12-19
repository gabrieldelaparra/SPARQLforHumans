using SparqlForHumans.Lucene;
using SparqlForHumans.Lucene.Extensions;
using SparqlForHumans.Lucene.Index;
using SparqlForHumans.Lucene.Queries;
using SparqlForHumans.RDF.Filtering;
using SparqlForHumans.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Lucene.Net.Index;
using Lucene.Net.Store;
using NaturalSort.Extension;
using SparqlForHumans.Lucene.Queries.Graph;
using SparqlForHumans.RDF.FilterReorderSort;
using SparqlForHumans.RDF.Reordering;
using VDS.RDF;

namespace SparqlForHumans.CLI
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Options.InternUris = false;
            //CreatePropertiesHistogram();
            QueryForSomeZeroFrequencyProperties();
            //FilterReorderSortAll();
            //FilterReorderSort500();
            //CreateEntitiesIndex(@"C:\Users\admin\Desktop\DCC\SparqlforHumans\SparqlForHumans.CLI\bin\x64\Debug\netcoreapp2.1\filtered-All.Sorted.nt", true);
            //CreatePropertiesIndex(@"C:\Users\admin\Desktop\DCC\SparqlforHumans\SparqlForHumans.CLI\bin\x64\Debug\netcoreapp2.1\filtered-All.Sorted.nt", true);
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
            //QueryEntities("obam");
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

        public static void QueryForSomeZeroFrequencyProperties()
        {
            var query = new BatchIdEntityPropertiesQuery(LuceneDirectoryDefaults.EntityIndexPath, new []{"P10"}).Query();
        }

        public static void CreatePropertiesHistogram()
        {
            var list = new List<string>();
            using (var luceneDirectory = FSDirectory.Open(LuceneDirectoryDefaults.PropertyIndexPath))
            using (var luceneDirectoryReader = DirectoryReader.Open(luceneDirectory))
            {
                var docCount = luceneDirectoryReader.MaxDoc;
                for (var i = 0; i < docCount; i++)
                {
                    var doc = luceneDirectoryReader.Document(i);
                    var property = doc.MapProperty();
                    list.Add($"{property.Id},{property.Label.Replace(',', ' ')},{property.Rank},{property.Domain.Count},{property.Range.Count}");
                }
            }

            File.WriteAllLines("PropertyDomainRangeHistogram.csv", list.OrderBy(x=>x, StringComparer.OrdinalIgnoreCase.WithNaturalSort()));
        }

        public static void CreateEntitiesIndex(string filename, bool overwrite = false)
        {
            var entitiesOutputPath = LuceneDirectoryDefaults.EntityIndexPath;
            entitiesOutputPath.DeleteIfExists(overwrite);
            new EntitiesIndexer(filename, LuceneDirectoryDefaults.EntityIndexPath).Index();
        }
        public static void CreatePropertiesIndex(string filename, bool overwrite = false)
        {
            var propertyOutputPath = LuceneDirectoryDefaults.PropertyIndexPath;
            propertyOutputPath.DeleteIfExists(overwrite);
            new PropertiesIndexer(filename, propertyOutputPath).Index();
        }

        private static void Filter2MM()
        {
            var inputFilename = @"C:\Users\admin\Desktop\DCC\SparQLforHumans.Dataset\latest-truthy.nt.gz";
            var outputFilename = "filtered-All-2MM.nt";
            TriplesFilter.Filter(inputFilename, outputFilename, 2000000);
        }

        private static void Filter500k()
        {
            var inputFilename = @"filtered-All-2MM.nt";
            var outputFilename = "filtered-All-500k.nt";
            TriplesFilter.Filter(inputFilename, outputFilename, 500000);
        }

        private static void Filter5k()
        {
            var inputFilename = @"filtered-All-2MM.nt";
            var outputFilename = "filtered-All-5k.nt";
            TriplesFilter.Filter(inputFilename, outputFilename, 5000);
        }

        private static void Filter500()
        {
            var inputFilename = @"filtered-All-5k.nt";
            var outputFilename = "filtered-All-500.nt";
            TriplesFilter.Filter(inputFilename, outputFilename, 500);
        }

        private static void FilterAll()
        {
            var inputFilename = @"C:\Users\admin\Desktop\DCC\SparQLforHumans.Dataset\latest-truthy.nt.gz";
            var outputFilename = "filtered-All.nt";
            TriplesFilter.Filter(inputFilename, outputFilename, -1);
        }

        private static void ReorderAll()
        {
            var inputFilename = "filtered-All.nt";
            TriplesReordering.Reorder(inputFilename);
        }
        private static void FilterReorderSort500()
        {
            var inputFilename = @"EntityIndexMultipleInstanceReverse.nt";
            TriplesFilterReorderSort.FilterReorderSort(inputFilename);
        }


        private static void FilterReorderSortAll()
        {
            var inputFilename = @"C:\Users\admin\Desktop\DCC\SparqlforHumans\SparqlForHumans.UnitTests\Resources\QueryGraphPracticalResults1.nt";
            var output = @"C:\Users\admin\Desktop\DCC\SparqlforHumans\SparqlForHumans.UnitTests\Resources\QueryGraphPracticalResults1-Sorted.nt";
            TriplesFilterReorderSort.FilterReorderSort(inputFilename, output);
        }

        private static void QueryEntities(string query)
        {
            Console.WriteLine($"Query Entity: {query}\n");
            //var results = MultiDocumentQueries.QueryEntitiesByLabel(query).ToList();
            var results = new MultiLabelEntityQuery(LuceneDirectoryDefaults.EntityIndexPath, query).Query();
            MappingExtensions.AddProperties(results, LuceneDirectoryDefaults.EntityIndexPath);
            foreach (var result in results)
            {
                Console.WriteLine(result.ToRankedString());
                Console.WriteLine($"     Props: {string.Join(",", result.Properties.OrderBy(x => x.Rank).Select(x => $"{x.Id}:{x.Label}").Distinct())}");
            }
        }

        private static void QueryProperties(string query)
        {
            Console.WriteLine($"Query Property: {query}\n");
            var results = new MultiLabelPropertyQuery(LuceneDirectoryDefaults.PropertyIndexPath, query).Query();
            //var results = MultiDocumentQueries.QueryPropertiesByLabel(query);
            foreach (var result in results)
            {
                Console.WriteLine(result.ToRankedString());
                Console.WriteLine($"     Domains: {string.Join(",", result.Domain.Select(x => $"{x}").Distinct())}");
            }
        }
    }
}