using System.IO;
using System.Linq;
using Lucene.Net.Index;
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
    public class PropertyIndexTests
    {
        /// <summary>
        /// Este test crea un indice y agrega el Domain (Origen) de las propiedades.
        /// Se dan los siguientes ejemplios:
        ///
        /// ```
        /// Q76 (Obama) -> P31 (Type) -> Q5 (Human)
        /// Q76 (Obama) -> P27 -> Qxx
        /// Q76 (Obama) -> P555 -> Qxx
        /// ...
        /// Q5 (Human)
        /// ...
        /// Q17 (Country)
        /// ...
        /// Q298 (Chile) -> P31 (Type) -> Q17 (Country)
        /// Q298 (Chile) -> P555 -> Qxx
        /// Q298 (Chile) -> P777 -> Qxx
        /// ...
        /// Otros
        /// ```
        ///
        /// El dominio que se calcula, debe mostrar que:
        ///
        /// ```
        /// P27: Dominio Q5
        /// P555: Dominio Q5, Q17
        /// P777: Dominio Q17
        /// ```
        /// 
        /// </summary>
        [Fact]
        public void TestAddDomainToIndex()
        {
            const string filename = @"Resources/PropertyDomain.nt";
            Assert.True(File.Exists(filename));

            const string entitiesOutputPath = "EntitiesIndex";
            const string propertyOutputPath = "PropertyIndex";

            entitiesOutputPath.DeleteIfExists();
            propertyOutputPath.DeleteIfExists();

            Assert.False(Directory.Exists(entitiesOutputPath));
            Assert.False(Directory.Exists(propertyOutputPath));

            using (var entitiesDirectory = FSDirectory.Open(entitiesOutputPath.GetOrCreateDirectory()))
            using (var propertiesDirectory = FSDirectory.Open(propertyOutputPath.GetOrCreateDirectory()))
            {
                EntitiesIndex.CreateEntitiesIndex(filename, entitiesDirectory, true);
                PropertiesIndex.CreatePropertiesIndex(filename, propertiesDirectory, true);

                var typesAndPropertiesDictionary = IndexBuilder.CreateTypesAndPropertiesDictionary(entitiesDirectory);
                var invertedPropertiesDictionary = IndexBuilder.CreateInvertedProperties(typesAndPropertiesDictionary);

                var property27 = SingleDocumentQueries.QueryPropertyById("P27", propertiesDirectory);
                var property555 = SingleDocumentQueries.QueryPropertyById("P555", propertiesDirectory);
                var property777 = SingleDocumentQueries.QueryPropertyById("P777", propertiesDirectory);

                Assert.Empty(property27.DomainTypes);
                Assert.Empty(property555.DomainTypes);
                Assert.Empty(property777.DomainTypes);

                PropertiesIndex.AddDomainTypesToPropertiesIndex(propertiesDirectory, invertedPropertiesDictionary);

                var property27WithDomain = SingleDocumentQueries.QueryPropertyById("P27", propertiesDirectory);
                var property555WithDomain = SingleDocumentQueries.QueryPropertyById("P555", propertiesDirectory);
                var property777WithDomain = SingleDocumentQueries.QueryPropertyById("P777", propertiesDirectory);

                Assert.NotEmpty(property27WithDomain.DomainTypes);
                Assert.Equal("Q5", property27WithDomain.DomainTypes.ElementAt(0));

                Assert.NotEmpty(property555WithDomain.DomainTypes);
                Assert.Equal("Q5", property555WithDomain.DomainTypes.ElementAt(0));
                Assert.Equal("Q17", property555WithDomain.DomainTypes.ElementAt(1));

                Assert.NotEmpty(property777WithDomain.DomainTypes);
                Assert.Equal("Q17", property777WithDomain.DomainTypes.ElementAt(0));
            }

            propertyOutputPath.DeleteIfExists();
            entitiesOutputPath.DeleteIfExists();
        }

        /// <summary>
        /// Este test crea un indice y agrega el Range (Destino) de las propiedades.
        /// Se dan los siguientes ejemplios:
        ///
        /// ```
        /// Q76 (Obama) -> P31 (Type) -> Q5 (Human)
        /// Q76 (Obama) -> P69 (EducatedAt) -> Q49088 (Columbia)
        /// Q76 (Obama) -> P69 (EducatedAt) -> Q49122 (Harvard)
        /// Q76 (Obama) -> P555 -> Qxx
        /// ...
        /// Q49088 (Columbia) -> P31 (Type) -> Q902104 (Private)
        /// Q49088 (Columbia) -> P31 (Type) -> Q15936437 (Research)
        /// Q49088 (Columbia) -> P31 (Type) -> Q1188663 (Colonial)
        /// Q49088 (Columbia) -> P31 (Type) -> Q23002054 (NonProfit)
        /// ...
        /// Q49122 (Harvard) -> P31 (Type) -> Q13220391 (Graduate)
        /// Q49122 (Harvard) -> P31 (Type) -> Q1321960 (Law)
        /// ...
        /// Q298 (Chile) -> P31 (Type) -> Q17 (Country)
        /// Q298 (Chile) -> P38 (Currency) -> Q200050 (Peso)
        /// Q298 (Chile) -> P38 (Currency) -> Q1573250 (UF)
        /// Q298 (Chile) -> P777 -> Qxx
        /// ...
        /// Q200050 (Peso) -> P31 (Type) -> Q1643989 (Legal Tender)
        /// Q200050 (Peso) -> P31 (Type) -> Q8142 (Currency)
        /// ...
        /// Q1573250 (UF) -> P31 (Type) -> Q747699 (UnitOfAccount)
        /// ...
        /// Otros
        /// ```
        ///
        /// El Range que se calcula, debe mostrar que:
        ///
        /// ```
        /// P69: Range (4+2) Q902104, Q15936437, Q1188663, Q23002054, Q13220391, Q1321960
        /// P38: Range (3+1) Q1643989, Q8142, Q747699
        /// ```
        /// 
        /// </summary>
        [Fact]
        public void TestAddRangeToIndex()
        {

        }

        [Fact]
        public void TestCreateBasicIndex()
        {
            const string filename = @"Resources/PropertyIndex.nt";
            Assert.True(File.Exists(filename));

            const string outputPath = "PropertyIndex";

            outputPath.DeleteIfExists();

            Assert.False(Directory.Exists(outputPath));

            using (var propertiesDirectory = FSDirectory.Open(outputPath.GetOrCreateDirectory()))
            {
                PropertiesIndex.CreatePropertiesIndex(filename, propertiesDirectory, true);
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

                    Assert.Equal("sovereign state", doc.GetValue(Labels.AltLabel));

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
    }
}