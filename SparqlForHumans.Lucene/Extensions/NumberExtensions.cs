using System;

namespace SparqlForHumans.Lucene.Extensions
{
    public static class NumberExtensions
    {
        public static double ToThreeDecimals(this double input)
        {
            return Math.Truncate(input * 1000) / 1000;
        }
    }
}