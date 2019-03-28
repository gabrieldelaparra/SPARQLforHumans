using SparqlForHumans.Utilities;
using Xunit;

namespace SparqlForHumans.UnitTests.Utilities
{
    public class StringExtensionsTests
    {
        [Fact]
        public void TestToIntMix()
        {
            const string sample = "a1a1";
            const int expected = 11;
            Assert.Equal(expected, sample.ToInt());
        }

        [Fact]
        public void TestToIntOnlyLetters()
        {
            const string sample = "abcs";
            const int expected = 0;
            Assert.Equal(expected, sample.ToInt());
        }

        [Fact]
        public void TestToIntOnlyNumbers()
        {
            const string sample = "1234";
            const int expected = 1234;
            Assert.Equal(expected, sample.ToInt());
        }

        [Fact]
        public void TestToIntNegativeNumbers()
        {
            const string sample = "-1234";
            const int expected = -1234;
            Assert.Equal(expected, sample.ToInt());
        }
    }
}