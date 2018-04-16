using SparqlForHumans.Core.Utilities;
using System;

namespace SparqlForHumans.CLI
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            var lines = GZipHandler.ReadGZip(@"C:\Users\admin\Desktop\DCC\latest-truthy.nt-gz\latest-truthy.nt.gz");

            foreach (var item in lines)
            {
                Console.WriteLine(item);
            }
            Console.ReadLine();
        }
    }
}
