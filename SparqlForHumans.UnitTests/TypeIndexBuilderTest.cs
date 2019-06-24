using System.Linq;
using Lucene.Net.Store;
using SparqlForHumans.Lucene.Extensions;
using SparqlForHumans.Lucene.Indexing;
using SparqlForHumans.Lucene.Indexing.Indexer;
using SparqlForHumans.Lucene.Queries;
using SparqlForHumans.Models.LuceneIndex;
using SparqlForHumans.Utilities;
using Xunit;
using Directory = System.IO.Directory;

namespace SparqlForHumans.UnitTests
{
    public class TypeIndexBuilderTest
    {
        /// <summary>
        ///     Given the following nTriples file:
        ///     Q76 (Obama) -> P31 (InstanceOf) -> Q5 (Human)
        ///     Q76 (Obama) -> P27 -> Qxx
        ///     Q76 (Obama) -> P555 -> Qxx
        ///     ...
        ///     Q77 (Other Human) -> P31 (InstanceOf) -> Q5 (Human)
        ///     Q77 (Other Human) -> P33 -> Qxx
        ///     Q77 (Other Human) -> P44 -> Qxx
        ///     ...
        ///     Q5 (Human)
        ///     ...
        ///     Q278 (Chile) -> P31 (InstanceOf) -> Q17 (Country)
        ///     Q278 (Chile) -> P555 -> Qxx
        ///     Q278 (Chile) -> P777 -> Qxx
        ///     ...
        ///     Q17 (Country)
        ///     ...
        ///     Should return a dictionary with the following:
        ///     (Human) Q5 (Obama + OtherHuman): P31, P27, P555, P33, P44
        ///     (Country) Q17 (Chile): P31, P555, P777
        /// </summary>
        [Fact]
        public static void TestCreateEntitiesTypesPropertiesDictionary()
        {
            const string filename = "Resources/TypeProperties.nt";
            const string outputPath = "TypeAddFolderProperties";

            outputPath.DeleteIfExists();
            Assert.False(Directory.Exists(outputPath));

            Assert.False(true);
            new EntitiesIndexer(filename, outputPath).Index();
            using (var luceneIndexDirectory = FSDirectory.Open(outputPath.GetOrCreateDirectory()))
            {
                //var dictionary = IndexBuilder.CreateTypesAndPropertiesDictionary(luceneIndexDirectory);

                //Assert.Equal(2, dictionary.Count);

                //Assert.Equal(5, dictionary.Keys.ElementAt(0));
                //Assert.Equal(17, dictionary.Keys.ElementAt(1));

                ////P27, P555, P33, P44
                //Assert.Equal(4, dictionary.ElementAt(0).Value.Length);

                ////P555, P777
                //Assert.Equal(2, dictionary.ElementAt(1).Value.Length);
            }

            outputPath.DeleteIfExists();
        }

        /// <summary>
        ///     The CreateTypesAndPropertiesDictionary creates the following dictionary:
        ///     (Human) Q5 (Obama + OtherHuman): P31, P27, P555, P33, P44
        ///     (Country) Q17 (Chile): P31, P555, P777
        ///     The InvertDictionary dictionary should return the following:
        ///     P31: Q5, Q17
        ///     P27: Q5
        ///     P555: Q5, Q17
        ///     P33: Q5
        ///     P44: Q5
        ///     P777: Q17
        /// </summary>
        [Fact]
        public static void TestCreatePropertiesInvertedDictionary()
        {
            const string filename = "Resources/TypeProperties.nt";
            const string outputPath = "TypeAddFolderInvertedProperties";

            outputPath.DeleteIfExists();
            Assert.False(Directory.Exists(outputPath));

            Assert.False(true);
            var entitiesIndexer = new EntitiesIndexer(filename, outputPath);
            using (var luceneIndexDirectory = FSDirectory.Open(outputPath.GetOrCreateDirectory()))
            {
                //var typesDictionary = IndexBuilder.CreateTypesAndPropertiesDictionary(luceneIndexDirectory);
                //var propertiesDictionary = typesDictionary.InvertDictionary();

                //Assert.Equal(5, propertiesDictionary.Count);

                //Assert.Equal(27, propertiesDictionary.Keys.ElementAt(0));
                //Assert.Equal(555, propertiesDictionary.Keys.ElementAt(1));
                //Assert.Equal(33, propertiesDictionary.Keys.ElementAt(2));
                //Assert.Equal(44, propertiesDictionary.Keys.ElementAt(3));
                //Assert.Equal(777, propertiesDictionary.Keys.ElementAt(4));

                ////P27>Q30 (Q5)
                //Assert.Single(propertiesDictionary.ElementAt(0).Value);
                //Assert.Equal(5, propertiesDictionary.ElementAt(0).Value.ElementAt(0));

                ////P555>Q555 (Q17)
                //Assert.Equal(2, propertiesDictionary.ElementAt(1).Value.Length);
                //Assert.Equal(5, propertiesDictionary.ElementAt(1).Value.ElementAt(0));
                //Assert.Equal(17, propertiesDictionary.ElementAt(1).Value.ElementAt(1));

                ////P777>Q777 (Q17)
                //Assert.Single(propertiesDictionary.ElementAt(4).Value);
                //Assert.Equal(17, propertiesDictionary.ElementAt(4).Value.ElementAt(0));
            }

            outputPath.DeleteIfExists();
        }

        [Fact]
        public static void TestQueryIsTypeFields()
        {
            const string filename = "Resources/TypeProperties.nt";
            const string outputPath = "CreateIndexIsTypeFields";

            outputPath.DeleteIfExists();
            Assert.False(Directory.Exists(outputPath));

            new EntitiesIndexer(filename, outputPath).Index();
            using (var luceneIndexDirectory = FSDirectory.Open(outputPath.GetOrCreateDirectory()))
            {
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