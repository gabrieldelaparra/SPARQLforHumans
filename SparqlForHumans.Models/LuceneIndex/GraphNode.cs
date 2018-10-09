namespace SparqlForHumans.Models.LuceneIndex
{
    public class GraphNode
    {
        public GraphNode(string id, int index)
        {
            Id = id;
            Index = index;
        }

        public string Id { get; set; }
        public int Index { get; set; }

        //Just store the index, not the name of the node.
        public string[] ConnectedNodes { get; set; }

        public override string ToString()
        {
            return $"{Id} - {ConnectedNodes.Length}";
        }
    }
}