using Lucene.Net.Index;
using Lucene.Net.Search;
using SparqlForHumans.Lucene.Extensions;
using SparqlForHumans.Models;
using SparqlForHumans.Models.LuceneIndex;
using System.Collections.Generic;

namespace SparqlForHumans.Lucene.Queries.Base
{
    public abstract class BaseEntityQuery : BaseQuery<Entity>
    {
        public BaseEntityQuery(string luceneIndexPath, IEnumerable<string> searchStrings, int resultsLimit = 1) : base(luceneIndexPath, searchStrings, resultsLimit) { }

        public BaseEntityQuery(string luceneIndexPath, string searchString, int resultsLimit = 1) : base(luceneIndexPath, searchString, resultsLimit) { }

        public override List<Entity> Query()
        {
            return GetDocuments().ToEntities();
        }
    }

    public abstract class BaseTypeQuery : BaseQuery<Entity>
    {
        public BaseTypeQuery(string luceneIndexPath, IEnumerable<string> searchStrings, int resultsLimit = 1) : base(luceneIndexPath, searchStrings, resultsLimit) { }

        public BaseTypeQuery(string luceneIndexPath, string searchString, int resultsLimit = 1) : base(luceneIndexPath, searchString, resultsLimit) { }

        public override List<Entity> Query()
        {
            return GetDocuments().ToEntities();
        }
        internal override Filter Filter => new PrefixFilter(new Term(Labels.IsTypeEntity.ToString(), true.ToString()));
    }
}
