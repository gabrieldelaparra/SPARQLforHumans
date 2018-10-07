using System;

namespace SparqlForHumans.Core.Utilities
{
    public static class LuceneIndexExtensions
    {
        public static string EntityIndexPath =>
            $@"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}\DCC\SparqlForHumans\LuceneEntitiesIndex";

        public static string PropertyIndexPath =>
            $@"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}\DCC\SparqlForHumans\LucenePropertiesIndex";
    }
}