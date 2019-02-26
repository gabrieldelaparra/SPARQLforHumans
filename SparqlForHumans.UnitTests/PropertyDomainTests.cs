using System.IO;
using System.Linq;
using Lucene.Net.Store;
using SparqlForHumans.Lucene.Extensions;
using SparqlForHumans.Lucene.Indexing;
using SparqlForHumans.Lucene.Queries;
using SparqlForHumans.Utilities;
using Xunit;
using Directory = System.IO.Directory;

namespace SparqlForHumans.UnitTests
{
    public class PropertyDomainTests
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
                var invertedPropertiesDictionary = typesAndPropertiesDictionary.InvertDictionary();

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
    }
}
