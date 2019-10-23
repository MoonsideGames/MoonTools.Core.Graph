using System.Linq;
using System.Collections.Generic;

namespace MoonTools.Core.Graph
{
    public class UndirectedGraph<TNode, TEdgeData> : DirectedGraph<TNode, TEdgeData> where TNode : System.IEquatable<TNode>
    {
        enum Color { White, Gray, Black }

        new public int Size => edges.Count / 2;

        public bool Complete => Size == (Order * (Order - 1) / 2);

        public bool Chordal
        {
            get
            {
                var lexicographicOrder = LexicographicBFS();
                return lexicographicOrder
                    .Select((node, index) => (node, index))
                    .All(pair =>
                    {
                        var (node, index) = pair;
                        var successors = lexicographicOrder.Skip(index - 1).Take(nodes.Count - index - 1);
                        return Clique(Neighbors(node).Intersect(successors).Union(Enumerable.Repeat(node, 1)));
                    });
            }
        }

        public bool Bipartite
        {
            get
            {
                var colors = new Dictionary<TNode, Color>();
                var d = new Dictionary<TNode, int>();
                var partition = new Dictionary<TNode, int>();

                foreach (var node in Nodes)
                {
                    colors[node] = Color.White;
                    d[node] = int.MaxValue;
                    partition[node] = 0;
                }

                var start = Nodes.First();
                colors[start] = Color.Gray;
                partition[start] = 1;
                d[start] = 0;

                var stack = new Stack<TNode>();
                stack.Push(start);

                while (stack.Count > 0)
                {
                    var node = stack.Pop();
                    foreach (var neighbor in Neighbors(node))
                    {
                        if (partition[neighbor] == partition[node]) { return false; }
                        if (colors[neighbor] == Color.White)
                        {
                            colors[neighbor] = Color.Gray;
                            d[neighbor] = d[node] + 1;
                            partition[neighbor] = 3 - partition[node];
                            stack.Push(neighbor);
                        }
                    }
                    stack.Pop();
                    colors[node] = Color.Black;
                }

                return true;
            }
        }

        public override void AddEdge(TNode v, TNode u, TEdgeData edgeData)
        {
            base.AddEdge(v, u, edgeData);
            base.AddEdge(u, v, edgeData);
        }

        public override void AddEdges(params (TNode, TNode, TEdgeData)[] edges)
        {
            foreach (var edge in edges)
            {
                AddEdge(edge.Item1, edge.Item2, edge.Item3);
            }
        }

        public override void RemoveEdge(TNode v, TNode u)
        {
            base.RemoveEdge(v, u);
            base.RemoveEdge(u, v);
        }

        public bool Clique(IEnumerable<TNode> nodeList)
        {
            return nodeList.All(node => nodeList.All(other => Neighbors(node).Contains(other) || node.Equals(other)));
        }
    }
}