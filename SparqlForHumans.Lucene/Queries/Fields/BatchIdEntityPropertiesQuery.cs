﻿using System.Collections.Generic;
using System.Linq;
using SparqlForHumans.Lucene.Extensions;
using SparqlForHumans.Lucene.Queries.Base;
using SparqlForHumans.Lucene.Queries.Parsers;
using SparqlForHumans.Models;

namespace SparqlForHumans.Lucene.Queries {
    public class BatchIdEntityPropertiesQuery : BaseEntityQuery
    {
        public BatchIdEntityPropertiesQuery(string luceneIndexPath, IEnumerable<string> searchStrings, int resultsLimit = 20) : base(luceneIndexPath, searchStrings, resultsLimit) { }

        internal override IQueryParser QueryParser => new EntityPropertiesQueryParser();

        internal override bool IsInvalidSearchString(string inputString) => string.IsNullOrEmpty(inputString);
        public override List<Entity> Query(int totalResultsLimit = 100)
        {
            return GetDocuments().ToEntities().Distinct().OrderByDescending(x => x.Rank).Take(totalResultsLimit).ToList();
        }
    }

    public class BatchIdEntityReversePropertiesQuery : BaseEntityQuery
    {
        public BatchIdEntityReversePropertiesQuery(string luceneIndexPath, IEnumerable<string> searchStrings, int resultsLimit = 20) : base(luceneIndexPath, searchStrings, resultsLimit) { }

        internal override IQueryParser QueryParser => new EntityPropertiesQueryParser();

        internal override bool IsInvalidSearchString(string inputString) => string.IsNullOrEmpty(inputString);
        public override List<Entity> Query(int totalResultsLimit = 100)
        {
            return GetDocuments().ToEntities().Distinct().OrderByDescending(x => x.Rank).Take(totalResultsLimit).ToList();
        }
    }
}