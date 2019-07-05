namespace SparqlForHumans.Models
{
    public interface IBaseEntity : ISubject, IHasAltLabel, IHasDescription, IHasInstanceOf, IHasIsType
    {
    }
}