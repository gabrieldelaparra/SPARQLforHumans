using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Store;
using SparqlForHumans.Lucene.Extensions;
using SparqlForHumans.Models;
using System.Collections.Generic;
using System.Linq;

namespace SparqlForHumans.Lucene.Queries
{
    public class MultiDocumentQueries
    {
        public static IEnumerable<Entity> QueryEntitiesByIds(IEnumerable<string> searchIds, Directory luceneDirectory)
        {
            return QueryDocumentsByIds(searchIds, luceneDirectory)?.Select(x => x.MapEntity());
        }

        public static IEnumerable<Property> QueryPropertiesByIds(IEnumerable<string> searchIds,
            Directory luceneDirectory)
        {
            return QueryDocumentsByIds(searchIds, luceneDirectory)?.Select(x => x.MapProperty());
        }

        public static IEnumerable<Document> QueryDocumentsByIds(IEnumerable<string> searchIds,
            Directory luceneDirectory)
        {
            var documents = new List<Document>();

            // NotEmpty Validation
            if (searchIds == null)
            {
                return documents;
            }

            using (var luceneDirectoryReader = DirectoryReader.Open(luceneDirectory))
            {
                var searcher = new IndexSearcher(luceneDirectoryReader);
                foreach (var searchText in searchIds)
                {
                    documents.Add(BaseParser.QueryDocumentByIdAndRank(searchText, searcher));
                }
            }

            return documents;
        }
    }
}