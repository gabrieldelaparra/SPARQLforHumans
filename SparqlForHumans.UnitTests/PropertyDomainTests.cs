using System.Collections.Generic;
using System.IO;
using System.Linq;
using Lucene.Net.Store;
using SparqlForHumans.Lucene.Indexing;
using SparqlForHumans.Lucene.Indexing.Indexer;
using SparqlForHumans.Lucene.Queries;
using SparqlForHumans.Lucene.Relations;
using SparqlForHumans.RDF.Extensions;
using SparqlForHumans.Utilities;
using Xunit;
using Directory = System.IO.Directory;

namespace SparqlForHumans.UnitTests
{
    public class PropertyDomainTests
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
            Assert.True(File.Exists(filename));

            // Prepare output directories
            const string entitiesOutputPath = "PropertyDomain-EntitiesIndex";
            const string propertyOutputPath = "PropertyDomain-PropertyIndex";

            entitiesOutputPath.DeleteIfExists();
            propertyOutputPath.DeleteIfExists();

            Assert.False(Directory.Exists(entitiesOutputPath));
            Assert.False(Directory.Exists(propertyOutputPath));

            new EntitiesIndexer(filename, entitiesOutputPath).Index();
            new PropertiesIndexer(filename, propertyOutputPath).Index();

            // Create indexes
            using (var entitiesDirectory = FSDirectory.Open(entitiesOutputPath.GetOrCreateDirectory()))
            using (var propertiesDirectory = FSDirectory.Open(propertyOutputPath.GetOrCreateDirectory()))
            {
                //No domains in the Index
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
            entitiesOutputPath.DeleteIfExists();
        }

        [Fact]
        public void TestGetPropertyDomainMultipleDomain()
        {
            //Arrange
            var lines = new List<string>
            {
                "<http://www.wikidata.org/entity/Q76> <http://www.wikidata.org/prop/direct/P31> <http://www.wikidata.org/entity/Q5> .",
                "<http://www.wikidata.org/entity/Q76> <http://www.wikidata.org/prop/direct/P27> <http://www.wikidata.org/entity/Q30> .",
                "<http://www.wikidata.org/entity/Q76> <http://www.wikidata.org/prop/direct/P555> <http://www.wikidata.org/entity/Q556> .",
                "<http://www.wikidata.org/entity/Q298> <http://www.wikidata.org/prop/direct/P31> <http://www.wikidata.org/entity/Q17> .",
                "<http://www.wikidata.org/entity/Q298> <http://www.wikidata.org/prop/direct/P555> <http://www.wikidata.org/entity/Q31> .",
                "<http://www.wikidata.org/entity/Q298> <http://www.wikidata.org/prop/direct/P777> <http://www.wikidata.org/entity/Q32> ."
            };
            var entityGroups = lines.GroupBySubject();

            var propertyDomainTypes = new PropertyToSubjectTypesRelationMapper(entityGroups).RelationIndex.ToArray();

            // P27, P555, P777
            Assert.Equal(3, propertyDomainTypes.Length);

            // P27 -> 5
            Assert.Equal(27, propertyDomainTypes[0].Key);
            Assert.Single(propertyDomainTypes[0].Value);
            Assert.Equal(5, propertyDomainTypes[0].Value[0]);

            // P555 -> 5, 17
            Assert.Equal(555, propertyDomainTypes[1].Key);
            Assert.Equal(2, propertyDomainTypes[1].Value.Length);
            Assert.Equal(5, propertyDomainTypes[1].Value[0]);
            Assert.Equal(17, propertyDomainTypes[1].Value[1]);

            // P777 -> 17
            Assert.Equal(777, propertyDomainTypes[2].Key);
            Assert.Single(propertyDomainTypes[2].Value);
            Assert.Equal(17, propertyDomainTypes[2].Value[0]);
        }

