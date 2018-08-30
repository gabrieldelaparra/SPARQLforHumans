using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lucene.Net.Documents;
using SparqlForHumans.Core.Utilities;
using VDS.RDF;

namespace SparqlForHumans.CLI
{
    public class TestBuilderHelper
    {
        public static void GetFirst20ObamaTriplesGroups()
        {
            const string inputFilename = @"filtered-All-2MM.nt";
            var wikidataDumpLines = FileHelper.GetInputLines(inputFilename);
            const int limit = 50;
            var count = 0;
            //Group them by QCode.
            var entityGroups = wikidataDumpLines.GroupBySubject();

            foreach (var group in entityGroups)
            {
                var triples = group.Select(x => x.GetTripleAsTuple());
                var labelTriple = triples.FirstOrDefault(x =>
                    x.predicate.GetPredicateType().Equals(RDFExtensions.PredicateType.Label));

                if(labelTriple.predicate == null) 
                    continue;

                if (!labelTriple.ntObject.IsLiteral() || !labelTriple.ToSafeString().ToUpper().Contains("OBAMA")) 
                    continue;

                foreach (var line in @group)
                {
                    Console.WriteLine(line);
                }

                count++;
                if (count.Equals(limit))
                    return;
            }
        }
    }
}
