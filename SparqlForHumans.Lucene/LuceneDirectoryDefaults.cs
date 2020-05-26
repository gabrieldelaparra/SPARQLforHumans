using System;
using System.IO;

namespace SparqlForHumans.Lucene
{
    public static class LuceneDirectoryDefaults
    {
        private static string BaseFolder => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            "SparqlForHumans");

        public static string EntityIndexPath => Path.Combine(BaseFolder, "LuceneEntitiesIndex");
        public static string PropertyIndexPath => Path.Combine(BaseFolder, "LucenePropertiesIndex");
    }
}