namespace MoonTools.Core.Graph.Extensions
{
    public static class GraphBuilder
    {
        public static DirectedGraph<TNode, Unit> DirectedGraph<TNode>() where TNode : System.IEquatable<TNode>
        {
            return new DirectedGraph<TNode, Unit>();
        }

        public static DirectedMultiGraph<TNode, Unit> DirectedMultiGraph<TNode>() where TNode : System.IEquatable<TNode>
        {
            return new DirectedMultiGraph<TNode, Unit>();
        }

        public static DirectedWeightedGraph<TNode, Unit> DirectedWeightedGraph<TNode>() where TNode : System.IEquatable<TNode>
        {
            return new DirectedWeightedGraph<TNode, Unit>();
        }

        public static DirectedWeightedMultiGraph<TNode, Unit> DirectedWeightedMultiGraph<TNode>() where TNode : System.IEquatable<TNode>
        {
            return new DirectedWeightedMultiGraph<TNode, Unit>();
        }

        public static UndirectedGraph<TNode, Unit> UndirectedGraph<TNode>() where TNode : System.IEquatable<TNode>
        {
            return new UndirectedGraph<TNode, Unit>();
        }

        public static UndirectedWeightedGraph<TNode, Unit> UndirectedWeightedGraph<TNode>() where TNode : System.IEquatable<TNode>
        {
            return new UndirectedWeightedGraph<TNode, Unit>();
        }
    }
}