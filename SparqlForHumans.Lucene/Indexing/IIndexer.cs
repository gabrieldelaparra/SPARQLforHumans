using System;
using System.Collections.Generic;
using System.Text;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Store;
using SparqlForHumans.Lucene.Indexing.BaseFields;
using SparqlForHumans.Lucene.Relations;

namespace SparqlForHumans.Lucene.Indexing
{
    interface IIndexer
    {
        IList<IRelationMapper> RelationMappers { get; set; }
        IList<IFieldIndexer<Field>> Fields { get; set; }
        string Filename { get; set; }
        Directory Directory { get; set; }
    }
}
