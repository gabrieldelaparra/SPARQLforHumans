using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SparqlForHumans.Lucene.Extensions;
using SparqlForHumans.Lucene.Indexing;
using SparqlForHumans.RDF.Extensions;
using SparqlForHumans.Utilities;
using Xunit;

namespace SparqlForHumans.UnitTests
{
    public class PropertyRangeTests
    {
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
        /// P38: Range (2+1) Q1643989, Q8142, Q747699
        /// ```
        /// 
        /// </summary>
        [Fact]
        public void TestCreatePropertyRange()
        {
            // Arrange
            const string filename = "Resources/PropertyRange.nt";
            var lines = FileHelper.GetInputLines(filename);
            var entityGroups = lines.GroupBySubject().ToArray();
            
            //EntityTypes Dictionary
            var entityTypes = entityGroups
                .Select(x => x.GetEntityTypes())
                .ToDictionary();

            var propertyRangeTriples = lines
                .Select(x => x.GetTriple())
                .Where(PropertyRange.IsValidPropertyRangeTriple);

            // Act
            var dictionary = propertyRangeTriples
                .Select(x => x.GetPropertyRangeType(ref entityTypes))
                .ToDictionary();

            // Assert
            Assert.Equal(2, dictionary.Count);
            var property69WithRange = dictionary[69];
            var property38WithRange = dictionary[38];

            Assert.NotEmpty(property69WithRange);
            Assert.Equal(902104, property69WithRange[0]);
            Assert.Equal(15936437, property69WithRange[1]);
            Assert.Equal(1188663, property69WithRange[2]);
            Assert.Equal(23002054, property69WithRange[3]);
            Assert.Equal(13220391, property69WithRange[4]);
            Assert.Equal(1321960, property69WithRange[5]);

            Assert.NotEmpty(property38WithRange);
            Assert.Equal(1643989, property38WithRange[0]);
            Assert.Equal(8142, property38WithRange[1]);
            Assert.Equal(747699, property38WithRange[2]);

        }


        [Fact]
        public void TestAddRangeToIndex()
        {
            Assert.False(true);
            //const string filename = @"Resources/PropertyRange.nt";
            //Assert.True(File.Exists(filename));

            //const string entitiesOutputPath = "EntitiesIndex";
            //const string propertyOutputPath = "PropertyIndex";

            //entitiesOutputPath.DeleteIfExists();
            //propertyOutputPath.DeleteIfExists();

            //Assert.False(Directory.Exists(entitiesOutputPath));
            //Assert.False(Directory.Exists(propertyOutputPath));

            //using (var entitiesDirectory = FSDirectory.Open(entitiesOutputPath.GetOrCreateDirectory()))
            //using (var propertiesDirectory = FSDirectory.Open(propertyOutputPath.GetOrCreateDirectory()))
            //{
            //    EntitiesIndex.CreateEntitiesIndex(filename, entitiesDirectory, true);
            //    PropertiesIndex.CreatePropertiesIndex(filename, propertiesDirectory, true);

            //    var typesAndPropertiesDictionary = IndexBuilder.CreateTypesAndPropertiesDictionary(entitiesDirectory);
            //    var invertedPropertiesDictionary = IndexBuilder.CreateInvertedProperties(typesAndPropertiesDictionary);

            //    var property69 = SingleDocumentQueries.QueryPropertyById("P69", propertiesDirectory);
            //    var property38 = SingleDocumentQueries.QueryPropertyById("P38", propertiesDirectory);

            //    Assert.Empty(property69.DomainTypes);
            //    Assert.Empty(property38.DomainTypes);

            //    //Hay que implementar esto.
            //    PropertiesIndex.AddRangeTypesToPropertiesIndex(propertiesDirectory, invertedPropertiesDictionary);

            //    var property69WithRange = SingleDocumentQueries.QueryPropertyById("P69", propertiesDirectory);
            //    var property38WithRange = SingleDocumentQueries.QueryPropertyById("P38", propertiesDirectory);

            //    Assert.NotEmpty(property69WithRange.DomainTypes);
            //    Assert.Equal("Q902104", property69WithRange.DomainTypes.ElementAt(0));
            //    Assert.Equal("Q15936437", property69WithRange.DomainTypes.ElementAt(1));
            //    Assert.Equal("Q1188663", property69WithRange.DomainTypes.ElementAt(2));
            //    Assert.Equal("Q23002054", property69WithRange.DomainTypes.ElementAt(3));
            //    Assert.Equal("Q13220391", property69WithRange.DomainTypes.ElementAt(4));
            //    Assert.Equal("Q1321960", property69WithRange.DomainTypes.ElementAt(5));

            //    Assert.NotEmpty(property38WithRange.DomainTypes);
            //    Assert.Equal("Q1643989", property38WithRange.DomainTypes.ElementAt(0));
            //    Assert.Equal("Q8142", property38WithRange.DomainTypes.ElementAt(1));
            //    Assert.Equal("Q747699", property38WithRange.DomainTypes.ElementAt(2));
            //}

            //propertyOutputPath.DeleteIfExists();
            //entitiesOutputPath.DeleteIfExists();
        }
    }
}
