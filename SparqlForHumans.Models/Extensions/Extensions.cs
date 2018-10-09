namespace SparqlForHumans.Models.Extensions
{
    public static class Extensions
    {
        public static string Uri(this Entity entity)
        {
            return $"<{Wikidata.WikidataDump.EntityIRI}{entity.Id}>";
        }

        public static string Uri(this Property property)
        {
            return $"<{Wikidata.WikidataDump.PropertyIRI}{property.Id}>";
        }
    }
}
