using System.IO;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Store;
using Directory = Lucene.Net.Store.Directory;

namespace SparqlForHumans.Core.Utilities
{
    public static class LuceneIndexExtensions
    {
        public static string IndexPath => @"../LuceneIndex";

        public static Directory LuceneIndexDirectory => IndexPath.GetLuceneDirectory();

        public static Directory GetLuceneDirectory(this string directoryPath)
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
    }
}