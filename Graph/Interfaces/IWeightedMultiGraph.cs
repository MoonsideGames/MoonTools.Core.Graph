namespace MoonTools.Core.Graph
{
    public interface IWeightedMultiGraph<TNode, TEdgeData>
    {
        System.Guid AddEdge(TNode v, TNode u, int weight, TEdgeData edgeData);
    }
}