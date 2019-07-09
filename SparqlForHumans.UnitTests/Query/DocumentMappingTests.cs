using Lucene.Net.Store;
using SparqlForHumans.Lucene.Extensions;
using SparqlForHumans.Lucene.Index;
using SparqlForHumans.Lucene.Queries;
using SparqlForHumans.Models;
using SparqlForHumans.Utilities;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace SparqlForHumans.UnitTests.Query
{
    public class DocumentMappingTests
    {
        [Fact]
        public static void TestMapEntitySingleInstance()
        {
            const string filename = "Resources/EntityIndexSingleInstance-Map.nt";
            const string outputPath = "Resources/IndexSingleMapping";

            outputPath.DeleteIfExists();

            new EntitiesIndexer(filename, outputPath).Index();

            var expected = new Entity
            {
                Id = "Q26",
                Label = "Northern Ireland",
                Description = "region in north-west Europe, part of the United Kingdom",
                AltLabels = new List<string>
                {
                    "NIR",
                    "UKN",
                    "North Ireland"
                },
                InstanceOf = new List<string>
                {
                    "Q100"
                },
                SubClass = new List<string>
                {
                    "Q46"
                },
                Properties = new List<Property>
                {
                    new Property
                    {
                        Id = "P17",
                        Value = "Q145"
                    },
                    new Property
                    {
                        Id = "P47",
                        Value = "Q27"
                    },
                    new Property
                    {
                        Id = "P131",
                        Value = "Q145"
                    }
                }
            };

            var actual = new SingleLabelQuery(outputPath, expected.Label).QueryDocuments().ToEntities().FirstOrDefault();
            //var actual = SingleDocumentQueries.QueryEntityByLabel(expected.Label, luceneDirectory);

            Assert.NotNull(actual);

            //Id
            Assert.Equal(expected.Id, actual.Id);

            //Label
            Assert.Equal(expected.Label, actual.Label);

            //InstanceOf
            Assert.Equal(expected.InstanceOf.Count, actual.InstanceOf.Count);
            Assert.Equal(expected.InstanceOf.FirstOrDefault(), actual.InstanceOf.FirstOrDefault());

            //SubClass
            Assert.Equal(expected.SubClass.Count, actual.SubClass.Count);
            Assert.Equal(expected.SubClass.FirstOrDefault(), actual.SubClass.FirstOrDefault());

            //IsType
            Assert.False(actual.IsType);

            //Rank
            Assert.Equal(1, actual.Rank);

            //Alt-Label:
            Assert.Equal(expected.AltLabels.Count(), actual.AltLabels.Count());
            Assert.Equal(expected.AltLabels.ElementAt(0), actual.AltLabels.ElementAt(0));
            Assert.Equal(expected.AltLabels.ElementAt(1), actual.AltLabels.ElementAt(1));
            Assert.Equal(expected.AltLabels.ElementAt(2), actual.AltLabels.ElementAt(2));

            //Description
            Assert.Equal(expected.Description, actual.Description);

            //Properties
            Assert.Equal(expected.Properties.Count(), actual.Properties.Count());
            Assert.Equal(expected.Properties.ElementAt(0).Id, actual.Properties.ElementAt(0).Id);
            Assert.Equal(expected.Properties.ElementAt(1).Id, actual.Properties.ElementAt(1).Id);
            Assert.Equal(expected.Properties.ElementAt(2).Id, actual.Properties.ElementAt(2).Id);
        }

        [Fact]
        public static void TestMapEntityTwoInstances()
        {
            const string filename = "Resources/EntityIndexTwoInstanceOfWithTypes-Map.nt";
            const string outputPath = "Resources/IndexTwoInstanceMapping";

            outputPath.DeleteIfExists();

            var expected1 = new Entity
            {
                Id = "Q26",
                Label = "Northern Ireland",
                Description = "region in north-west Europe, part of the United Kingdom",
                AltLabels = new List<string>
                {
                    "NIR",
                    "UKN",
                    "North Ireland"
                },
                InstanceOf = new List<string>
                {
                    "Q27",
                    "Q145"
                },
                SubClass = new List<string>
                {
                    "Q46",
                    "Q47"
                },
                Properties = new List<Property>
                {
                    new Property
                    {
                        Id = "P17",
                        Value = "Q145"
                    },
                    new Property
                    {
                        Id = "P47",
                        Value = "Q27"
                    },
                    new Property
                    {
                        Id = "P131",
                        Value = "Q145"
                    }
                }
            };

            var expected2 = new Entity
            {
                Id = "Q145",
                Label = "Base1",
                Description = "Base Type1"
            };

            var expected3 = new Entity
            {
                Id = "Q27",
                Label = "Base2",
                Description = "Base Type2"
            };

            new EntitiesIndexer(filename, outputPath).Index();

            var actual = new SingleLabelQuery(outputPath, expected1.Label).QueryDocuments().ToEntities().FirstOrDefault();
            {
                //Expected 1: Id, Label, InstanceOf, SubClass, Rank, IsType, Alt-Label, Description, Properties

                Assert.NotNull(actual);

                Assert.Equal(expected1.Id, actual.Id);
                Assert.Equal(expected1.Label, actual.Label);
                Assert.Equal(expected1.Description, actual.Description);
                Assert.Equal(expected1.InstanceOf.Count, actual.InstanceOf.Count);
                Assert.Equal(expected1.InstanceOf.FirstOrDefault(), actual.InstanceOf.FirstOrDefault());
                Assert.Equal(expected1.SubClass.Count, actual.SubClass.Count);
                Assert.Equal(expected1.SubClass.FirstOrDefault(), actual.SubClass.FirstOrDefault());
                Assert.False(actual.IsType);
                Assert.Equal(0.333, actual.Rank.ToThreeDecimals());
                Assert.Equal(expected1.AltLabels.Count(), actual.AltLabels.Count());
                Assert.Equal(expected1.AltLabels.ElementAt(0), actual.AltLabels.ElementAt(0));
                Assert.Equal(expected1.AltLabels.ElementAt(1), actual.AltLabels.ElementAt(1));
                Assert.Equal(expected1.AltLabels.ElementAt(2), actual.AltLabels.ElementAt(2));
                Assert.Equal(expected1.Properties.Count(), actual.Properties.Count());
                Assert.Equal(expected1.Properties.ElementAt(0).Id, actual.Properties.ElementAt(0).Id);
                Assert.Equal(expected1.Properties.ElementAt(1).Id, actual.Properties.ElementAt(1).Id);
                Assert.Equal(expected1.Properties.ElementAt(2).Id, actual.Properties.ElementAt(2).Id);

                //Expected 2: Id, Label
                actual = new SingleLabelQuery(outputPath, expected2.Label).QueryDocuments().ToEntities().FirstOrDefault();
                Assert.NotNull(actual);
                Assert.Equal(expected2.Id, actual.Id);
                Assert.True(actual.IsType);
                Assert.Equal(0.333, actual.Rank.ToThreeDecimals());
                Assert.Equal(expected2.Label, actual.Label);

                //Expected 3: Id, Label
                actual = new SingleLabelQuery(outputPath, expected3.Label).QueryDocuments().ToEntities().FirstOrDefault();
                Assert.NotNull(actual);
                Assert.Equal(expected3.Id, actual.Id);
                Assert.True(actual.IsType);
                Assert.Equal(0.333, actual.Rank.ToThreeDecimals());
                Assert.Equal(expected3.Label, actual.Label);
            }
        }
    }
}