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

        [Fact]
        public void TestToDoubleMix()
        {
            const string sample = "a1a1";
            const double expected = 11;
            Assert.Equal(expected, sample.ToDouble());
        }

        [Fact]
        public void TestToDoubleOnlyLetters()
        {
            const string sample = "abcs";
            const double expected = 0;
            Assert.Equal(expected, sample.ToDouble());
        }

        [Fact]
        public void TestToDoubleOnlyNumbers()
        {
            const string sample = "1234";
            const double expected = 1234;
            Assert.Equal(expected, sample.ToDouble());
        }

        [Fact]
        public void TestToDoubleNegativeNumbers()
        {
            const string sample = "-1234";
            const double expected = -1234;
            Assert.Equal(expected, sample.ToDouble());
        }

        [Fact]
        public void TestToDoublePositiveExponential()
        {
            const string sample = "1234E05";
            const double expected = 1234E05;
            Assert.Equal(expected, sample.ToDouble());
        }

        [Fact]
        public void TestToDoubleNegativeExponential()
        {
            const string sample = "1234E-05";
            const double expected = 1234E-05;
            Assert.Equal(expected, sample.ToDouble());
        }
    }
}