namespace MoonTools.Core.Graph
{
    public interface IWeightedGraph<TNode, TEdgeData>
    {
        void AddEdge(TNode v, TNode u, int weight, TEdgeData edgeData);
    }
}