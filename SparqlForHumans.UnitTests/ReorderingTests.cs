using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using SparqlForHumans.RDF.Reordering;
using SparqlForHumans.Utilities;
using Xunit;

namespace SparqlForHumans.UnitTests
{
    public class ReorderingTests
    {
        [Fact]
        public void TestReorder()
        {
            const string filename = "Resources/Filter5k-Index.nt";
            Assert.True(File.Exists(filename));
            var outputFilename = FileHelper.GetReorderedOutputFilename(filename);

            outputFilename.DeleteIfExists();

            Assert.False(File.Exists(outputFilename));

            TriplesReordering.Reorder(filename, outputFilename);

            Assert.True(File.Exists(outputFilename));
            var reorderedLines = FileHelper.ReadLines(outputFilename);

            Assert.Equal(3511,reorderedLines.Count());

            outputFilename.DeleteIfExists();
        }
    }
}
