using Lucene.Net.Index;
using Lucene.Net.Store;
using System.IO;

namespace SparqlForHumans.Core.Services
{
    public static class IndexProperties
    {

        public static string entityIRI { get; } = "http://www.wikidata.org/entity/";
        public static string entityPrefix { get; } = "Q";

        public static string propertyIRI { get; } = "http://www.wikidata.org/prop/direct/";

        public static string labelIRI { get; } = "http://www.w3.org/2000/01/rdf-schema#label";
        //public static string prefLabel  { get; }= "http://www.w3.org/2004/02/skos/core#prefLabel";
        //public static string nameIRI  { get; }= "http://schema.org/name";
        public static string alt_labelIRI { get; } = "http://www.w3.org/2004/02/skos/core#altLabel";

        public static string descriptionIRI { get; } = "http://schema.org/description";

        public static string instanceOf { get; } = "P31";
        //public static string image  { get; }= "P18";

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
