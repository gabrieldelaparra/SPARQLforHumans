using Lucene.Net.Index;
using SparqlForHumans.Core.Properties;
using SparqlForHumans.Core.Services;
using SparqlForHumans.Core.Utilities;
using System.IO;
using System.Linq;
using Xunit;

namespace SparqlForHumans.UnitTests
{
    public class PropertyIndexTests
    {
        [Fact]
        public void TestCreateBasicIndex()
        {

            const string filename = @"Resources/PropertyIndex.nt";
            Assert.True(File.Exists(filename));

            const string outputPath = "PropertyIndex";

            outputPath.DeleteIfExists();

            Assert.False(Directory.Exists(outputPath));

            IndexBuilder.CreatePropertiesIndex(filename, outputPath.GetLuceneDirectory(), true);

            Assert.True(Directory.Exists(outputPath));

            using (var reader = IndexReader.Open(outputPath.GetLuceneDirectory(), true))
            {
                var docCount = reader.MaxDoc;

                Assert.Equal(7, docCount);

                var doc = reader.Document(0);
                Assert.NotNull(doc);
                //Id
                Assert.Equal("P17", doc.GetField(Labels.Id.ToString()).StringValue);

                //Label
                Assert.Equal("country", doc.GetField(Labels.Label.ToString()).StringValue);

                //Alt-Label:
                Assert.Equal(4, doc.GetFields(Labels.AltLabel.ToString()).Length);

                Assert.Equal("sovereign state", doc.GetField(Labels.AltLabel.ToString()).StringValue);

                Assert.Equal("sovereign state", doc.GetFields(Labels.AltLabel.ToString())[0].StringValue);
                Assert.Equal("state", doc.GetFields(Labels.AltLabel.ToString())[1].StringValue);
                Assert.Equal("land", doc.GetFields(Labels.AltLabel.ToString())[2].StringValue);

                //Description
                Assert.Equal("sovereign state of this item; don't use on humans", doc.GetField(Labels.Description.ToString()).StringValue);

                //Frequency
                Assert.Equal("3", doc.GetField(Labels.Rank.ToString()).StringValue);
            }

            outputPath.DeleteIfExists();
        }

        [Fact]
        public void TestAddDomainToIndex()
        {
            const string filename = @"Resources/PropertyDomain.nt";
            Assert.True(File.Exists(filename));

            const string propertyOutputPath = "PropertyIndex";
            const string entitiesOutputPath = "EntitiesIndex";

            propertyOutputPath.DeleteIfExists();
            entitiesOutputPath.DeleteIfExists();

            Assert.False(Directory.Exists(entitiesOutputPath));
            Assert.False(Directory.Exists(propertyOutputPath));

            var entitiesDirectory = entitiesOutputPath.GetLuceneDirectory();
            var propertiesDirectory = propertyOutputPath.GetLuceneDirectory();

            IndexBuilder.CreateEntitiesIndex(filename, entitiesDirectory, true);
            var typesAndPropertiesDictionary = IndexBuilder.CreateTypesAndPropertiesDictionary(entitiesDirectory);

            IndexBuilder.CreatePropertiesIndex(filename, propertiesDirectory, true);
            var invertedPropertiesDictionary = IndexBuilder.CreateInvertedProperties(typesAndPropertiesDictionary);

            var property27 = SingleDocumentQueries.QueryPropertyById("P27", propertiesDirectory);
            var property555 = SingleDocumentQueries.QueryPropertyById("P555", propertiesDirectory);
            var property777 = SingleDocumentQueries.QueryPropertyById("P777", propertiesDirectory);

            Assert.Empty(property27.DomainTypes);
            Assert.Empty(property555.DomainTypes);
            Assert.Empty(property777.DomainTypes);

            IndexBuilder.AddDomainTypesToPropertiesIndex(propertiesDirectory, invertedPropertiesDictionary);

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

            propertyOutputPath.DeleteIfExists();
            entitiesOutputPath.DeleteIfExists();
        }
    }

    
}