using SparqlForHumans.Lucene.Index;
using SparqlForHumans.Utilities;

namespace SparqlForHumans.UnitTests
{
    public static class TestHelper
    {
        private static int sequenceIndex = 0;
        public static (string EntitiesIndexPath, string PropertiesIndexPath) CreateIndexPaths(string filename, string baseEntitiesIndexPath, string basePropertiesIndexPath)
        {
            var entitiesIndexPath = $"{baseEntitiesIndexPath}_{sequenceIndex}";
            var propertiesIndexPath = $"{basePropertiesIndexPath}_{sequenceIndex++}";
            entitiesIndexPath.DeleteIfExists();
            propertiesIndexPath.DeleteIfExists();
            new EntitiesIndexer(filename, entitiesIndexPath).Index();
            new PropertiesIndexer(filename, propertiesIndexPath, entitiesIndexPath).Index();
            return (entitiesIndexPath, propertiesIndexPath);
        }
    }
}