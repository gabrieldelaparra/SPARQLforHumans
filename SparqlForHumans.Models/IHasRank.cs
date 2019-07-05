namespace SparqlForHumans.Models
{
    public interface IHasRank
    {
    }

    public interface IHasRank<T> : IHasRank
    {
        T Rank { get; set; }
    }
}