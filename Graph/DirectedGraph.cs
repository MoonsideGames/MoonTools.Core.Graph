using System;
using System.Collections.Generic;
using System.Linq;

namespace MoonTools.Core.Graph
{
    public enum SearchSymbol
    {
        Start,
        Finish
    }

    public class DirectedGraph<TNode, TEdgeData> : IGraph<TNode, TEdgeData> where TNode : IEquatable<TNode>
    {
        protected HashSet<TNode> nodes = new HashSet<TNode>();
        protected HashSet<(TNode, TNode)> edges = new HashSet<(TNode, TNode)>();
        protected Dictionary<(TNode, TNode), TEdgeData> edgesToEdgeData = new Dictionary<(TNode, TNode), TEdgeData>();
        protected Dictionary<TNode, HashSet<TNode>> neighbors = new Dictionary<TNode, HashSet<TNode>>();

        private class SimpleCycleComparer : IEqualityComparer<IEnumerable<TNode>>
        {
            public bool Equals(IEnumerable<TNode> x, IEnumerable<TNode> y)
            {
                return x.SequenceEqual(y);
            }

            public int GetHashCode(IEnumerable<TNode> obj)
            {
                return obj.Aggregate(0, (current, next) => current.GetHashCode() ^ next.GetHashCode());
            }
        }

        public IEnumerable<TNode> Nodes => nodes;
        public IEnumerable<(TNode, TNode)> Edges => edges;

        public void AddNode(TNode node)
        {
            if (!Exists(node))
            {
                nodes.Add(node);
                neighbors.Add(node, new HashSet<TNode>());
            }
        }

        public void AddNodes(params TNode[] nodes)
        {
            foreach (var node in nodes)
            {
                AddNode(node);
            }
        }

        public bool Exists(TNode node)
        {
            return nodes.Contains(node);
        }

        public bool Exists(TNode v, TNode u)
        {
            return edges.Contains((v, u));
        }

        private void CheckNodes(params TNode[] givenNodes)
        {
            foreach (var node in givenNodes)
            {
                if (!Exists(node))
                {
                    throw new ArgumentException($"Vertex {node} does not exist in the graph");
                }
            }
        }

        readonly List<(TNode, TNode)> edgesToRemove = new List<(TNode, TNode)>();

        public void RemoveNode(TNode node)
        {
            CheckNodes(node);

            edgesToRemove.Clear();

            foreach (var entry in neighbors)
            {
                if (entry.Value.Contains(node))
                {
                    edgesToRemove.Add((entry.Key, node));
                }
            }

            foreach (var edge in edgesToRemove)
            {
                RemoveEdge(edge.Item1, edge.Item2);
            }

            nodes.Remove(node);
            neighbors.Remove(node);
        }

        public void AddEdge(TNode v, TNode u, TEdgeData edgeData)
        {
            CheckNodes(v, u);

            neighbors[v].Add(u);
            edges.Add((v, u));
            edgesToEdgeData.Add((v, u), edgeData);
        }

        public void AddEdges(params (TNode, TNode, TEdgeData)[] edges)
        {
            foreach (var edge in edges)
            {
                AddEdge(edge.Item1, edge.Item2, edge.Item3);
            }
        }

        private void CheckEdge(TNode v, TNode u)
        {
            CheckNodes(v, u);
            if (!Exists(v, u)) { throw new ArgumentException($"Edge between vertex {v} and vertex {u} does not exist in the graph"); }
        }

        public void RemoveEdge(TNode v, TNode u)
        {
            CheckEdge(v, u);
            neighbors[v].Remove(u);
            edgesToEdgeData.Remove((v, u));
        }

        public TEdgeData EdgeData(TNode v, TNode u)
        {
            CheckEdge(v, u);
            return edgesToEdgeData[(v, u)];
        }

        public IEnumerable<TNode> Neighbors(TNode node)
        {
            CheckNodes(node);

            return neighbors[node];
        }

