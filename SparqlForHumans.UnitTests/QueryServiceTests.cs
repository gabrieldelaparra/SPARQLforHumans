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
        public void TestQuerySingleInstance()
        {
            const string outputPath = "Resources/IndexSingle";
            var luceneIndexDirectory = outputPath.GetLuceneDirectory();

            //QueryService.QueryEntityByLabel()
        }
    }
}
