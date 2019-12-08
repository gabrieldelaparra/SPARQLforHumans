using System.Collections.Generic;
using System.Linq;
using SparqlForHumans.Models;
using SparqlForHumans.Utilities;

namespace SparqlForHumans.Lucene.Queries.Fields
{
    public class IntersectEntityPropertiesQuery
    {
        private BatchIdEntityPropertiesQuery queryHandler;
        private string luceneIndexPath;
        private IEnumerable<string> searchStrings;
        private int lotResultsLimit;
        public IntersectEntityPropertiesQuery(string luceneIndexPath, IEnumerable<string> searchStrings, int lotResultsLimit = 100)
        {
            this.luceneIndexPath = luceneIndexPath;
            this.searchStrings = searchStrings;
            this.lotResultsLimit = lotResultsLimit;
        }

        public List<Entity> Query(int totalResultsLimit = 100)
        {
            var results = new List<Entity>();
            foreach (var searchString in searchStrings) {
                queryHandler = new BatchIdEntityPropertiesQuery(luceneIndexPath, new []{searchString}, lotResultsLimit);
                var queryResults = queryHandler.Query();
                results = results.IntersectIfAny(queryResults).ToList();
            }
            return results.OrderByDescending(x => x.Rank).Take(totalResultsLimit).ToList();
        }
    }

    public class IntersectEntityReversePropertiesQuery
    {
        private BatchIdEntityReversePropertiesQuery queryHandler;
        private string luceneIndexPath;
        private IEnumerable<string> searchStrings;
        private int lotResultsLimit;
        public IntersectEntityReversePropertiesQuery(string luceneIndexPath, IEnumerable<string> searchStrings, int lotResultsLimit = 100)
        {
            this.luceneIndexPath = luceneIndexPath;
            this.searchStrings = searchStrings;
            this.lotResultsLimit = lotResultsLimit;
        }

        public List<Entity> Query(int totalResultsLimit = 100)
        {
            var results = new List<Entity>();
            foreach (var searchString in searchStrings) {
                queryHandler = new BatchIdEntityReversePropertiesQuery(luceneIndexPath, new []{searchString}, lotResultsLimit);
                var queryResults = queryHandler.Query();
                results = results.IntersectIfAny(queryResults).ToList();
            }
            return results.OrderByDescending(x => x.Rank).Take(totalResultsLimit).ToList();
        }
    }
}
