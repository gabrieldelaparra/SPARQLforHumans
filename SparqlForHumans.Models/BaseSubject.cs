namespace SparqlForHumans.Models
{
    public class BaseSubject : IEntity
    {
        public string Id { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;

        public override string ToString()
        {
            return $"{Label} ({Id})";
        }
    }
}