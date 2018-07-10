using System.Data.Entity;

namespace SparqlForHumans.Core.EF
{
    public class GraphNodeDbContext : DbContext
    {
        public DbSet<GraphNode> GraphNodes { get; set; }
    }
}
