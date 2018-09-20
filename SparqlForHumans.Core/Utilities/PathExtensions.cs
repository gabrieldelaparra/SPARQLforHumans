using System.IO;

namespace SparqlForHumans.Core.Utilities
{
    public static class PathExtensions
    {
        public static void DeleteIfExists(this string path, bool additionalCondition = false)
        {
            if (Directory.Exists(path) && additionalCondition)
                Directory.Delete(path, true);
        }
    }
}
