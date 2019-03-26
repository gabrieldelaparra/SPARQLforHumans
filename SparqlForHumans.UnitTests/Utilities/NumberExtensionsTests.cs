using SparqlForHumans.Utilities;
using Xunit;

namespace SparqlForHumans.UnitTests.Utilities
{
    public class NumberExtensionsTests
    {
        [Fact]
        public void TestToThreeDecimalsLarger()
        {
            const double dec = 1.23456789;
            const double expected = 1.234;
            Assert.Equal(expected, dec.ToThreeDecimals());
        }

        [Fact]
        public void TestToThreeDecimalsShorter()
        {
            const double dec = 1.2;
            const double expected = 1.200;
            Assert.Equal(expected, dec.ToThreeDecimals());
        }

        [Fact]
        public void TestToThreeDecimalsZero()
        {
            const double dec = 0.0;
            const double expected = 0.000;
            Assert.Equal(expected, dec.ToThreeDecimals());
        }
    }
}