namespace MoonTools.Core.Graph
{
    public class UndirectedWeightedGraph<TNode, TEdgeData> : DirectedWeightedGraph<TNode, TEdgeData> where TNode : System.IEquatable<TNode>
    {
        public override void AddEdge(TNode v, TNode u, int weight, TEdgeData edgeData)
        {
            base.AddEdge(v, u, weight, edgeData);
            base.AddEdge(u, v, weight, edgeData);
        }

        public override void AddEdges(params (TNode, TNode, int, TEdgeData)[] edges)
        {
            foreach (var edge in edges)
            {
                AddEdge(edge.Item1, edge.Item2, edge.Item3, edge.Item4);
            }
        }
    }
}