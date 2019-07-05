using System;

namespace SparqlForHumans.Lucene
{
    public static class LuceneDirectoryDefaults
    {
        //TODO: What is the difference between readonly and {get;} only?

        private static string BaseFolder => $@"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}\DCC\SparqlForHumans";
        public static string EntityIndexPath => $@"{BaseFolder}\LuceneEntitiesIndex";
        public static string PropertyIndexPath => $@"{BaseFolder}\LucenePropertiesIndex";
    }
}