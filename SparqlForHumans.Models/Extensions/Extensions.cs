using SparqlForHumans.Models.Wikidata;

namespace SparqlForHumans.Models.Extensions
{
    public static class Extensions
    {
        public static string FormatWithUri(this string id, string uri)
        {
            return $"{uri}{id}";
        }

        public static string WikidataUri(this Entity entity)
        {
            return entity.Id.FormatWithUri(WikidataDump.EntityIRI);
        }

        public static string WikidataUri(this Property property)
        {
            return property.Id.FormatWithUri(WikidataDump.PropertyIRI);
        }
    }
}