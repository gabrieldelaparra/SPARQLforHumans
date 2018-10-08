﻿using System.Collections.Generic;
using System.Linq;
using Lucene.Net.Store;
using SparqlForHumans.Lucene.Services;
using SparqlForHumans.Lucene.Services.Query;
using SparqlForHumans.Lucene.Utilities;
using Xunit;
using Directory = System.IO.Directory;

namespace SparqlForHumans.UnitTests
{
    public class QueryServiceTests
    {
        [Fact]
        public void TestManyBarackObamaShouldShowFirst()
        {
            const string filename = "Resources/ObamaQueries.nt";
            const string outputPath = "BarackObamaIndex";

            outputPath.DeleteIfExists();

            using (var luceneIndexDirectory = FSDirectory.Open(outputPath.GetOrCreateDirectory()))
            {
                IndexBuilder.CreateEntitiesIndex(filename, luceneIndexDirectory, true);
                var entity = MultiDocumentQueries.QueryEntitiesByLabel("Obama", luceneIndexDirectory)
                    .FirstOrDefault();
                Assert.Equal("Q76", entity.Id);
            }

            outputPath.DeleteIfExists();
        }

        [Fact]
        public void TestManyMichelleObamaShouldShowFirst()
        {
            const string filename = "Resources/ObamaQueries.nt";
            const string outputPath = "MichelleObamaIndex";

            outputPath.DeleteIfExists();

            using (var luceneIndexDirectory = FSDirectory.Open(outputPath.GetOrCreateDirectory()))
            {
                IndexBuilder.CreateEntitiesIndex(filename, luceneIndexDirectory, true);
                var entity = MultiDocumentQueries
                    .QueryEntitiesByLabel("Michelle Obama", luceneIndexDirectory)
                    .FirstOrDefault();
                Assert.Equal("Q13133", entity.Id);
            }

            outputPath.DeleteIfExists();
        }

        [Fact]
        public void TestQueryAddProperties()
        {
            const string outputPath = "Resources/PropertyIndex";

            Assert.True(Directory.Exists(outputPath));
            using (var luceneIndexDirectory = FSDirectory.Open(outputPath.GetOrCreateDirectory()))
            {
                var entity = SingleDocumentQueries.QueryEntityById("Q26", luceneIndexDirectory);

                Assert.NotNull(entity);
                Assert.Equal("Q26", entity.Id);

                //Properties
                Assert.Equal(4, entity.Properties.Count());

                Assert.Equal("P17", entity.Properties.ElementAt(0).Id);
                //Assert.Equal("Q145", entity.Properties.ElementAt(0).Value);
                //Assert.Equal(string.Empty, entity.Properties.ElementAt(0).Label);

                Assert.Equal("P47", entity.Properties.ElementAt(1).Id);
                //Assert.Equal("Q27", entity.Properties.ElementAt(1).Value);
                //Assert.Equal(string.Empty, entity.Properties.ElementAt(1).Label);

                Assert.Equal("P30", entity.Properties.ElementAt(2).Id);
                //Assert.Equal("Q46", entity.Properties.ElementAt(2).Value);
                //Assert.Equal(string.Empty, entity.Properties.ElementAt(2).Label);

                Assert.Equal("P131", entity.Properties.ElementAt(3).Id);
                //Assert.Equal("Q145", entity.Properties.ElementAt(3).Value);
                //Assert.Equal(string.Empty, entity.Properties.ElementAt(3).Label);

                entity.AddProperties(luceneIndexDirectory);

                Assert.Equal("P17", entity.Properties.ElementAt(0).Id);
                //Assert.Equal("Q145", entity.Properties.ElementAt(0).Value);
                Assert.Equal("country", entity.Properties.ElementAt(0).Label);

                Assert.Equal("P47", entity.Properties.ElementAt(1).Id);
                //Assert.Equal("Q27", entity.Properties.ElementAt(1).Value);
                Assert.Equal("shares border with", entity.Properties.ElementAt(1).Label);

                Assert.Equal("P30", entity.Properties.ElementAt(2).Id);
                //Assert.Equal("Q46", entity.Properties.ElementAt(2).Value);
                Assert.Equal("continent", entity.Properties.ElementAt(2).Label);

                Assert.Equal("P131", entity.Properties.ElementAt(3).Id);
                //Assert.Equal("Q145", entity.Properties.ElementAt(3).Value);
                Assert.Equal("located in the administrative territorial entity", entity.Properties.ElementAt(3).Label);
            }
        }

