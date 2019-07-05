using SparqlForHumans.Models.Wikidata;

namespace SparqlForHumans.Models.Extensions
{
    public static class WikidataUriExtensions
    {
        public static string FormatWithUri(this string id, string uri)
        {
            return $"{uri}{id}";
        }

        public static string WikidataUri(this IEntity entity)
        {
            return entity.Id.FormatWithUri(WikidataDump.EntityIRI);
        }

        public static string WikidataUri(this IProperty property)
        {
            return property.Id.FormatWithUri(WikidataDump.PropertyIRI);
        }
    }
}