        [Fact]
        public void TestGetPropertyDomainMultipleTypesAndDomain()
        {
            //Arrange
            var lines = new List<string>
            {
                "<http://www.wikidata.org/entity/Q76> <http://www.wikidata.org/prop/direct/P31> <http://www.wikidata.org/entity/Q5> .",
                "<http://www.wikidata.org/entity/Q76> <http://www.wikidata.org/prop/direct/P27> <http://www.wikidata.org/entity/Q30> .",
                "<http://www.wikidata.org/entity/Q76> <http://www.wikidata.org/prop/direct/P555> <http://www.wikidata.org/entity/Q556> .",
                "<http://www.wikidata.org/entity/Q298> <http://www.wikidata.org/prop/direct/P31> <http://www.wikidata.org/entity/Q17> .",
                "<http://www.wikidata.org/entity/Q298> <http://www.wikidata.org/prop/direct/P555> <http://www.wikidata.org/entity/Q31> .",
                "<http://www.wikidata.org/entity/Q298> <http://www.wikidata.org/prop/direct/P777> <http://www.wikidata.org/entity/Q32> .",
                "<http://www.wikidata.org/entity/Q77> <http://www.wikidata.org/prop/direct/P31> <http://www.wikidata.org/entity/Q5> .",
                "<http://www.wikidata.org/entity/Q77> <http://www.wikidata.org/prop/direct/P31> <http://www.wikidata.org/entity/Q6> .",
                "<http://www.wikidata.org/entity/Q77> <http://www.wikidata.org/prop/direct/P27> <http://www.wikidata.org/entity/Q30> .",
                "<http://www.wikidata.org/entity/Q77> <http://www.wikidata.org/prop/direct/P555> <http://www.wikidata.org/entity/Q556> ."
            };
            var entityGroups = lines.GroupBySubject();

            var propertyDomainTypes = new PropertyToSubjectTypesRelationMapper(entityGroups).RelationIndex.ToArray();

            // P27, P555, P777
            Assert.Equal(3, propertyDomainTypes.Length);

            // P27 -> 5, 6
            Assert.Equal(27, propertyDomainTypes[0].Key);
            Assert.Equal(2, propertyDomainTypes[0].Value.Length);
            Assert.Equal(5, propertyDomainTypes[0].Value[0]);
            Assert.Equal(6, propertyDomainTypes[0].Value[1]);

            // P555 -> 5, 17, 6
            Assert.Equal(555, propertyDomainTypes[1].Key);
            Assert.Equal(3, propertyDomainTypes[1].Value.Length);
            Assert.Equal(5, propertyDomainTypes[1].Value[0]);
            Assert.Equal(17, propertyDomainTypes[1].Value[1]);
            Assert.Equal(6, propertyDomainTypes[1].Value[2]);

            // P777 -> 17
            Assert.Equal(777, propertyDomainTypes[2].Key);
            Assert.Single(propertyDomainTypes[2].Value);
            Assert.Equal(17, propertyDomainTypes[2].Value[0]);
        }

        [Fact]
        public void TestGetPropertyDomainSingleDomain()
        {
            //Arrange
            var lines = new List<string>
            {
                "<http://www.wikidata.org/entity/Q76> <http://www.wikidata.org/prop/direct/P31> <http://www.wikidata.org/entity/Q5> .",
                "<http://www.wikidata.org/entity/Q76> <http://www.wikidata.org/prop/direct/P27> <http://www.wikidata.org/entity/Q30> .",
                "<http://www.wikidata.org/entity/Q76> <http://www.wikidata.org/prop/direct/P555> <http://www.wikidata.org/entity/Q556> ."
            };
            var entityGroups = lines.GroupBySubject();

            var propertyDomainTypes = new PropertyToSubjectTypesRelationMapper(entityGroups).RelationIndex.ToArray();

            // P27, P555
            Assert.Equal(2, propertyDomainTypes.Length);

            // P27 -> P5
            Assert.Equal(27, propertyDomainTypes[0].Key);
            Assert.Single(propertyDomainTypes[0].Value);
            Assert.Equal(5, propertyDomainTypes[0].Value[0]);

            // P555 -> P5
            Assert.Equal(555, propertyDomainTypes[1].Key);
            Assert.Single(propertyDomainTypes[1].Value);
            Assert.Equal(5, propertyDomainTypes[1].Value[0]);
        }
    }
}