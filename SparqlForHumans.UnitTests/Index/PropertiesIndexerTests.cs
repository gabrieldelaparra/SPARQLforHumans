﻿using System.Collections.Generic;
using System.Linq;
using Lucene.Net.Index;
using Lucene.Net.Store;
using SparqlForHumans.Lucene.Extensions;
using SparqlForHumans.Lucene.Index;
using SparqlForHumans.Lucene.Queries.Fields;
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
            const string propertiesIndexPath = "PropertyDomain-PropertyIndex";
            const string entitiesIndexPath = "PropertyDomain-EntitiesIndex";
            propertiesIndexPath.DeleteIfExists();
            entitiesIndexPath.DeleteIfExists();

            //Act:
            new EntitiesIndexer(filename, entitiesIndexPath).Index();
            new PropertiesIndexer(filename, propertiesIndexPath, entitiesIndexPath).Index();

            var properties = new BatchIdPropertyQuery(propertiesIndexPath, new List<string> {"P27", "P555", "P777"})
                .Query().ToArray();

            //Assert:
            Assert.NotEmpty(properties);
            Assert.Equal(3, properties.Length);

            Assert.Equal("P555", properties[0].Id);
            Assert.Equal("P27", properties[1].Id);
            Assert.Equal("P777", properties[2].Id);
            
            var property555 = properties[0];
            var property27 = properties[1];
            var property777 = properties[2];

            Assert.Equal("country of citinzenship", property27.Label);
            Assert.Equal("random property 555", property555.Label);
            Assert.Equal("random property 777", property777.Label);

            Assert.NotEmpty(property27.Domain);
            Assert.Equal(5, property27.Domain.ElementAt(0));

            Assert.NotEmpty(property555.Domain);
            Assert.Equal(5, property555.Domain.ElementAt(0));
            Assert.Equal(17, property555.Domain.ElementAt(1));

            Assert.NotEmpty(property777.Domain);
            Assert.Equal(17, property777.Domain.ElementAt(0));

            propertiesIndexPath.DeleteIfExists();
            entitiesIndexPath.DeleteIfExists();
        }

        /// <summary>
        ///     Este test crea un indice y agrega el Range (Destino) de las propiedades.
        ///     Se dan los siguientes ejemplios:
        ///     ```
        ///     Q76 (Obama) -> P31 (Type) -> Q5 (Human)
        ///     Q76 (Obama) -> P69 (EducatedAt) -> Q49088 (Columbia)
        ///     Q76 (Obama) -> P69 (EducatedAt) -> Q49122 (Harvard)
        ///     Q76 (Obama) -> P555 -> Qxx
        ///     ...
        ///     Q49088 (Columbia) -> P31 (Type) -> Q902104 (Private)
        ///     Q49088 (Columbia) -> P31 (Type) -> Q15936437 (Research)
        ///     Q49088 (Columbia) -> P31 (Type) -> Q1188663 (Colonial)
        ///     Q49088 (Columbia) -> P31 (Type) -> Q23002054 (NonProfit)
        ///     ...
        ///     Q49122 (Harvard) -> P31 (Type) -> Q13220391 (Graduate)
        ///     Q49122 (Harvard) -> P31 (Type) -> Q1321960 (Law)
        ///     ...
        ///     Q298 (Chile) -> P31 (Type) -> Q17 (Country)
        ///     Q298 (Chile) -> P38 (Currency) -> Q200050 (Peso)
        ///     Q298 (Chile) -> P38 (Currency) -> Q1573250 (UF)
        ///     Q298 (Chile) -> P777 -> Qxx
        ///     ...
        ///     Q200050 (Peso) -> P31 (Type) -> Q1643989 (Legal Tender)
        ///     Q200050 (Peso) -> P31 (Type) -> Q8142 (Currency)
        ///     ...
        ///     Q1573250 (UF) -> P31 (Type) -> Q747699 (UnitOfAccount)
        ///     ...
        ///     Otros
        ///     ```
        ///     El Range que se calcula, debe mostrar que:
        ///     ```
        ///     P69: Range (4+2) Q902104, Q15936437, Q1188663, Q23002054, Q13220391, Q1321960
        ///     P38: Range (2+1) Q1643989, Q8142, Q747699
        ///     ```
        /// </summary>
        [Fact]
        public void TestAddRangeToIndex()
        {
            //Arrange
            const string filename = "Resources/PropertyRange.nt";
            const string propertiesIndexPath = "PropertyRangeIndex";
            const string entitiesIndexPath = "PropertyRangeIndex-EntitiesIndex";
            propertiesIndexPath.DeleteIfExists();
            entitiesIndexPath.DeleteIfExists();

            //Act
            new EntitiesIndexer(filename, entitiesIndexPath).Index();
            new PropertiesIndexer(filename, propertiesIndexPath,entitiesIndexPath).Index();
            var properties = new MultiLabelPropertyQuery(propertiesIndexPath, "*").Query().ToList();

            //Assert
            Assert.NotEmpty(properties);
            Assert.Equal(3, properties.Count); //P68, P38, P31

            var property69WithRange = properties.FirstOrDefault(x => x.Id.Equals("P69")).Range;
            var property38WithRange = properties.FirstOrDefault(x => x.Id.Equals("P38")).Range;

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

            propertiesIndexPath.DeleteIfExists();
            entitiesIndexPath.DeleteIfExists();
        }

        /// <summary>
        ///     Este test crea un indice y agrega el Range (Destino) de las propiedades.
        ///     Se dan los siguientes ejemplos:
        ///     ```
        ///     Q76 (Obama) -> P31 (Type) -> Q5 (Human)
        ///     ...
        ///     Q5 (Human) -> P31 (Type) -> Q100 (Living Being)
        ///     ...
        ///     Q298 (Chile) -> P31 (Type) -> Q17 (Country)
        ///     ...
        ///     Q17 (Country) -> P31 (Type) -> Q200 (TerritoryDivision)
        ///     ...
        ///     ```
        ///     El Range que se calcula, debe mostrar que:
        ///     ```
        ///     P31: Range (2) Q100, Q200
        ///     ```
        /// </summary>
        [Fact]
        public void TestAddRangeToIndex_InstanceOf()
        {
            //Arrange
            const string filename = "Resources/PropertyRange-P31.nt";
            const string propertiesIndexPath = "PropertyRangeIndex_P31";
            const string entitiesIndexPath = "PropertyRangeIndex_P31-EntitiesIndex";
            propertiesIndexPath.DeleteIfExists();
            entitiesIndexPath.DeleteIfExists();

            //Act
            new EntitiesIndexer(filename, entitiesIndexPath).Index();
            new PropertiesIndexer(filename, propertiesIndexPath,entitiesIndexPath).Index();
            var properties = new MultiLabelPropertyQuery(propertiesIndexPath, "*").Query();

            //Assert
            Assert.NotEmpty(properties);
            Assert.Single(properties); //P31

            var property31 = properties.FirstOrDefault(x => x.Id.Equals("P31"));
            var property31WithRange = property31.Range;

            //TODO: Not sure what is the correct result, but in the query results tests, this makes no sense.
            //Assert.Empty(property31WithRange);
            Assert.NotEmpty(property31WithRange);
            Assert.Contains(100, property31WithRange);
            Assert.Contains(200, property31WithRange);
            Assert.Contains(5, property31WithRange);

            propertiesIndexPath.DeleteIfExists();
            entitiesIndexPath.DeleteIfExists();
        }

        [Fact]
        public void TestCreatePropertyIndex()
        {
            const string filename = @"Resources/PropertyIndex.nt";
            const string propertiesIndexPath = "CreatePropertyIndex";
            const string entitiesIndexPath = "CreatePropertyIndex-EntitiesIndex";
            propertiesIndexPath.DeleteIfExists();
            entitiesIndexPath.DeleteIfExists();

            //Act
            new EntitiesIndexer(filename, entitiesIndexPath).Index();
            new PropertiesIndexer(filename, propertiesIndexPath, entitiesIndexPath).Index();

            using (var propertiesDirectory = FSDirectory.Open(propertiesIndexPath.GetOrCreateDirectory())) {
                //Assert
                using var reader = DirectoryReader.Open(propertiesDirectory);
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

            propertiesIndexPath.DeleteIfExists();
            entitiesIndexPath.DeleteIfExists();
        }

        [Fact]
        public void TestCreatePropertyIndexAndMapResults()
        {
            const string filename = "Resources/PropertyIndex5k.nt";
            const string propertiesIndexPath = "CreatePropertyIndexMapResults";
            const string entitiesIndexPath = "CreatePropertyIndexMapResults-EntitiesIndex";

            propertiesIndexPath.DeleteIfExists();
            entitiesIndexPath.DeleteIfExists();

            new EntitiesIndexer(filename, entitiesIndexPath).Index();
            new PropertiesIndexer(filename, propertiesIndexPath,entitiesIndexPath).Index();

            Assert.True(Directory.Exists(propertiesIndexPath));

            var queryCity = new SingleLabelPropertyQuery(propertiesIndexPath, "located").Query().ToArray();

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
            
            propertiesIndexPath.DeleteIfExists();
            entitiesIndexPath.DeleteIfExists();
        }
    }
}