using System;
using System.Collections.Generic;
using System.Text;
using SparqlForHumans.Core.Services;
using SparqlForHumans.Core.Utilities;
using Xunit;

namespace SparqlForHumans.UnitTests
{
    public class QueryServiceTests
    {
        [Fact]
        public void TestQuerySingleInstanceByLabel()
        {
            const string outputPath = "Resources/IndexSingle";
            var luceneIndexDirectory = outputPath.GetLuceneDirectory();

            var entity = QueryService.QueryEntityByLabel("Northern Ireland", luceneIndexDirectory);
            Assert.NotNull(entity);
            Assert.Equal("Q26", entity.Id);

            entity = QueryService.QueryEntityByLabel("Ireland", luceneIndexDirectory);
            Assert.NotNull(entity);
            Assert.Equal("Q26", entity.Id);

            entity = QueryService.QueryEntityByLabel("Northern", luceneIndexDirectory);
            Assert.NotNull(entity);
            Assert.Equal("Q26", entity.Id);

            entity = QueryService.QueryEntityByLabel("north", luceneIndexDirectory);
            Assert.NotNull(entity);
            Assert.Equal("Q26", entity.Id);
        }

        [Fact]
        public void TestQuerySingleInstanceById()
        {
            const string outputPath = "Resources/IndexSingle";
            var luceneIndexDirectory = outputPath.GetLuceneDirectory();

            var entity = QueryService.QueryEntityById("Q26", luceneIndexDirectory);
            Assert.NotNull(entity);
            Assert.Equal("Q26", entity.Id);
        }
    }
}
