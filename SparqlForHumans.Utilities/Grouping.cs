using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SparqlForHumans.Utilities
{
    public class Grouping<TKey, TElement> : IGrouping<TKey, TElement>
    {
        private readonly IEnumerable<TElement> _elements;

        public Grouping(TKey key, IEnumerable<TElement> elements)
        {
            Key = key;
            _elements = elements ?? throw new ArgumentNullException(nameof(elements));
        }

        public TKey Key { get; }

        public IEnumerator<TElement> GetEnumerator()
        {
            return _elements.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}