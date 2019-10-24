namespace MoonTools.Core.Graph
{
    public interface IUnweightedGraph<TNode, TEdgeData>
    {
        void AddEdge(TNode v, TNode u, TEdgeData edgeData);
    }
}