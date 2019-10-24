namespace MoonTools.Core.Graph.Extensions
{
    public static class WeightedExtensions
    {
        public static void AddEdge<TGraph, TNode>(this TGraph g, TNode v, TNode u, int weight) where TGraph : Graph<TNode, Unit>, IWeightedGraph<TNode, Unit> where TNode : System.IEquatable<TNode>
        {
            g.AddEdge(v, u, weight, default(Unit));
        }

        public static void AddEdges<TGraph, TNode>(this TGraph g, params (TNode, TNode, int)[] edges) where TGraph : Graph<TNode, Unit>, IWeightedGraph<TNode, Unit> where TNode : System.IEquatable<TNode>
        {
            foreach (var (v, u, weight) in edges)
            {
                g.AddEdge(v, u, weight, default(Unit));
            }
        }
    }
}