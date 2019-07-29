namespace SparqlForHumans.Models.Query
{
    //TODO: Implement IsEqualityComparer<RDFExplorerGraph>, para saber si el graph cambió, luego procesar la otra parte.
    public class RDFExplorerGraph
    {
        public Node[] nodes { get; set; } = new Node[0];

        public Edge[] edges { get; set; } = new Edge[0];

        public Selected selected { get; set; } = new Selected();

        public override string ToString()
        {
            return $"{string.Join<Node>(";", nodes)}";
        }
    }


}
