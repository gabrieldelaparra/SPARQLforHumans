using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Store;
using SparqlForHumans.Lucene.Extensions;
using SparqlForHumans.Models;
using SparqlForHumans.Models.LuceneIndex;
using SparqlForHumans.Utilities;

namespace SparqlForHumans.Lucene.Queries
{
    public static class SingleDocumentQueries
    {
        public static Entity QueryEntityById(string searchId)
        {
            using (var luceneDirectory = FSDirectory.Open(LuceneIndexExtensions.EntityIndexPath.GetOrCreateDirectory()))
            {
                return QueryDocumentById(searchId, luceneDirectory)?.MapEntity();
            }
        }

        public static Property QueryPropertyById(string searchId)
        {
            using (var luceneDirectory =
                FSDirectory.Open(LuceneIndexExtensions.PropertyIndexPath.GetOrCreateDirectory()))
            {
                return QueryDocumentById(searchId, luceneDirectory)?.MapProperty();
            }
        }

        public static Entity QueryEntityByLabel(string searchText)
        {
            using (var luceneDirectory = FSDirectory.Open(LuceneIndexExtensions.EntityIndexPath.GetOrCreateDirectory()))
            {
                return QueryDocumentByLabel(searchText, luceneDirectory)?.MapEntity();
            }
        }

        public static Property QueryPropertyByLabel(string searchText)
        {
            using (var luceneDirectory =
                FSDirectory.Open(LuceneIndexExtensions.PropertyIndexPath.GetOrCreateDirectory()))
            {
                return QueryDocumentByLabel(searchText, luceneDirectory)?.MapProperty();
            }
        }

        public static Entity QueryEntityById(string searchId, Directory luceneDirectory)
        {
            return QueryDocumentById(searchId, luceneDirectory)?.MapEntity();
        }

        public static Property QueryPropertyById(string searchId, Directory luceneDirectory)
        {
            return QueryDocumentById(searchId, luceneDirectory)?.MapProperty();
        }

        public static Entity QueryEntityByLabel(string searchText, Directory luceneDirectory)
        {
            return QueryDocumentByLabel(searchText, luceneDirectory)?.MapEntity();
        }

        public static Property QueryPropertyByLabel(string searchText, Directory luceneDirectory)
        {
            return QueryDocumentByLabel(searchText, luceneDirectory)?.MapProperty();
        }

        public static Document QueryDocumentById(string searchId, Directory luceneDirectory)
        {
            // NotEmpty Validation
            if (string.IsNullOrEmpty(searchId))
            {
                return null;
            }

            var document = new Document();

            using (var luceneDirectoryReader = DirectoryReader.Open(luceneDirectory))
            {
                var searcher = new IndexSearcher(luceneDirectoryReader);
                document = BaseParser.QueryDocumentByIdAndRank(searchId, searcher);
            }

            return document;
        }

        public static Document QueryDocumentByLabel(string searchText, Directory luceneDirectory,
            bool isType = false)
        {
            if (string.IsNullOrEmpty(searchText))
            {
                return null;
            }

            searchText = BaseParser.PrepareSearchTerm(searchText);

            // NotEmpty Validation
            if (string.IsNullOrEmpty(searchText.Replace("*", "").Replace("?", "")))
            {
                return null;
            }

            Filter filter = null;
            if (isType)
            {
                filter = new PrefixFilter(new Term(Labels.IsTypeEntity.ToString(), "true"));
            }

            var document = new Document();

            using (var luceneDirectoryReader = DirectoryReader.Open(luceneDirectory))
            {
                var searcher = new IndexSearcher(luceneDirectoryReader);
                var parser = BaseParser.GetMultiFieldParser();
                document = BaseParser.QueryDocumentByRank(searchText, searcher, parser, filter);
            }

            return document;
        }
    }
}