        readonly HashSet<TNode> discovered = new HashSet<TNode>();
        readonly Dictionary<TNode, uint> dfsOutput = new Dictionary<TNode, uint>();

        public IEnumerable<(TNode, uint)> NodeDFS()
        {
            discovered.Clear();
            dfsOutput.Clear();
            uint time = 0;

            void dfsHelper(TNode v) // refactor this to remove closure
            {
                discovered.Add(v);

                foreach (var neighbor in Neighbors(v))
                {
                    if (!discovered.Contains(neighbor))
                    {
                        dfsHelper(neighbor);
                    }
                }

                time++;
                dfsOutput[v] = time;
            }

            foreach (var node in Nodes)
            {
                if (!discovered.Contains(node))
                {
                    dfsHelper(node);
                }
            }

            return dfsOutput.Select(entry => (entry.Key, entry.Value));
        }

        public bool Cyclic()
        {
            return StronglyConnectedComponents().Any((scc) => scc.Count() > 1);
        }

        public IEnumerable<TNode> TopologicalSort()
        {
            return NodeDFS().OrderByDescending(entry => entry.Item2).Select(entry => entry.Item1);
        }

        readonly Dictionary<TNode, uint> preorder = new Dictionary<TNode, uint>();
        readonly Dictionary<TNode, uint> lowlink = new Dictionary<TNode, uint>();
        readonly Dictionary<TNode, bool> sccFound = new Dictionary<TNode, bool>();
        readonly Stack<TNode> sccQueue = new Stack<TNode>();
        readonly List<List<TNode>> sccResult = new List<List<TNode>>();

        public IEnumerable<IEnumerable<TNode>> StronglyConnectedComponents()
        {
            preorder.Clear();
            lowlink.Clear();
            sccFound.Clear();
            sccQueue.Clear();
            sccResult.Clear();

            uint preorderCounter = 0;

            foreach (var source in Nodes)
            {
                if (!sccFound.ContainsKey(source))
                {
                    var queue = new Stack<TNode>();
                    queue.Push(source);

                    while (queue.Count > 0)
                    {
                        var v = queue.Peek();
                        if (!preorder.ContainsKey(v))
                        {
                            preorderCounter++;
                            preorder[v] = preorderCounter;
                        }

                        var done = true;
                        var vNeighbors = Neighbors(v);
                        foreach (var w in vNeighbors)
                        {
                            if (!preorder.ContainsKey(w))
                            {
                                queue.Push(w);
                                done = false;
                                break;
                            }
                        }

                        if (done)
                        {
                            lowlink[v] = preorder[v];
                            foreach (var w in vNeighbors)
                            {
                                if (!sccFound.ContainsKey(w))
                                {
                                    if (preorder[w] > preorder[v])
                                    {
                                        lowlink[v] = Math.Min(lowlink[v], lowlink[w]);
                                    }
                                    else
                                    {
                                        lowlink[v] = Math.Min(lowlink[v], preorder[w]);
                                    }
                                }
                            }
                            queue.Pop();
                            if (lowlink[v] == preorder[v])
                            {
                                sccFound[v] = true;
                                var scc = new List<TNode>
                                {
                                    v
                                };
                                while (sccQueue.Count > 0 && preorder[sccQueue.Peek()] > preorder[v])
                                {
                                    var k = sccQueue.Pop();
                                    sccFound[k] = true;
                                    scc.Add(k);
                                }
                                sccResult.Add(scc);
                            }
                            else
                            {
                                sccQueue.Push(v);
                            }
                        }
                    }
                }
            }

            return sccResult;
        }

