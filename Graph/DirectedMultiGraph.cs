namespace MoonTools.Core.Graph
{
    public class DirectedMultiGraph<TNode, TEdgeData> : MultiGraph<TNode, TEdgeData>, IUnweightedGraph<TNode, TEdgeData> where TNode : System.IEquatable<TNode>
    {
        public void AddEdge(TNode v, TNode u, TEdgeData edgeData)
        {
            BaseAddEdge(v, u, edgeData);
        }

        public void AddEdges(params (TNode, TNode, TEdgeData)[] edges)
        {
            foreach (var edge in edges)
            {
                AddEdge(edge.Item1, edge.Item2, edge.Item3);
            }
        }
    }
}