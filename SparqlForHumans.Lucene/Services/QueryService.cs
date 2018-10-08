using System.Linq;
using Lucene.Net.Store;
using SparqlForHumans.Lucene.Services.Query;
using SparqlForHumans.Lucene.Utilities;
using SparqlForHumans.Models;

namespace SparqlForHumans.Lucene.Services
{
    public static class QueryService
    {
        public static Entity AddProperties(this Entity entity)
        {
            using (var propertiesDirectory =
                FSDirectory.Open(LuceneIndexExtensions.PropertyIndexPath.GetOrCreateDirectory()))
            {
                return AddProperties(entity, propertiesDirectory);
            }
        }

        public static Entity AddProperties(this Entity entity, Directory luceneDirectory)
        {
            var propertiesIds = entity.Properties.Select(x => x.Id);
            var properties = MultiDocumentQueries.QueryPropertiesByIds(propertiesIds, luceneDirectory);

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