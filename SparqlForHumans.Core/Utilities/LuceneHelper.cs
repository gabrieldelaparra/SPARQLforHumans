using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Store;
using System.IO;

namespace SparqlForHumans.Core.Utilities
{
    public static class LuceneHelper
    {
        private static readonly string indexPath = @"../LuceneIndex";

        public static Lucene.Net.Store.Directory LuceneIndexDirectory => GetLuceneDirectory(indexPath);

        public static Lucene.Net.Store.Directory GetLuceneDirectory(string directoryPath)
        {
            var directoryInfo = FileHelper.GetOrCreateDirectory(directoryPath);

            var luceneIndexDirectory = FSDirectory.Open(directoryInfo);

            if (IndexWriter.IsLocked(luceneIndexDirectory))
                IndexWriter.Unlock(luceneIndexDirectory);

            var lockFilePath = Path.Combine(directoryPath, "write.lock");

            if (File.Exists(lockFilePath))
                File.Delete(lockFilePath);

            return luceneIndexDirectory;
        }

        public static int GetDocumentCount(Lucene.Net.Store.Directory luceneIndexDirectory)
        {
            using (var searcher = new IndexSearcher(luceneIndexDirectory, readOnly: true))
            {
                return searcher.MaxDoc;
            }
        }
    }
}
