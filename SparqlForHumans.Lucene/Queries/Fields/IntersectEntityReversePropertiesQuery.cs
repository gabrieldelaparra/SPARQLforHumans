//using System.Collections.Generic;
//using System.Linq;
//using SparqlForHumans.Models;
//using SparqlForHumans.Utilities;

//namespace SparqlForHumans.Lucene.Queries.Fields
//{
//    public class IntersectEntityReversePropertiesQuery
//    {
//        private readonly int lotResultsLimit;
//        private readonly string luceneIndexPath;
//        private BatchIdEntityReversePropertiesQuery queryHandler;
//        private readonly IEnumerable<string> searchStrings;

//        public IntersectEntityReversePropertiesQuery(string luceneIndexPath, IEnumerable<string> searchStrings,
//                                                     int lotResultsLimit = 100)
//        {
//            this.luceneIndexPath = luceneIndexPath;
//            this.searchStrings = searchStrings;
//            this.lotResultsLimit = lotResultsLimit;
//        }

//        public List<Entity> Query(int totalResultsLimit = 100)
//        {
//            var results = new List<Entity>();
//            foreach (var searchString in searchStrings) {
//                queryHandler =
//                    new BatchIdEntityReversePropertiesQuery(luceneIndexPath, new[] {searchString}, lotResultsLimit);
//                var queryResults = queryHandler.Query();
//                results = results.IntersectIfAny(queryResults).ToList();
//            }

//            return results.OrderByDescending(x => x.Rank).Take(totalResultsLimit).ToList();
//        }
//    }
//}

