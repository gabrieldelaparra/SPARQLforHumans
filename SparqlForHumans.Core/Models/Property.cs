namespace SparqlForHumans.Core.Models
{
    public class Property
    {
        public string Id { get; set; }
        public string Label { get; set; }
        public string Value { get; set; }

        public override string ToString()
        {
            return $"{Id} {Label} -> {Value}";
        }
    }
}