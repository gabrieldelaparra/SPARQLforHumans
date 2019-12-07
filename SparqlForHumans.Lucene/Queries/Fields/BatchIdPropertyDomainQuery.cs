using SparqlForHumans.Lucene.Extensions;
using SparqlForHumans.Lucene.Queries.Base;
using SparqlForHumans.Lucene.Queries.Parsers;
using SparqlForHumans.Models;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using SparqlForHumans.Utilities;

namespace SparqlForHumans.Lucene.Queries
{
    public class BatchIdPropertyDomainQuery : BasePropertyQuery
    {
        public BatchIdPropertyDomainQuery(string luceneIndexPath, IEnumerable<string> searchStrings) : base(luceneIndexPath, searchStrings, 20) { }

        internal override IQueryParser QueryParser => new DomainQueryParser();
        internal override bool IsInvalidSearchString(string inputString) => string.IsNullOrEmpty(Regex.Replace(inputString, @"[\D]", string.Empty));
        internal override string PrepareSearchTerm(string inputString) => Regex.Replace(inputString.GetUriIdentifier(), @"[\D]", string.Empty);
        public override List<Property> Query(int resultsLimit = 100)
        {
            return GetDocuments().ToProperties().Distinct().OrderByDescending(x => x.Rank).Take(resultsLimit).ToList();
        }
    }
}
