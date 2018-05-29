using System.IO;
using Lucene.Net.Index;
using Lucene.Net.Store;
using Directory = Lucene.Net.Store.Directory;

namespace SparqlForHumans.Core.Utilities
{
    public static class LuceneHelper
    {
        private static readonly string indexPath = @"../LuceneIndex";

        public static Directory LuceneIndexDirectory => GetLuceneDirectory(indexPath);

        public static Directory GetLuceneDirectory(string directoryPath)
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

        public static int GetDocumentCount(this Directory luceneIndexDirectory)
        {
            using (var reader = IndexReader.Open(luceneIndexDirectory, true))
            {
                return reader.NumDocs();
            }
        }

        public static bool HasRank(this Directory luceneIndexDirectory)
        {
            using (var reader = IndexReader.Open(luceneIndexDirectory, true))
            {
                var docCount = reader.NumDocs();
                for (var i = 0; i < docCount; i++)
                {
                    var document = reader.Document(i);
                    if (document.Boost.Equals(1))
                        continue;
                    return true;
                }
            }

            return false;
        }
    }
}