using System.Linq;
using Lucene.Net.Store;
using SparqlForHumans.Core.Services;
using SparqlForHumans.Core.Utilities;
using SparqlForHumans.Models;
using Xunit;
using Directory = System.IO.Directory;

namespace SparqlForHumans.UnitTests
{
    public class TypeIndexBuilderTest
    {
        [Fact]
        public static void TestAddTypesFields()
        {
            const string filename = "Resources/EntityType.nt";
            const string outputPath = "TypeAddFolderEntityType";

            outputPath.DeleteIfExists();
            Assert.False(Directory.Exists(outputPath));

            using (var luceneIndexDirectory = FSDirectory.Open(outputPath.GetOrCreateDirectory()))
            {
                IndexBuilder.CreateEntitiesIndex(filename, luceneIndexDirectory, true);

                var obamaDocument = SingleDocumentQueries.QueryDocumentById("Q76", luceneIndexDirectory);
                var personDocument = SingleDocumentQueries.QueryDocumentById("Q5", luceneIndexDirectory);
                var countryDocument = SingleDocumentQueries.QueryDocumentById("Q17", luceneIndexDirectory);
                var chileDocument = SingleDocumentQueries.QueryDocumentById("Q298", luceneIndexDirectory);

                Assert.Equal("Q76", obamaDocument.GetValue(Labels.Id));
                Assert.Equal("Q5", personDocument.GetValue(Labels.Id));
                Assert.Equal("Q17", countryDocument.GetValue(Labels.Id));
                Assert.Equal("Q298", chileDocument.GetValue(Labels.Id));
            }

            using (var luceneIndexDirectory = FSDirectory.Open(outputPath.GetOrCreateDirectory()))
            {
                var dictionary = IndexBuilder.CreateTypesAndPropertiesDictionary(luceneIndexDirectory);
                IndexBuilder.AddIsTypeEntityToEntitiesIndex(dictionary, luceneIndexDirectory);

                Assert.True(Directory.Exists(outputPath));

                var obamaDocument = SingleDocumentQueries.QueryDocumentById("Q76", luceneIndexDirectory);
                var personDocument = SingleDocumentQueries.QueryDocumentById("Q5", luceneIndexDirectory);
                var countryDocument = SingleDocumentQueries.QueryDocumentById("Q17", luceneIndexDirectory);
                var chileDocument = SingleDocumentQueries.QueryDocumentById("Q298", luceneIndexDirectory);

                Assert.Equal("Q76", obamaDocument.GetValue(Labels.Id));
                Assert.Equal("Q298", chileDocument.GetValue(Labels.Id));
                Assert.Equal("Q5", personDocument.GetValue(Labels.Id));
                Assert.Equal("Q17", countryDocument.GetValue(Labels.Id));

                Assert.Empty(obamaDocument.GetValue(Labels.IsTypeEntity));
                Assert.Empty(chileDocument.GetValue(Labels.IsTypeEntity));
                Assert.Equal("true", personDocument.GetValue(Labels.IsTypeEntity));
                Assert.Equal("true", countryDocument.GetValue(Labels.IsTypeEntity));
            }

            outputPath.DeleteIfExists();
        }

        [Fact]
        public static void TestCreateEntitiesTypesPropertiesDictionary()
        {
            const string filename = "Resources/EntityType.nt";
            const string outputPath = "TypeAddFolderProperties";

            outputPath.DeleteIfExists();
            Assert.False(Directory.Exists(outputPath));

            using (var luceneIndexDirectory = FSDirectory.Open(outputPath.GetOrCreateDirectory()))
            {
                IndexBuilder.CreateEntitiesIndex(filename, luceneIndexDirectory, true);

                var dictionary = IndexBuilder.CreateTypesAndPropertiesDictionary(luceneIndexDirectory);

                Assert.Equal(2, dictionary.Count);

                Assert.Equal("Q5", dictionary.Keys.ElementAt(0));
                Assert.Equal("Q17", dictionary.Keys.ElementAt(1));

                //P27, P31
                Assert.Equal(3, dictionary.ElementAt(0).Value.Count);

                //P555, P777, P31
                Assert.Equal(3, dictionary.ElementAt(1).Value.Count);
            }

            outputPath.DeleteIfExists();
        }

        [Fact]
        public static void TestCreatePropertiesInvertedDictionary()
        {
            const string filename = "Resources/EntityType.nt";
            const string outputPath = "TypeAddFolderInvertedProperties";

            outputPath.DeleteIfExists();
            Assert.False(Directory.Exists(outputPath));

            using (var luceneIndexDirectory = FSDirectory.Open(outputPath.GetOrCreateDirectory()))
            {
                IndexBuilder.CreateEntitiesIndex(filename, luceneIndexDirectory, true);

                var typesDictionary = IndexBuilder.CreateTypesAndPropertiesDictionary(luceneIndexDirectory);
                var propertiesDictionary = IndexBuilder.CreateInvertedProperties(typesDictionary);

                Assert.Equal(4, propertiesDictionary.Count);

                Assert.Equal("P31", propertiesDictionary.Keys.ElementAt(0));
                Assert.Equal("P27", propertiesDictionary.Keys.ElementAt(1));
                Assert.Equal("P555", propertiesDictionary.Keys.ElementAt(2));
                Assert.Equal("P777", propertiesDictionary.Keys.ElementAt(3));

                //P27>Q30 (Q5)
                Assert.Single(propertiesDictionary.ElementAt(1).Value);
                Assert.Equal("Q5", propertiesDictionary.ElementAt(1).Value.ElementAt(0));

                //P555>Q555 (Q17)
                Assert.Equal(2, propertiesDictionary.ElementAt(2).Value.Count);
                Assert.Equal("Q5", propertiesDictionary.ElementAt(2).Value.ElementAt(0));
                Assert.Equal("Q17", propertiesDictionary.ElementAt(2).Value.ElementAt(1));

                //P777>Q777 (Q17)
                Assert.Single(propertiesDictionary.ElementAt(3).Value);
                Assert.Equal("Q17", propertiesDictionary.ElementAt(3).Value.ElementAt(0));
            }

            outputPath.DeleteIfExists();
        }

        [Fact]
        public static void TestQueryTypesFields()
        {
            const string filename = "Resources/EntityType.nt";
            const string outputPath = "TypeAddFolderTypesFields";

            outputPath.DeleteIfExists();
            Assert.False(Directory.Exists(outputPath));

            using (var luceneIndexDirectory = FSDirectory.Open(outputPath.GetOrCreateDirectory()))
            {
                IndexBuilder.CreateEntitiesIndex(filename, luceneIndexDirectory, true);
                var dictionary = IndexBuilder.CreateTypesAndPropertiesDictionary(luceneIndexDirectory);

                IndexBuilder.AddIsTypeEntityToEntitiesIndex(dictionary, luceneIndexDirectory);

                var query = "chile";
                var types = MultiDocumentQueries.QueryEntitiesByLabel(query, luceneIndexDirectory, true);
                var all = MultiDocumentQueries.QueryEntitiesByLabel(query, luceneIndexDirectory, false);

                Assert.Empty(types);
                Assert.Single(all);

                query = "country";
                types = MultiDocumentQueries.QueryEntitiesByLabel(query, luceneIndexDirectory, true);
                all = MultiDocumentQueries.QueryEntitiesByLabel(query, luceneIndexDirectory, false);

                Assert.Single(types);
                Assert.Single(all);
            }

            outputPath.DeleteIfExists();
        }
    }
}