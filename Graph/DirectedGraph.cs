using System;
using System.Collections.Generic;
using System.Linq;

namespace MoonTools.Core.Graph
{
    public enum SearchSymbol
    {
        start,
        finish
    }

    public class DirectedGraph<T>
    {
        private class SimpleCycleComparer : IEqualityComparer<IEnumerable<T>>
        {
            public bool Equals(IEnumerable<T> x, IEnumerable<T> y)
            {
                return x.SequenceEqual(y);
            }

            public int GetHashCode(IEnumerable<T> obj)
            {
                return obj.Aggregate(0, (current, next) => current.GetHashCode() ^ next.GetHashCode());
            }
        }

        protected List<T> _vertices = new List<T>();
        protected Dictionary<T, HashSet<T>> _neighbors = new Dictionary<T, HashSet<T>>();

        public IEnumerable<T> Vertices { get { return _vertices; } }

        /*
         * GRAPH STRUCTURE METHODS
         */

        public void AddVertex(T vertex)
        {
            if (!VertexExists(vertex))
            {
                _vertices.Add(vertex);
                _neighbors.Add(vertex, new HashSet<T>());
            }
        }

        public void AddVertices(params T[] vertices)
        {
            foreach (var vertex in vertices)
            {
                AddVertex(vertex);
            }
        }

        public bool VertexExists(T vertex)
        {
            return Vertices.Contains(vertex);
        }

        public void RemoveVertex(T vertex)
        {
            var edgesToRemove = new List<Tuple<T, T>>();

            if (VertexExists(vertex))
            {
                foreach (var entry in _neighbors)
                {
                    if (entry.Value.Contains(vertex))
                    {
                        edgesToRemove.Add(Tuple.Create(entry.Key, vertex));
                    }
                }

                foreach (var edge in edgesToRemove)
                {
                    RemoveEdge(edge.Item1, edge.Item2);
                }

                _vertices.Remove(vertex);
                _neighbors.Remove(vertex);
            }
        }

        public void AddEdge(T v, T u)
        {
            if (VertexExists(v) && VertexExists(u))
            {
                _neighbors[v].Add(u);
            }
        }

        public void AddEdges(params Tuple<T, T>[] edges)
        {
            foreach (var edge in edges)
            {
                AddEdge(edge.Item1, edge.Item2);
            }
        }

        public void RemoveEdge(T v, T u)
        {
            _neighbors[v].Remove(u);
        }

        public IEnumerable<T> Neighbors(T vertex)
        {
            if (VertexExists(vertex))
            {
                return _neighbors[vertex];
            }
            else
            {
                return Enumerable.Empty<T>();
            }
        }

        /*
         * GRAPH ANALYSIS METHODS
         */

        public Dictionary<T, Dictionary<SearchSymbol, uint>> NodeDFS()
        {
            var discovered = new HashSet<T>();
            uint time = 0;
            var output = new Dictionary<T, Dictionary<SearchSymbol, uint>>();

            foreach (var vertex in Vertices)
            {
                output.Add(vertex, new Dictionary<SearchSymbol, uint>());
            }

            void dfsHelper(T v)
            {
                discovered.Add(v);
                time++;
                output[v].Add(SearchSymbol.start, time);

                foreach (var neighbor in Neighbors(v))
                {
                    if (!discovered.Contains(neighbor))
                    {
                        dfsHelper(neighbor);
                    }
                }

                time++;
                output[v].Add(SearchSymbol.finish, time);
            }

            foreach (var vertex in Vertices)
            {
                if (!discovered.Contains(vertex))
                {
                    dfsHelper(vertex);
                }
            }

            return output;
        }

        public bool Cyclic()
        {
            return StronglyConnectedComponents().Any((scc) => scc.Count() > 1);
        }

        public IEnumerable<T> TopologicalSort()
        {
            var dfs = NodeDFS();
            var priority = new SortedList<uint, T>();
            foreach (var entry in dfs)
            {
                priority.Add(entry.Value[SearchSymbol.finish], entry.Key);
            }
            return priority.Values.Reverse();
        }

        public IEnumerable<IEnumerable<T>> StronglyConnectedComponents()
        {
            var preorder = new Dictionary<T, uint>();
            var lowlink = new Dictionary<T, uint>();
            var sccFound = new Dictionary<T, bool>();
            var sccQueue = new Stack<T>();

            var result = new List<List<T>>();

            uint preorderCounter = 0;

            foreach (var source in Vertices)
            {
                if (!sccFound.ContainsKey(source))
                {
                    var queue = new Stack<T>();
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
                                var scc = new List<T>
                                {
                                    v
                                };
                                while (sccQueue.Count > 0 && preorder[sccQueue.Peek()] > preorder[v])
                                {
                                    var k = sccQueue.Pop();
                                    sccFound[k] = true;
                                    scc.Add(k);
                                }
                                result.Add(scc);
                            }
                            else
                            {
                                sccQueue.Push(v);
                            }
                        }
                    }
                }
            }

            return result;
        }

        public IEnumerable<IEnumerable<T>> SimpleCycles()
        {
            void unblock(T thisnode, HashSet<T> blocked, Dictionary<T, HashSet<T>> B)
            {
                var stack = new Stack<T>();
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

            List<List<T>> result = new List<List<T>>();
            var subGraph = Clone();

            var sccs = new Stack<IEnumerable<T>>();
            foreach (var scc in StronglyConnectedComponents())
            {
                sccs.Push(scc);
            }

            while (sccs.Count > 0)
            {
                var scc = new Stack<T>(sccs.Pop());
                var startNode = scc.Pop();
                var path = new Stack<T>();
                path.Push(startNode);
                var blocked = new HashSet<T>
                {
                    startNode
                };
                var closed = new HashSet<T>();
                var B = new Dictionary<T, HashSet<T>>();
                var stack = new Stack<Tuple<T, Stack<T>>>();
                stack.Push(Tuple.Create(startNode, new Stack<T>(subGraph.Neighbors(startNode))));

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
                            var resultPath = new List<T>();
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
                            stack.Push(Tuple.Create(nextNode, new Stack<T>(subGraph.Neighbors(nextNode))));
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
                                    B[neighbor] = new HashSet<T>();
                                }
                                B[neighbor].Add(thisnode);
                            }
                        }

                        stack.Pop();
                        path.Pop();
                    }
                }

                subGraph.RemoveVertex(startNode);
                var H = subGraph.SubGraph(scc.ToArray());
                var HSccs = H.StronglyConnectedComponents();
                foreach (var HScc in HSccs)
                {
                    sccs.Push(HScc);
                }
            }

            return result.Distinct(new SimpleCycleComparer());
        }

        public DirectedGraph<T> Clone()
        {
            var clone = new DirectedGraph<T>();
            clone.AddVertices(Vertices.ToArray());

            foreach (var v in Vertices)
            {
                foreach (var n in Neighbors(v))
                {
                    clone.AddEdge(v, n);
                }
            }

            return clone;
        }

        public DirectedGraph<T> SubGraph(params T[] subVertices)
        {
            var subGraph = new DirectedGraph<T>();
            subGraph.AddVertices(subVertices.ToArray());

            foreach (var v in Vertices)
            {
                if (Vertices.Contains(v))
                {
                    var neighbors = Neighbors(v);
                    foreach (var u in neighbors)
                    {
                        if (subVertices.Contains(u))
                        {
                            subGraph.AddEdge(v, u);
                        }
                    }
                }
            }

            return subGraph;
        }
    }
}
