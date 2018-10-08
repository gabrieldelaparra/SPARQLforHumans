using System.IO;

namespace SparqlForHumans.Lucene.Utilities
{
    public static class PathExtensions
    {
        public static void DeleteIfExists(this string path, bool additionalCondition = true)
        {
            if (Directory.Exists(path) && additionalCondition)
                Directory.Delete(path, true);

            if (File.Exists(path) && additionalCondition)
                File.Delete(path);
        }
    }
}