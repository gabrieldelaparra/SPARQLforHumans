using System.Linq;
using Lucene.Net.Store;
using SparqlForHumans.Lucene.Extensions;
using SparqlForHumans.Lucene.Indexing;
using SparqlForHumans.Lucene.Queries;
using SparqlForHumans.Models;
using SparqlForHumans.Models.LuceneIndex;
using SparqlForHumans.Utilities;
using Xunit;
using Directory = System.IO.Directory;

namespace SparqlForHumans.UnitTests
{
    public class TypeIndexBuilderTest
    {
        [Fact]
        public static void TestAddTypesFields()
        {
            const string filename = "Resources/TypeProperties.nt";
            const string outputPath = "TypeAddFolderEntityType";

            outputPath.DeleteIfExists();
            Assert.False(Directory.Exists(outputPath));

            using (var luceneIndexDirectory = FSDirectory.Open(outputPath.GetOrCreateDirectory()))
            {
                EntitiesIndex.CreateEntitiesIndex(filename, luceneIndexDirectory, true);

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
                EntitiesIndex.AddIsTypeEntityToEntitiesIndex(dictionary, luceneIndexDirectory);

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

        /// <summary>
        /// Given the following nTriples file:
        /// 
        /// Q76 (Obama) -> P31 (InstanceOf) -> Q5 (Human)
        /// Q76 (Obama) -> P27 -> Qxx
        /// Q76 (Obama) -> P555 -> Qxx
        /// ...
        /// Q77 (Other Human) -> P31 (InstanceOf) -> Q5 (Human)
        /// Q77 (Other Human) -> P33 -> Qxx
        /// Q77 (Other Human) -> P44 -> Qxx
        /// ...
        /// Q5 (Human)
        /// ...
        /// Q278 (Chile) -> P31 (InstanceOf) -> Q17 (Country)
        /// Q278 (Chile) -> P555 -> Qxx
        /// Q278 (Chile) -> P777 -> Qxx
        /// ...
        /// Q17 (Country)
        /// ...
        ///
        /// Should return a dictionary with the following:
        /// (Human) Q5 (Obama + OtherHuman): P31, P27, P555, P33, P44
        /// (Country) Q17 (Chile): P31, P555, P777
        /// </summary>
        [Fact]
        public static void TestCreateEntitiesTypesPropertiesDictionary()
        {
            const string filename = "Resources/TypeProperties.nt";
            const string outputPath = "TypeAddFolderProperties";

            outputPath.DeleteIfExists();
            Assert.False(Directory.Exists(outputPath));

            using (var luceneIndexDirectory = FSDirectory.Open(outputPath.GetOrCreateDirectory()))
            {
                EntitiesIndex.CreateEntitiesIndex(filename, luceneIndexDirectory, true);

                var dictionary = IndexBuilder.CreateTypesAndPropertiesDictionary(luceneIndexDirectory);

                Assert.Equal(2, dictionary.Count);

                Assert.Equal("Q5", dictionary.Keys.ElementAt(0));
                Assert.Equal("Q17", dictionary.Keys.ElementAt(1));

                //P27, P555, P33, P44, P31
                Assert.Equal(5, dictionary.ElementAt(0).Value.Count);

                //P555, P777, P31
                Assert.Equal(3, dictionary.ElementAt(1).Value.Count);
            }

            outputPath.DeleteIfExists();
        }

        /// <summary>
        /// The CreateTypesAndPropertiesDictionary creates the following dictionary:
        /// (Human) Q5 (Obama + OtherHuman): P31, P27, P555, P33, P44
        /// (Country) Q17 (Chile): P31, P555, P777
        ///
        /// The InvertDictionary dictionary should return the following:
        /// P31: Q5, Q17
        /// P27: Q5
        /// P555: Q5, Q17
        /// P33: Q5
        /// P44: Q5
        /// P777: Q17
        /// </summary>
        [Fact]
        public static void TestCreatePropertiesInvertedDictionary()
        {
            const string filename = "Resources/TypeProperties.nt";
            const string outputPath = "TypeAddFolderInvertedProperties";

            outputPath.DeleteIfExists();
            Assert.False(Directory.Exists(outputPath));

            using (var luceneIndexDirectory = FSDirectory.Open(outputPath.GetOrCreateDirectory()))
            {
                EntitiesIndex.CreateEntitiesIndex(filename, luceneIndexDirectory, true);

                var typesDictionary = IndexBuilder.CreateTypesAndPropertiesDictionary(luceneIndexDirectory);
                var propertiesDictionary = typesDictionary.InvertDictionary();

                Assert.Equal(6, propertiesDictionary.Count);

                Assert.Equal("P31", propertiesDictionary.Keys.ElementAt(0));
                Assert.Equal("P27", propertiesDictionary.Keys.ElementAt(1));
                Assert.Equal("P555", propertiesDictionary.Keys.ElementAt(2));
                Assert.Equal("P33", propertiesDictionary.Keys.ElementAt(3));
                Assert.Equal("P44", propertiesDictionary.Keys.ElementAt(4));
                Assert.Equal("P777", propertiesDictionary.Keys.ElementAt(5));

                //P27>Q30 (Q5)
                Assert.Single(propertiesDictionary.ElementAt(1).Value);
                Assert.Equal("Q5", propertiesDictionary.ElementAt(1).Value.ElementAt(0));

                //P555>Q555 (Q17)
                Assert.Equal(2, propertiesDictionary.ElementAt(2).Value.Count);
                Assert.Equal("Q5", propertiesDictionary.ElementAt(2).Value.ElementAt(0));
                Assert.Equal("Q17", propertiesDictionary.ElementAt(2).Value.ElementAt(1));

                //P777>Q777 (Q17)
                Assert.Single(propertiesDictionary.ElementAt(5).Value);
                Assert.Equal("Q17", propertiesDictionary.ElementAt(5).Value.ElementAt(0));
            }

            outputPath.DeleteIfExists();
        }

        [Fact]
        public static void TestQueryTypesFields()
        {
            const string filename = "Resources/TypeProperties.nt";
            const string outputPath = "TypeAddFolderTypesFields";

            outputPath.DeleteIfExists();
            Assert.False(Directory.Exists(outputPath));

            using (var luceneIndexDirectory = FSDirectory.Open(outputPath.GetOrCreateDirectory()))
            {
                EntitiesIndex.CreateEntitiesIndex(filename, luceneIndexDirectory, true);
                var dictionary = IndexBuilder.CreateTypesAndPropertiesDictionary(luceneIndexDirectory);

                EntitiesIndex.AddIsTypeEntityToEntitiesIndex(dictionary, luceneIndexDirectory);

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