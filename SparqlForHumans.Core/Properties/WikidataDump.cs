namespace SparqlForHumans.Core.Properties
{
    public static class WikidataDump
    {
        public static string EntityIRI { get; } = "http://www.wikidata.org/entity/";
        public static string EntityPrefix { get; } = "Q";

        public static string PropertyIRI { get; } = "http://www.wikidata.org/prop/direct/";
        public static string PropertyPrefix { get; } = "P";

        public static string LabelIRI { get; } = "http://www.w3.org/2000/01/rdf-schema#label";

        //public static string prefLabel  { get; }= "http://www.w3.org/2004/02/skos/core#prefLabel";
        //public static string nameIRI  { get; }= "http://schema.org/name";
        public static string Alt_labelIRI { get; } = "http://www.w3.org/2004/02/skos/core#altLabel";

        public static string DescriptionIRI { get; } = "http://schema.org/description";

        public static string InstanceOf { get; } = "P31";

        public static string PropertyValueSeparator { get; } = "##";

        //public static string image  { get; }= "P18";
    }
}