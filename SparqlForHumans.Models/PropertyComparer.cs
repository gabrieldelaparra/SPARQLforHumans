using System.Collections.Generic;

namespace SparqlForHumans.Models
{
    public class PropertyComparer : IEqualityComparer<Property>
    {
        public bool Equals(Property x, Property y)
        {
            if (x == null || y == null) return false;
            return x.Id == y.Id;
        }

        public int GetHashCode(Property obj)
        {
            return obj.Id.GetHashCode();
        }
    }
}