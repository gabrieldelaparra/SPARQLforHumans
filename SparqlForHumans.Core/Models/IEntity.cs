namespace SparqlForHumans.Core.Models
{
    public interface IEntity
    {
        string Id { get; set; }
        string Label { get; set; }
    }
}