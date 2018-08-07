using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using SparqlForHumans.Core.Services;
using Xunit;

namespace SparqlForHumans.UnitTests
{
    public class PropertyFrequencyTests
    {
        [Fact]
        public void TestGetFileFrequency()
        {
            var filename = @"Resources/PropertyFrequencies.nt";
            Assert.True(File.Exists(filename));

            var dictionary = PropertiesFrequency.GetPropertiesFrequency(filename);
            
            Assert.NotNull(dictionary);

            //Assert.Equal(3, dictionary.Count);

            //Assert.Equal("P17", dictionary.ElementAt(0).Key);
            //Assert.Equal(4, dictionary.ElementAt(0).Value);

        }
    }
}
