using System.Collections.Generic;
using Lucene.Net.Index;
using SparqlForHumans.RDF.Models;

namespace SparqlForHumans.Lucene.Index.Base
{
    internal interface IIndexer
    {
        IEnumerable<IFieldIndexer<IIndexableField>> FieldIndexers { get; set; }
        string InputFilename { get; set; }
        string OutputDirectory { get; set; }
        IEnumerable<IFieldIndexer<IIndexableField>> RelationMappers { get; set; }
        bool FilterGroups(SubjectGroup tripleGroup);
        void Index();
    }
}