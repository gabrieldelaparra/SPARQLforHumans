﻿using System.Collections.Generic;
using Lucene.Net.Index;
using SparqlForHumans.Lucene.Indexing.Base;
using SparqlForHumans.RDF.Models;

namespace SparqlForHumans.Lucene.Indexing.Indexer
{
    internal interface IIndexer
    {
        IEnumerable<IFieldIndexer<IIndexableField>> RelationMappers { get; set; }
        IEnumerable<IFieldIndexer<IIndexableField>> FieldIndexers { get; set; }
        string InputFilename { get; set; }
        string OutputDirectory { get; set; }
        bool FilterGroups(SubjectGroup tripleGroup);
        void Index();
    }
}