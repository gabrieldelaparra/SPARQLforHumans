﻿using System.Collections.Generic;
using System.Linq;

namespace SparqlForHumans.Models.Query
{
    public class Node
    {
        public int id { get; set; }

        public string name { get; set; }

        public string[] uris { get; set; } = new string[0];

        public override bool Equals(object obj)
        {
            var y = obj as Node;
            if (y == null) return false;
            if (ReferenceEquals(this, y)) return true;
            if (ReferenceEquals(this, null) || ReferenceEquals(y, null)) return false;
            return this.id.Equals(y.id) && this.uris.SequenceEqual(y.uris);
        }

        public override int GetHashCode()
        {
            return this.id.GetHashCode() ^ this.uris.GetHashCode();
        }
    }
}
