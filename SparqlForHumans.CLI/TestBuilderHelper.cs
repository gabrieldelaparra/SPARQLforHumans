using System;
using System.Linq;
using SparqlForHumans.Lucene.Utilities;

namespace SparqlForHumans.CLI
{
    public class TestBuilderHelper
    {
        public static void GetFirst20ObamaTriplesGroups()
        {
            const string inputFilename = @"filtered-All-2MM.nt";
            var wikidataDumpLines = FileHelper.GetInputLines(inputFilename);
            const int limit = 40;
            var countObama = 0;
            var countPropObama = 0;
            //Group them by QCode.
            var entityGroups = wikidataDumpLines.GroupBySubject();

            foreach (var group in entityGroups)
            {
                var triples = group.Select(x => x.GetTripleAsTuple());

                var labelTriple = triples.FirstOrDefault(x =>
                    x.predicate.GetPredicateType().Equals(RDFExtensions.PredicateType.Label));

                if (labelTriple.predicate != null)
                    if (labelTriple.ntObject.IsLiteral())
                        if (labelTriple.ntObject.ToString().ToUpper().Contains("OBAMA"))
                            if (!countObama.Equals(limit))
                            {
                                foreach (var line in group) Console.WriteLine(line);
                                countObama++;
                                continue;
                            }

                var obamaTriple = triples.Where(x =>
                    x.predicate.GetPredicateType().Equals(RDFExtensions.PredicateType.Property));

                if (obamaTriple.Any())
                    if (obamaTriple.Any(x => x.ntObject.IsEntityQ() && x.ntObject.GetId().Equals("Q76")))
                        if (!countPropObama.Equals(limit))
                        {
                            foreach (var line in group) Console.WriteLine(line);

                            countPropObama++;
                        }

                if (countObama.Equals(limit) && countPropObama.Equals(limit))
                    return;
            }


            //foreach (var line in @group)
            //{
            //    Console.WriteLine(line);
            //}
            //count++;
            //if (count.Equals(limit))
            //    return;

            //var labelTriple = triples.FirstOrDefault(x =>
            //    x.predicate.GetPredicateType().Equals(RDFExtensions.PredicateType.Label)
            //                  && x.ntObject.IsLiteral()
            //    && x.ntObject.ToString().ToUpper().Contains("OBAMA"));

            //var obamaTriple = triples.FirstOrDefault(x =>
            //    x.predicate.GetPredicateType().Equals(RDFExtensions.PredicateType.Property)
            //    && x.ntObject.GetId().Equals("Q76"));

            //if (labelTriple.predicate == null && obamaTriple.predicate == null)
            //    continue;
        }
    }
}