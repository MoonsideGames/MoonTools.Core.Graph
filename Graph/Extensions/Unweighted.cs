namespace MoonTools.Core.Graph.Extensions
{
    public static class UnweightedExtensions
    {
        public static void AddEdge<TGraph, TNode>(this TGraph g, TNode v, TNode u) where TGraph : Graph<TNode, Unit>, IUnweightedGraph<TNode, Unit> where TNode : System.IEquatable<TNode>
        {
            g.AddEdge(v, u, default(Unit));
        }

        public static void AddEdges<TGraph, TNode>(this TGraph g, params (TNode, TNode)[] edges) where TGraph : Graph<TNode, Unit>, IUnweightedGraph<TNode, Unit> where TNode : System.IEquatable<TNode>
        {
            foreach (var (v, u) in edges)
            {
                g.AddEdge(v, u, default(Unit));
            }
        }
    }
}