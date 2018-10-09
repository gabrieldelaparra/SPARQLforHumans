using System;
using System.Collections.Generic;
using System.Text;
using SparqlForHumans.Models;
using SparqlForHumans.Models.RDFQuery;
using SparqlForHumans.RDF.Queries;
using Xunit;

namespace SparqlForHumans.UnitTests
{
    public class RDFQueryBuilderTests
    {
        [Fact]
        public void TestGetLabelQuery()
        {
            var entity = new RDFEntity()
            {
                Id = "Q25",
            };
            var property = new RDFProperty()
            {
                Id = "P31",
            };
            var rdfQuery = new QueryTriple()
            {
                Subject = entity,
                Predicate = property,
                Object = new RDFLabel()
                {
                    Label = "instanceOf",
                },
            };
            var query = rdfQuery.BuildQuery();

            Assert.Contains("SELECT ?instanceOf", query.ToString());
            Assert.Contains("WHERE", query.ToString());
            Assert.Contains("<http://www.wikidata.org/entity/Q25> <http://www.wikidata.org/prop/direct/P31> ?instanceOf .", query.ToString());

        }
    }
}
