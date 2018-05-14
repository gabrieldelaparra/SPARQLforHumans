using Lucene.Net.Index;
using Lucene.Net.Store;
using System.IO;

namespace SparqlForHumans.Core.Properties
{
    public static class Paths
    {
        public static string indexPath = @"../LuceneIndex";

        private static Lucene.Net.Store.Directory luceneIndexDirectory;
        public static Lucene.Net.Store.Directory LuceneIndexDirectory
        {
            get
            {
                if (luceneIndexDirectory == null) luceneIndexDirectory = FSDirectory.Open(new DirectoryInfo(indexPath));
                if (IndexWriter.IsLocked(luceneIndexDirectory)) IndexWriter.Unlock(luceneIndexDirectory);
                var lockFilePath = Path.Combine(indexPath, "write.lock");
                if (File.Exists(lockFilePath)) File.Delete(lockFilePath);
                return luceneIndexDirectory;
            }
        }
    }
}
