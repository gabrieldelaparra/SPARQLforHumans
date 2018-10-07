namespace SparqlForHumans.Models
{
    public class Subject : ISubject
    {
        public string Id { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;

        public override string ToString()
        {
            return $"{Label} ({Id})";
        }
    }
}