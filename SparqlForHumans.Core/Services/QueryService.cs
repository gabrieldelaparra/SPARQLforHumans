using System;
using System.Collections.Generic;
using System.Linq;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Store;
using SparqlForHumans.Core.Models;
using SparqlForHumans.Core.Properties;
using SparqlForHumans.Core.Utilities;
using Version = Lucene.Net.Util.Version;

namespace SparqlForHumans.Core.Services
{
    public static class QueryService
    {
        public static Entity AddProperties(this Entity entity, Directory luceneIndexDirectory)
        {
            var propertiesIds = entity.Properties.Select(x => x.Id);
            var properties = MultiDocumentQueries.QueryEntitiesByIds(propertiesIds, luceneIndexDirectory);

            for (var i = 0; i < entity.Properties.Count(); i++)
            {
                var property = entity.Properties.ElementAt(i);
                var prop = properties.FirstOrDefault(x => x.Id.Equals(property.Id));
                property.Label = prop.Label;
            }

            return entity;
        }

       
    }
}