        [Fact]
        public void TestQueryByMultipleIds()
        {
            var ids = new List<string> {"Q26", "Q27", "Q29"};
            const string outputPath = "Resources/IndexMultiple";
            Assert.True(Directory.Exists(outputPath));

            using (var luceneIndexDirectory = FSDirectory.Open(outputPath.GetOrCreateDirectory()))
            {
                var entities = MultiDocumentQueries.QueryEntitiesByIds(ids, luceneIndexDirectory);

                Assert.Equal(3, entities.Count());

                //Q26, Q27, Q29
                var doc = entities.ElementAt(0);
                Assert.NotNull(doc);
                Assert.Equal("Q26", doc.Id);
                Assert.Equal("Northern Ireland", doc.Label);

                doc = entities.ElementAt(1);
                Assert.NotNull(doc);
                Assert.Equal("Q27", doc.Id);
                Assert.Equal("Ireland", doc.Label);

                doc = entities.ElementAt(2);
                Assert.NotNull(doc);
                Assert.Equal("Q29", doc.Id);
                Assert.Equal("Spain", doc.Label);
            }
        }

        [Fact]
        public void TestQueryNonExistingEntityById()
        {
            const string outputPath = "Resources/IndexSingle";
            using (var luceneIndexDirectory = FSDirectory.Open(outputPath.GetOrCreateDirectory()))
            {
                var entity = SingleDocumentQueries.QueryEntityById("Q666", luceneIndexDirectory);
                Assert.Null(entity);
            }
        }

        [Fact]
        public void TestQueryNonExistingEntityByLabel()
        {
            const string outputPath = "Resources/IndexSingle";
            using (var luceneIndexDirectory = FSDirectory.Open(outputPath.GetOrCreateDirectory()))

            {
                var entity = SingleDocumentQueries.QueryEntityByLabel("Non-Existing", luceneIndexDirectory);
                Assert.Null(entity);
            }
        }

        [Fact]
        public void TestQueryNonExistingPropertyById()
        {
            const string outputPath = "Resources/IndexSingle";
            using (var luceneIndexDirectory = FSDirectory.Open(outputPath.GetOrCreateDirectory()))

            {
                var entity = SingleDocumentQueries.QueryPropertyById("P666", luceneIndexDirectory);
                Assert.Null(entity);
            }
        }

        [Fact]
        public void TestQueryNonExistingPropertyByLabel()
        {
            const string outputPath = "Resources/IndexSingle";
            using (var luceneIndexDirectory = FSDirectory.Open(outputPath.GetOrCreateDirectory()))

            {
                var entity = SingleDocumentQueries.QueryPropertyByLabel("Non-Existing", luceneIndexDirectory);
                Assert.Null(entity);
            }
        }

        [Fact]
        public void TestQuerySingleInstanceById()
        {
            const string outputPath = "Resources/IndexSingle";
            using (var luceneIndexDirectory = FSDirectory.Open(outputPath.GetOrCreateDirectory()))

            {
                var entity = SingleDocumentQueries.QueryEntityById("Q26", luceneIndexDirectory);
                Assert.NotNull(entity);
                Assert.Equal("Q26", entity.Id);
            }
        }

        [Fact]
        public void TestQuerySingleInstanceByLabel()
        {
            const string outputPath = "Resources/IndexSingle";
            using (var luceneIndexDirectory = FSDirectory.Open(outputPath.GetOrCreateDirectory()))

            {
                var entity = SingleDocumentQueries.QueryEntityByLabel("Northern Ireland", luceneIndexDirectory);
                Assert.NotNull(entity);
                Assert.Equal("Q26", entity.Id);

                entity = SingleDocumentQueries.QueryEntityByLabel("Ireland", luceneIndexDirectory);
                Assert.NotNull(entity);
                Assert.Equal("Q26", entity.Id);

                entity = SingleDocumentQueries.QueryEntityByLabel("Northern", luceneIndexDirectory);
                Assert.NotNull(entity);
                Assert.Equal("Q26", entity.Id);

                entity = SingleDocumentQueries.QueryEntityByLabel("north", luceneIndexDirectory);
                Assert.NotNull(entity);
                Assert.Equal("Q26", entity.Id);
            }
        }

        [Fact]
        public void TestSingleBarackObamaShouldShowFirst()
        {
            const string filename = "Resources/ObamaQueries.nt";
            const string outputPath = "BarackObamaIndex";

            outputPath.DeleteIfExists();

            using (var luceneIndexDirectory = FSDirectory.Open(outputPath.GetOrCreateDirectory()))
            {
                IndexBuilder.CreateEntitiesIndex(filename, luceneIndexDirectory, true);

                var entity = SingleDocumentQueries.QueryEntityByLabel("Obama", luceneIndexDirectory);
                Assert.Equal("Q76", entity.Id);
            }

            outputPath.DeleteIfExists();
        }

        [Fact]
        public void TestSingleMichelleObamaShouldShowFirst()
        {
            const string filename = "Resources/ObamaQueries.nt";
            const string outputPath = "MichelleObamaIndex";

            outputPath.DeleteIfExists();

            using (var luceneIndexDirectory = FSDirectory.Open(outputPath.GetOrCreateDirectory()))
            {
                IndexBuilder.CreateEntitiesIndex(filename, luceneIndexDirectory, true);

                var entity = SingleDocumentQueries.QueryEntityByLabel("Michelle Obama", luceneIndexDirectory);
                Assert.Equal("Q13133", entity.Id);
            }

            outputPath.DeleteIfExists();
        }
    }
}