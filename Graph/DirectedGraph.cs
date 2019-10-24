using System;
using System.Collections.Generic;
using System.Linq;
using Collections.Pooled;

namespace MoonTools.Core.Graph
{
    public class DirectedGraph<TNode, TEdgeData> : SimpleGraph<TNode, TEdgeData> where TNode : IEquatable<TNode>
    {
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

        public virtual void AddEdge(TNode v, TNode u, TEdgeData edgeData)
        {
            BaseAddEdge(v, u, edgeData);
        }

        public virtual void AddEdges(params (TNode, TNode, TEdgeData)[] edges)
        {
            foreach (var edge in edges)
            {
                AddEdge(edge.Item1, edge.Item2, edge.Item3);
            }
        }

        public void RemoveNode(TNode node)
        {
            CheckNodes(node);

            var edgesToRemove = new PooledList<(TNode, TNode)>(ClearMode.Always);

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

            edgesToRemove.Dispose();

            nodes.Remove(node);
            neighbors.Remove(node);
        }

        public virtual void RemoveEdge(TNode v, TNode u)
        {
            CheckEdge(v, u);
            neighbors[v].Remove(u);
            edges.Remove((v, u));
            edgeToEdgeData.Remove((v, u));
        }

        public IEnumerable<TNode> PreorderNodeDFS()
        {
            var dfsStack = new PooledStack<TNode>(ClearMode.Always);
            var dfsDiscovered = new PooledSet<TNode>(ClearMode.Always);

            foreach (var node in Nodes)
            {
                if (!dfsDiscovered.Contains(node))
                {
                    dfsStack.Push(node);
                    while (dfsStack.Count > 0)
                    {
                        var current = dfsStack.Pop();
                        if (!dfsDiscovered.Contains(current))
                        {
                            dfsDiscovered.Add(current);
                            yield return current;
                            foreach (var neighbor in Neighbors(current))
                            {
                                dfsStack.Push(neighbor);
                            }
                        }
                    }
                }
            }

            dfsStack.Dispose();
            dfsDiscovered.Dispose();
        }

        private IEnumerable<TNode> PostorderNodeDFSHelper(PooledSet<TNode> discovered, TNode v)
        {
            discovered.Add(v);

            foreach (var neighbor in Neighbors(v))
            {
                if (!discovered.Contains(neighbor))
                {
                    foreach (var node in PostorderNodeDFSHelper(discovered, neighbor))
                    {
                        yield return node;
                    }
                }
            }

            yield return v;
        }

        public IEnumerable<TNode> PostorderNodeDFS()
        {
            var dfsDiscovered = new PooledSet<TNode>(ClearMode.Always);

            foreach (var node in Nodes)
            {
                if (!dfsDiscovered.Contains(node))
                {
                    foreach (var thing in PostorderNodeDFSHelper(dfsDiscovered, node))
                    {
                        yield return thing;
                    }
                }
            }
        }

        public IEnumerable<TNode> NodeBFS()
        {
            var bfsQueue = new PooledQueue<TNode>(ClearMode.Always);
            var bfsDiscovered = new PooledSet<TNode>(ClearMode.Always);

            foreach (var node in Nodes)
            {
                if (!bfsDiscovered.Contains(node))
                {
                    bfsQueue.Enqueue(node);
                    while (bfsQueue.Count > 0)
                    {
                        var current = bfsQueue.Dequeue();
                        foreach (var neighbor in Neighbors(current))
                        {
                            if (!bfsDiscovered.Contains(neighbor))
                            {
                                bfsDiscovered.Add(neighbor);
                                bfsQueue.Enqueue(neighbor);
                                yield return neighbor;
                            }
                        }
                    }
                }
            }

            bfsQueue.Dispose();
            bfsDiscovered.Dispose();
        }

        List<PooledSet<TNode>> lexicographicSets = new List<PooledSet<TNode>>();
        HashSet<PooledSet<TNode>> replacedSets = new HashSet<PooledSet<TNode>>();

        public IEnumerable<TNode> LexicographicBFS()
        {
            lexicographicSets.Add(Nodes.ToPooledSet());

            while (lexicographicSets.Count > 0)
            {
                var firstSet = lexicographicSets[0];
                var node = firstSet.First();
                firstSet.Remove(node);
                if (firstSet.Count == 0) { lexicographicSets.RemoveAt(0); }

                yield return node;

                foreach (var neighbor in Neighbors(node))
                {
                    if (lexicographicSets.Any(set => set.Contains(neighbor)))
                    {
                        var s = lexicographicSets.Find(set => set.Contains(neighbor));
                        var sIndex = lexicographicSets.IndexOf(s);
                        PooledSet<TNode> t;
                        if (replacedSets.Contains(s) && sIndex > 0)
                        {
                            t = lexicographicSets[sIndex - 1];
                        }
                        else
                        {
                            t = new PooledSet<TNode>(ClearMode.Always);
                            lexicographicSets.Insert(sIndex, t);
                            replacedSets.Add(s);
                        }

                        s.Remove(neighbor);
                        t.Add(neighbor);
                        if (s.Count == 0) { lexicographicSets.Remove(s); replacedSets.Remove(s); s.Dispose(); }
                    }
                }
            }

            lexicographicSets.Clear();
            replacedSets.Clear();
        }

        public bool Cyclic()
        {
            return StronglyConnectedComponents().Any((scc) => scc.Count() > 1);
        }

        public IEnumerable<TNode> TopologicalSort()
        {
            return PostorderNodeDFS().Reverse();
        }
        List<PooledList<TNode>> sccs = new List<PooledList<TNode>>();

        public IEnumerable<IEnumerable<TNode>> StronglyConnectedComponents()
        {
            foreach (var scc in sccs)
            {
                scc.Dispose();
            }
            sccs.Clear();

            var preorder = new PooledDictionary<TNode, uint>();
            var lowlink = new PooledDictionary<TNode, uint>();
            var sccFound = new PooledDictionary<TNode, bool>();
            var sccQueue = new PooledStack<TNode>();

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
                                var scc = new PooledList<TNode>
                                {
                                    v
                                };
                                while (sccQueue.Count > 0 && preorder[sccQueue.Peek()] > preorder[v])
                                {
                                    var k = sccQueue.Pop();
                                    sccFound[k] = true;
                                    scc.Add(k);
                                }

                                sccs.Add(scc);
                                yield return scc;
                            }
                            else
                            {
                                sccQueue.Push(v);
                            }
                        }
                    }
                }
            }
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

        public override void Clear()
        {
            base.Clear();
        }
    }
}
