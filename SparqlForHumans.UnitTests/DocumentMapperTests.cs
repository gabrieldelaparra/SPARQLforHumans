using System.Collections.Generic;
using System.Linq;
using Lucene.Net.Store;
using SparqlForHumans.Core.Services;
using SparqlForHumans.Core.Utilities;
using SparqlForHumans.Models;
using Xunit;

namespace SparqlForHumans.UnitTests
{
    public class DocumentMapperTests
    {
        [Fact]
        public static void TestMapEntity()
        {
            const string outputPath = "Resources/IndexSingle";

            var entity = new Entity
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
                        Id = "P30",
                        Value = "Q46"
                    },
                    new Property
                    {
                        Id = "P131",
                        Value = "Q145"
                    }
                }
            };

            using (var luceneDirectory = FSDirectory.Open(outputPath.GetOrCreateDirectory()))
            {
                var first = SingleDocumentQueries.QueryEntityByLabel(entity.Label, luceneDirectory);

                Assert.NotNull(first);

                //Id
                Assert.Equal(entity.Id, first.Id);

                //Label
                Assert.Equal(entity.Label, first.Label);

                //InstanceOf
                //Assert.Equal(entity.InstanceOf, first.InstanceOf);
                Assert.Equal(entity.InstanceOfLabel, first.InstanceOfLabel);

                //Alt-Label:
                Assert.Equal(entity.AltLabels.Count(), first.AltLabels.Count());

                Assert.Equal(entity.AltLabels.ElementAt(0), first.AltLabels.ElementAt(0));
                Assert.Equal(entity.AltLabels.ElementAt(1), first.AltLabels.ElementAt(1));
                Assert.Equal(entity.AltLabels.ElementAt(2), first.AltLabels.ElementAt(2));

                //Description
                Assert.Equal(entity.Description, first.Description);

                //Properties and Values
                Assert.Equal(entity.Properties.Count(), first.Properties.Count());

                Assert.Equal(entity.Properties.ElementAt(0).Id, first.Properties.ElementAt(0).Id);
                //Assert.Equal(entity.Properties.ElementAt(0).Label, first.Properties.ElementAt(0).Label);
                Assert.Equal(entity.Properties.ElementAt(0).Value, first.Properties.ElementAt(0).Value);

                Assert.Equal(entity.Properties.ElementAt(1).Id, first.Properties.ElementAt(1).Id);
                //Assert.Equal(entity.Properties.ElementAt(1).Label, first.Properties.ElementAt(1).Label);
                Assert.Equal(entity.Properties.ElementAt(1).Value, first.Properties.ElementAt(1).Value);

                Assert.Equal(entity.Properties.ElementAt(2).Id, first.Properties.ElementAt(2).Id);
                //Assert.Equal(entity.Properties.ElementAt(2).Label, first.Properties.ElementAt(2).Label);
                Assert.Equal(entity.Properties.ElementAt(2).Value, first.Properties.ElementAt(2).Value);

                Assert.Equal(entity.Properties.ElementAt(3).Id, first.Properties.ElementAt(3).Id);
                //Assert.Equal(entity.Properties.ElementAt(3).Label, first.Properties.ElementAt(3).Label);
                Assert.Equal(entity.Properties.ElementAt(3).Value, first.Properties.ElementAt(3).Value);
            }
        }
    }
}