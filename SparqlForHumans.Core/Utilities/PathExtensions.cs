using System.IO;

namespace SparqlForHumans.Core.Utilities
{
    public static class PathExtensions
    {
        public static void DeleteIfExists(this string path)
        {
            if (Directory.Exists(path))
                Directory.Delete(path, true);
        }
    }
}
