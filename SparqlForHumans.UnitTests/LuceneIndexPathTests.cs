using SparqlForHumans.Core.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xunit;

namespace SparqlForHumans.UnitTests
{
    public class LuceneIndexPathTests
    {
        [Fact]
        public void TestGetLucenePath()
        {
            var path = "Temp";
            var luceneDirectory = Paths.GetLuceneDirectory(path);
            Assert.NotNull(luceneDirectory);
            Assert.IsAssignableFrom<Lucene.Net.Store.Directory>(luceneDirectory);

            Assert.True(Directory.Exists(path));
        }
    }
}
