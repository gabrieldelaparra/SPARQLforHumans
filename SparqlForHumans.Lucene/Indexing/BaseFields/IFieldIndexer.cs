using System.Collections.Generic;
using Lucene.Net.Documents;
using VDS.RDF;

namespace SparqlForHumans.Lucene.Indexing.BaseFields
{
    internal interface IFieldIndexer<T1> where T1 : Field
    {
        string FieldName { get; }
        T1 TriplesToField(IEnumerable<Triple> tripleGroup);
    }
}