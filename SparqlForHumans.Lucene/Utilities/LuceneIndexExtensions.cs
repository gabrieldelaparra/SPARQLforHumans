using Lucene.Net.Index;
using Lucene.Net.Store;

namespace SparqlForHumans.Core.Utilities
{
    public static class LuceneIndexExtensions
    {
        public static string EntityIndexPath => @"C:\Users\delapa\Desktop\DCC\SparQLforHumans\LuceneEntitiesIndex";
        public static string PropertyIndexPath => @"C:\Users\delapa\Desktop\DCC\SparQLforHumans\LucenePropertiesIndex";

        //public static Directory EntitiesIndexDirectory => EntityIndexPath.GetLuceneDirectory();
        //public static Directory PropertiesIndexDirectory => PropertyIndexPath.GetLuceneDirectory();
        //public static Directory TypesIndexDirectory => EntityIndexPath.GetLuceneDirectory();

        //public static Directory GetLuceneDirectory(this string directoryPath)
        //{
        //    var directoryInfo = FileHelper.GetOrCreateDirectory(directoryPath);

        //    var luceneIndexDirectory = FSDirectory.Open(directoryInfo);

        //    if (!IndexWriter.IsLocked(luceneIndexDirectory))
        //        return luceneIndexDirectory;

        //    IndexWriter.Unlock(luceneIndexDirectory);

        //    var lockFilePath = Path.Combine(directoryPath, "write.lock");

        //    if (File.Exists(lockFilePath))
        //        File.Delete(lockFilePath);

        //    return luceneIndexDirectory;
        //}

        public static int GetDocumentCount(this Directory luceneIndexDirectory)
        {
            using (var reader = DirectoryReader.Open(luceneIndexDirectory))
            {
                return reader.MaxDoc;
            } 
        }
    }
}