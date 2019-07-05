namespace SparqlForHumans.Models
{
    public interface IEntity : ISubject, IHasAltLabel, IHasDescription, IHasInstanceOf, IHasIsType, IHasRank<double>
    {
    }
}