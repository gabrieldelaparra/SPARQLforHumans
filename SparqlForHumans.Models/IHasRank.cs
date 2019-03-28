namespace SparqlForHumans.Models
{
    public interface IHasRank<T>
    {
        T Rank { get; set; }
    }
}
