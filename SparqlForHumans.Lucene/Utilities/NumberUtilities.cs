using System;

namespace SparqlForHumans.Lucene.Utilities
{
    public static class NumberUtilities
    {
        public static double ToThreeDecimals(this double input)
        {
            return Math.Truncate(input * 1000) / 1000;
        }
    }
}