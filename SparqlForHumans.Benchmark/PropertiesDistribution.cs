using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Lucene.Net.Index;
using Lucene.Net.Store;
using NaturalSort.Extension;
using SparqlForHumans.Lucene;
using SparqlForHumans.Lucene.Extensions;

namespace SparqlForHumans.Benchmark
{
    public static class PropertiesDistribution
    {
        public static void CreatePropertiesDistribution()
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

            File.WriteAllLines("PropertyDomainRangeHistogram.csv", list.OrderBy(x => x, StringComparer.OrdinalIgnoreCase.WithNaturalSort()));
        }
    }
}
