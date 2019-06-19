using System.Collections.Generic;
using Lucene.Net.Index;
using SparqlForHumans.Lucene.Indexing.BaseFields;
using SparqlForHumans.Lucene.Relations;
using SparqlForHumans.RDF.Models;

namespace SparqlForHumans.Lucene.Indexing
{
    interface IIndexer<TFieldTypes, TKey, TValue>
        where TFieldTypes : IIndexableField
    {
        IEnumerable<IRelationMapper<IDictionary<TKey, TValue>>> RelationMappers { get; set; }
        IEnumerable<ISubjectGroupIndexer<TFieldTypes>> FieldIndexers { get; set; }
        string InputFilename { get; set; }
        string OutputDirectory { get; set; }
        bool FilterGroups(SubjectGroup tripleGroup);
        void Index();
    }
}
