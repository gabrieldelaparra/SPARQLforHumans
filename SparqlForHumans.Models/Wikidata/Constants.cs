namespace SparqlForHumans.Models.Wikidata
{
    public static class Constants
    {
        public const string EntityIRI = "http://www.wikidata.org/entity/";
        public const string EntityPrefix = "Q";

        public const string PropertyIRI = "http://www.wikidata.org/prop/direct/";
        public const string PropertyPrefix = "P";

        public const string LabelIRI = "http://www.w3.org/2000/01/rdf-schema#label";

        //public static string prefLabel  { get; }= "http://www.w3.org/2004/02/skos/core#prefLabel";
        //public static string nameIRI  { get; }= "http://schema.org/name";
        public const string Alt_labelIRI = "http://www.w3.org/2004/02/skos/core#altLabel";

        public const string DescriptionIRI = "http://schema.org/description";

        public const string InstanceOf = "P31";
        public const string SubClass = "P279";

        public const string PropertyValueSeparator = "##";
        public const char BlankSpaceChar = ' ';
        public const char HyphenChar = '-';
        public const string QueryConcatenator = " AND ";

        //public static string image  { get; }= "P18";
    }
}