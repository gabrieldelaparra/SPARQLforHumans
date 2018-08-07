using System;
using System.Collections.Generic;
using System.Text;

namespace SparqlForHumans.Core.Services
{
    public static class NumberHelper
    {
        public static double ToThreeDecimals(this double input)
        {
            return Math.Truncate(input * 1000) / 1000;
        }
    }
}
