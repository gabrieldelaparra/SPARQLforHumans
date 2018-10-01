using System.Linq;
using Lucene.Net.Store;
using SparqlForHumans.Models;

namespace SparqlForHumans.Core.Services
{
    public static class QueryService
    {
        public static Entity AddProperties(this Entity entity, Directory luceneDirectory)
        {
            var propertiesIds = entity.Properties.Select(x => x.Id);
            var properties = MultiDocumentQueries.QueryEntitiesByIds(propertiesIds, luceneDirectory);

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