        public IEnumerable<IEnumerable<TNode>> SimpleCycles()
        {
            void unblock(TNode thisnode, HashSet<TNode> blocked, Dictionary<TNode, HashSet<TNode>> B) //refactor to remove closure
            {
                var stack = new Stack<TNode>();
                stack.Push(thisnode);

                while (stack.Count > 0)
                {
                    var node = stack.Pop();
                    if (blocked.Contains(thisnode))
                    {
                        blocked.Remove(thisnode);
                        if (B.ContainsKey(node))
                        {
                            foreach (var n in B[node])
                            {
                                if (!stack.Contains(n))
                                {
                                    stack.Push(n);
                                }
                            }
                            B[node].Clear();
                        }
                    }
                }
            }

            List<List<TNode>> result = new List<List<TNode>>();
            var subGraph = Clone();

            var sccs = new Stack<IEnumerable<TNode>>();
            foreach (var scc in StronglyConnectedComponents())
            {
                sccs.Push(scc);
            }

            while (sccs.Count > 0)
            {
                var scc = new Stack<TNode>(sccs.Pop());
                var startNode = scc.Pop();
                var path = new Stack<TNode>();
                path.Push(startNode);
                var blocked = new HashSet<TNode>
                {
                    startNode
                };
                var closed = new HashSet<TNode>();
                var B = new Dictionary<TNode, HashSet<TNode>>();
                var stack = new Stack<Tuple<TNode, Stack<TNode>>>();
                stack.Push(Tuple.Create(startNode, new Stack<TNode>(subGraph.Neighbors(startNode))));

                while (stack.Count > 0)
                {
                    var entry = stack.Peek();
                    var thisnode = entry.Item1;
                    var neighbors = entry.Item2;

                    if (neighbors.Count > 0)
                    {
                        var nextNode = neighbors.Pop();

                        if (nextNode.Equals(startNode))
                        {
                            var resultPath = new List<TNode>();
                            foreach (var v in path)
                            {
                                resultPath.Add(v);
                            }
                            result.Add(resultPath);
                            foreach (var v in path)
                            {
                                closed.Add(v);
                            }
                        }
                        else if (!blocked.Contains(nextNode))
                        {
                            path.Push(nextNode);
                            stack.Push(Tuple.Create(nextNode, new Stack<TNode>(subGraph.Neighbors(nextNode))));
                            closed.Remove(nextNode);
                            blocked.Add(nextNode);
                            continue;
                        }
                    }

                    if (neighbors.Count == 0)
                    {
                        if (closed.Contains(thisnode))
                        {
                            unblock(thisnode, blocked, B);
                        }
                        else
                        {
                            foreach (var neighbor in subGraph.Neighbors(thisnode))
                            {
                                if (!B.ContainsKey(neighbor))
                                {
                                    B[neighbor] = new HashSet<TNode>();
                                }
                                B[neighbor].Add(thisnode);
                            }
                        }

                        stack.Pop();
                        path.Pop();
                    }
                }

                subGraph.RemoveNode(startNode);
                var H = subGraph.SubGraph(scc.ToArray());
                var HSccs = H.StronglyConnectedComponents();
                foreach (var HScc in HSccs)
                {
                    sccs.Push(HScc);
                }
            }

            return result.Distinct(new SimpleCycleComparer());
        }

        public DirectedGraph<TNode, TEdgeData> Clone()
        {
            var clone = new DirectedGraph<TNode, TEdgeData>();
            clone.AddNodes(Nodes.ToArray());

            foreach (var v in Nodes)
            {
                foreach (var n in Neighbors(v))
                {
                    clone.AddEdge(v, n, EdgeData(v, n));
                }
            }

            return clone;
        }

        public DirectedGraph<TNode, TEdgeData> SubGraph(params TNode[] subVertices)
        {
            var subGraph = new DirectedGraph<TNode, TEdgeData>();
            subGraph.AddNodes(subVertices.ToArray());

            foreach (var n in Nodes)
            {
                if (Nodes.Contains(n))
                {
                    var neighbors = Neighbors(n);
                    foreach (var u in neighbors)
                    {
                        if (subVertices.Contains(u))
                        {
                            subGraph.AddEdge(n, u, EdgeData(n, u));
                        }
                    }
                }
            }

            return subGraph;
        }

        public void Clear()
        {
            nodes.Clear();
            neighbors.Clear();
            edges.Clear();
            edgesToEdgeData.Clear();
        }
    }
}
