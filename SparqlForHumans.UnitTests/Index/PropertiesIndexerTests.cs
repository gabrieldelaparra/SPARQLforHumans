using System.Linq;
using Lucene.Net.Index;
using Lucene.Net.Store;
using SparqlForHumans.Lucene.Extensions;
using SparqlForHumans.Lucene.Index;
using SparqlForHumans.Lucene.Queries;
using SparqlForHumans.Models.LuceneIndex;
using SparqlForHumans.Utilities;
using Xunit;
using Directory = System.IO.Directory;

namespace SparqlForHumans.UnitTests.Index
{
    public class PropertiesIndexerTests
    {
        /// <summary>
        ///     Este test crea un indice y agrega el Domain (Origen) de las propiedades.
        ///     Se dan los siguientes ejemplios:
        ///     ```
        ///     Q76 (Obama) -> P31 (Type) -> Q5 (Human)
        ///     Q76 (Obama) -> P27 -> Qxx
        ///     Q76 (Obama) -> P555 -> Qxx
        ///     ...
        ///     Q5 (Human)
        ///     ...
        ///     Q17 (Country)
        ///     ...
        ///     Q298 (Chile) -> P31 (Type) -> Q17 (Country)
        ///     Q298 (Chile) -> P555 -> Qxx
        ///     Q298 (Chile) -> P777 -> Qxx
        ///     ...
        ///     Otros
        ///     ```
        ///     El dominio que se calcula, debe mostrar que:
        ///     ```
        ///     P27: Dominio Q5
        ///     P555: Dominio Q5, Q17
        ///     P777: Dominio Q17
        ///     ```
        /// </summary>
        [Fact]
        public void TestAddDomainToIndex()
        {
            const string filename = @"Resources/PropertyDomain.nt";
            const string propertyOutputPath = "PropertyDomain-PropertyIndex";
            propertyOutputPath.DeleteIfExists();

            //Act:
            new PropertiesIndexer(filename, propertyOutputPath).Index();

            using (var propertiesDirectory = FSDirectory.Open(propertyOutputPath.GetOrCreateDirectory()))
            {
                //Assert:
                var property27 = SingleDocumentQueries.QueryPropertyById("P27", propertiesDirectory);
                var property555 = SingleDocumentQueries.QueryPropertyById("P555", propertiesDirectory);
                var property777 = SingleDocumentQueries.QueryPropertyById("P777", propertiesDirectory);

                Assert.Equal("country of citinzenship", property27.Label);
                Assert.Equal("random property 555", property555.Label);
                Assert.Equal("random property 777", property777.Label);

                Assert.NotEmpty(property27.DomainTypes);
                Assert.Equal("5", property27.DomainTypes.ElementAt(0));

                Assert.NotEmpty(property555.DomainTypes);
                Assert.Equal("5", property555.DomainTypes.ElementAt(0));
                Assert.Equal("17", property555.DomainTypes.ElementAt(1));

                Assert.NotEmpty(property777.DomainTypes);
                Assert.Equal("17", property777.DomainTypes.ElementAt(0));
            }

            propertyOutputPath.DeleteIfExists();
        }

        [Fact]
        public void TestCreatePropertyIndex()
        {
            const string filename = @"Resources/PropertyIndex.nt";
            const string outputPath = "CreatePropertyIndex";

            outputPath.DeleteIfExists();

            //Act
            new PropertiesIndexer(filename, outputPath).Index();

            using (var propertiesDirectory = FSDirectory.Open(outputPath.GetOrCreateDirectory()))
            {
                //Assert
                Assert.True(Directory.Exists(outputPath));

                using (var reader = DirectoryReader.Open(propertiesDirectory))
                {
                    var docCount = reader.MaxDoc;

                    Assert.Equal(7, docCount);

                    var doc = reader.Document(0);
                    Assert.NotNull(doc);

                    //Id
                    Assert.Equal("P17", doc.GetValue(Labels.Id));

                    //Label
                    Assert.Equal("country", doc.GetValue(Labels.Label));

                    //Alt-Label:
                    Assert.Equal(4, doc.GetValues(Labels.AltLabel).Length);

                    Assert.Equal("sovereign state", doc.GetValues(Labels.AltLabel)[0]);
                    Assert.Equal("state", doc.GetValues(Labels.AltLabel)[1]);
                    Assert.Equal("land", doc.GetValues(Labels.AltLabel)[2]);

                    //Description
                    Assert.Equal("sovereign state of this item; don't use on humans",
                        doc.GetValue(Labels.Description));

                    //Frequency
                    Assert.Equal("3", doc.GetValue(Labels.Rank));
                }
            }

            outputPath.DeleteIfExists();
        }

        [Fact]
        public void TestCreatePropertyIndexAndMapResults()
        {
            const string filename = "Resources/PropertyIndex5k.nt";
            const string outputPath = "CreatePropertyIndexMapResults";

            outputPath.DeleteIfExists();

            new PropertiesIndexer(filename, outputPath).Index();

            using (var luceneDirectory = FSDirectory.Open(outputPath.GetOrCreateDirectory()))
            {
                Assert.True(Directory.Exists(outputPath));

                var queryCity = MultiDocumentQueries.QueryPropertiesByLabel("located", luceneDirectory).ToArray();
                Assert.NotEmpty(queryCity);
                var result = queryCity[0];
                Assert.Equal("P131", result.Id);
                Assert.Equal("located in the administrative territorial entity", result.Label);
                Assert.Equal("the item is located on the territory of the following administrative entity...",
                    result.Description);
                Assert.Contains("is located in", result.AltLabels);
                Assert.Contains("is in the county of", result.AltLabels);
                Assert.Contains("is in the city of", result.AltLabels);
                Assert.NotEqual(0, result.Rank);
            }

            outputPath.DeleteIfExists();
        }
    }
}