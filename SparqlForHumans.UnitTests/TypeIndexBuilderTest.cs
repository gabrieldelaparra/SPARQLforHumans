using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Lucene.Net.Index;
using SparqlForHumans.Core.Properties;
using SparqlForHumans.Core.Services;
using SparqlForHumans.Core.Utilities;
using Xunit;

namespace SparqlForHumans.UnitTests
{
    public class TypeIndexBuilderTest
    {
        [Fact]
        public static void TestAddTypesFields()
        {
            const string filename = "Resources/EntityType.nt";
            const string outputPath = "TypeAddFolder";

            outputPath.DeleteIfExists();
            Assert.False(Directory.Exists(outputPath));

            var outputDirectory = outputPath.GetLuceneDirectory();

            IndexBuilder.CreateEntitiesIndex(filename, outputDirectory, true);

            var dictionary = IndexBuilder.CreateTypesAndPropertiesDictionary(outputDirectory);

            IndexBuilder.AddIsTypeEntityToEntitiesIndex(dictionary, outputDirectory);
            Assert.True(Directory.Exists(outputPath));

            using (var reader = IndexReader.Open(outputDirectory, true))
            {
                var obamaDocument = SingleDocumentQueries.QueryDocumentById("Q76", outputDirectory);
                var personDocument = SingleDocumentQueries.QueryDocumentById("Q5", outputDirectory);
                var countryDocument = SingleDocumentQueries.QueryDocumentById("Q17", outputDirectory);
                var chileDocument = SingleDocumentQueries.QueryDocumentById("Q298", outputDirectory);

                Assert.Equal("Q76", obamaDocument.GetValue(Labels.Id));
                Assert.Equal("Q5", personDocument.GetValue(Labels.Id));
                Assert.Equal("Q17", countryDocument.GetValue(Labels.Id));
                Assert.Equal("Q298", chileDocument.GetValue(Labels.Id));

                Assert.Equal("true", personDocument.GetValue(Labels.IsTypeEntity));
                Assert.Equal("true", countryDocument.GetValue(Labels.IsTypeEntity));

                Assert.Empty(obamaDocument.GetValue(Labels.IsTypeEntity));
                Assert.Empty(chileDocument.GetValue(Labels.IsTypeEntity));
            }

            outputPath.DeleteIfExists();
        }
    }
}
