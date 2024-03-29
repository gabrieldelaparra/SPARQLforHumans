﻿using System.Collections.Generic;
using Lucene.Net.Index;
using Lucene.Net.Search;
using SparqlForHumans.Lucene.Extensions;
using SparqlForHumans.Models;
using SparqlForHumans.Models.LuceneIndex;

namespace SparqlForHumans.Lucene.Queries.Base
{
    public abstract class BaseTypeQuery : BaseQuery<Entity>
    {
        public BaseTypeQuery(string luceneIndexPath, IEnumerable<string> searchStrings, int resultsLimit = 1) : base(
            luceneIndexPath, searchStrings, resultsLimit) { }

        public BaseTypeQuery(string luceneIndexPath, string searchString, int resultsLimit = 1) : base(luceneIndexPath,
            searchString, resultsLimit) { }

        internal override Filter Filter => new PrefixFilter(new Term(Labels.IsTypeEntity.ToString(), true.ToString()));

        public override IEnumerable<Entity> Query(int totalResultsLimit = 100)
        {
            return GetDocuments().ToEntities();
        }
    }
}