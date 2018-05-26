using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace SparqlForHumans.Core.Services
{
    public class GZipHandler
    {
        public static IEnumerable<string> ReadGZip(string filename)
        {
            var fileToDecompress = new FileInfo(filename);
            if (!fileToDecompress.Exists) yield break;

            using (var originalFileStream = fileToDecompress.OpenRead())
            using (var zip = new GZipStream(originalFileStream, CompressionMode.Decompress))
            using (StreamReader unzip = new StreamReader(zip))
                while (!unzip.EndOfStream)
                {
                    yield return unzip.ReadLine();
                }
        }

        
    }
}
