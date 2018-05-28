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

        public static int GetDocumentCount(this Lucene.Net.Store.Directory luceneIndexDirectory)
        {
            using (var reader = IndexReader.Open(luceneIndexDirectory, readOnly: true))
            {
                return reader.NumDocs();
            }
        }

        public static bool HasRank(this Lucene.Net.Store.Directory luceneIndexDirectory)
        {
            using (var reader = IndexReader.Open(luceneIndexDirectory, readOnly: true))
            {
                var docCount = reader.NumDocs();
                for (int i = 0; i < docCount; i++)
                {
                    var document = reader.Document(i);
                    if (document.Boost.Equals(1.0))
                        continue;
                    else
                        return false;
                }
            }
            return true;
        }
    }
}
