using System.IO;
using System.Linq;
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
        public static void TestCreatePropertiesInvertedDictionary()
        {
            const string filename = "Resources/EntityType.nt";
            const string outputPath = "TypeAddFolder";

            outputPath.DeleteIfExists();
            Assert.False(Directory.Exists(outputPath));

            var outputDirectory = outputPath.GetLuceneDirectory();

            IndexBuilder.CreateEntitiesIndex(filename, outputDirectory, true);

            var typesDictionary = IndexBuilder.CreateTypesAndPropertiesDictionary(outputDirectory);
            var propertiesDictionary = IndexBuilder.CreateInvertedProperties(typesDictionary);

            Assert.Equal(3, propertiesDictionary.Count);

            Assert.Equal("P27", propertiesDictionary.Keys.ElementAt(0));
            Assert.Equal("P555", propertiesDictionary.Keys.ElementAt(1));
            Assert.Equal("P777", propertiesDictionary.Keys.ElementAt(2));

            //P27>Q30 (Q5)
            Assert.Single(propertiesDictionary.ElementAt(0).Value);
            Assert.Equal("Q5", propertiesDictionary.ElementAt(0).Value.ElementAt(0));

            //P555>Q555 (Q17)
            Assert.Equal(2, propertiesDictionary.ElementAt(1).Value.Count);
            Assert.Equal("Q5", propertiesDictionary.ElementAt(1).Value.ElementAt(0));
            Assert.Equal("Q17", propertiesDictionary.ElementAt(1).Value.ElementAt(1));

            //P777>Q777 (Q17)
            Assert.Single(propertiesDictionary.ElementAt(2).Value);
            Assert.Equal("Q17", propertiesDictionary.ElementAt(2).Value.ElementAt(0));

            outputPath.DeleteIfExists();
        }

        [Fact]
        public static void TestQueryTypesFields()
        {
            const string filename = "Resources/EntityType.nt";
            const string outputPath = "TypeAddFolder";

            outputPath.DeleteIfExists();
            Assert.False(Directory.Exists(outputPath));

            var outputDirectory = outputPath.GetLuceneDirectory();

            IndexBuilder.CreateEntitiesIndex(filename, outputDirectory, true);

            var dictionary = IndexBuilder.CreateTypesAndPropertiesDictionary(outputDirectory);

            IndexBuilder.AddIsTypeEntityToEntitiesIndex(dictionary, outputDirectory);

            var query = "chile";
            var types = MultiDocumentQueries.QueryEntitiesByLabel(query, outputDirectory, true);
            var all = MultiDocumentQueries.QueryEntitiesByLabel(query, outputDirectory, false);

            Assert.Empty(types);
            Assert.Single(all);

            query = "country";
            types = MultiDocumentQueries.QueryEntitiesByLabel(query, outputDirectory, true);
            all = MultiDocumentQueries.QueryEntitiesByLabel(query, outputDirectory, false);

            Assert.Single(types);
            Assert.Single(all);

            outputPath.DeleteIfExists();
        }

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

            var obamaDocument = SingleDocumentQueries.QueryDocumentById("Q76", outputDirectory);
            var personDocument = SingleDocumentQueries.QueryDocumentById("Q5", outputDirectory);
            var countryDocument = SingleDocumentQueries.QueryDocumentById("Q17", outputDirectory);
            var chileDocument = SingleDocumentQueries.QueryDocumentById("Q298", outputDirectory);

            Assert.Equal("Q76", obamaDocument.GetValue(Labels.Id));
            Assert.Equal("Q5", personDocument.GetValue(Labels.Id));
            Assert.Equal("Q17", countryDocument.GetValue(Labels.Id));
            Assert.Equal("Q298", chileDocument.GetValue(Labels.Id));

            Assert.Empty(obamaDocument.GetValue(Labels.IsTypeEntity));
            Assert.Equal("true", personDocument.GetValue(Labels.IsTypeEntity));
            Assert.Empty(chileDocument.GetValue(Labels.IsTypeEntity));
            Assert.Equal("true", countryDocument.GetValue(Labels.IsTypeEntity));

            outputPath.DeleteIfExists();
        }

        [Fact]
        public static void TestCreateEntitiesTypesPropertiesDictionary()
        {
            const string filename = "Resources/EntityType.nt";
            const string outputPath = "TypeAddFolder";

            outputPath.DeleteIfExists();
            Assert.False(Directory.Exists(outputPath));

            var outputDirectory = outputPath.GetLuceneDirectory();

            IndexBuilder.CreateEntitiesIndex(filename, outputDirectory, true);

            var dictionary = IndexBuilder.CreateTypesAndPropertiesDictionary(outputDirectory);

            Assert.Equal(2, dictionary.Count);

            Assert.Equal("Q5", dictionary.Keys.ElementAt(0));
            Assert.Equal("Q17", dictionary.Keys.ElementAt(1));

            //P27>Q30, P555>Q556
            Assert.Equal(2, dictionary.ElementAt(0).Value.Count);

            //P555>Q555, P777>Q777
            Assert.Equal(2, dictionary.ElementAt(1).Value.Count);

            outputPath.DeleteIfExists();
        }
    }
}
