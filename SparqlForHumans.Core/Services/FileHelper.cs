using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SparqlForHumans.Core.Services
{
    public static class FileHelper
    {
        public static IEnumerable<string> ReadLines(string filename)
        {
            using (StreamReader streamReader = new StreamReader(new FileStream(filename, FileMode.Open)))
                while (!streamReader.EndOfStream)
                {
                    yield return streamReader.ReadLine();
                }
        }
    }
}
