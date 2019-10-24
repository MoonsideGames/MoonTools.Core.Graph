using System;
using System.Collections.Generic;
using System.Linq;
using Collections.Pooled;
using MoreLinq;

namespace MoonTools.Core.Graph
{
    public class DirectedWeightedGraph<TNode, TEdgeData> : IGraph<TNode, TEdgeData> where TNode : System.IEquatable<TNode>
    {
        protected HashSet<TNode> nodes = new HashSet<TNode>();
        protected Dictionary<TNode, HashSet<TNode>> neighbors = new Dictionary<TNode, HashSet<TNode>>();
        protected Dictionary<(TNode, TNode), TEdgeData> edgeToEdgeData = new Dictionary<(TNode, TNode), TEdgeData>();
        protected Dictionary<(TNode, TNode), int> weights = new Dictionary<(TNode, TNode), int>();

        public IEnumerable<TNode> Nodes => nodes;

        public void AddNode(TNode node)
        {
            if (Exists(node)) { return; }

            nodes.Add(node);
            neighbors[node] = new HashSet<TNode>();
        }

        public void AddNodes(params TNode[] nodes)
        {
            foreach (var node in nodes)
            {
                AddNode(node);
            }
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

        public void AddEdge(TNode v, TNode u, int weight, TEdgeData data)
        {
            CheckNodes(v, u);

            if (Exists(v, u)) { throw new ArgumentException($"Edge with vertex {v} and {u} already exists in the graph"); }

            neighbors[v].Add(u);
            weights.Add((v, u), weight);
            edgeToEdgeData.Add((v, u), data);
        }

        public void AddEdges(params (TNode, TNode, int, TEdgeData)[] edges)
        {
            foreach (var edge in edges)
            {
                AddEdge(edge.Item1, edge.Item2, edge.Item3, edge.Item4);
            }
        }

        public void Clear()
        {
            nodes.Clear();
            neighbors.Clear();
            edgeToEdgeData.Clear();
            weights.Clear();
        }

        public bool Exists(TNode node) => nodes.Contains(node);

        public bool Exists(TNode v, TNode u)
        {
            CheckNodes(v, u);
            return neighbors[v].Contains(u);
        }

        public IEnumerable<TNode> Neighbors(TNode node)
        {
            CheckNodes(node);
            return neighbors[node];
        }

        private void CheckEdge(TNode v, TNode u)
        {
            CheckNodes(v, u);
            if (!Exists(v, u)) { throw new ArgumentException($"Edge between vertex {v} and vertex {u} does not exist in the graph"); }
        }

        public int Weight(TNode v, TNode u)
        {
            CheckEdge(v, u);
            return weights[(v, u)];
        }

        public TEdgeData EdgeData(TNode v, TNode u)
        {
            CheckEdge(v, u);
            return edgeToEdgeData[(v, u)];
        }

        private IEnumerable<(TNode, TNode)> ReconstructPath(PooledDictionary<TNode, TNode> cameFrom, TNode currentNode)
        {
            while (cameFrom.ContainsKey(currentNode))
            {
                var edge = (cameFrom[currentNode], currentNode);
                currentNode = edge.Item1;
                yield return edge;
            }
        }

        public IEnumerable<(TNode, TNode)> AStarShortestPath(TNode start, TNode end, Func<TNode, TNode, int> heuristic)
        {
            CheckNodes(start, end);

            var openSet = new PooledSet<TNode>(ClearMode.Always);
            var closedSet = new PooledSet<TNode>(ClearMode.Always);
            var gScore = new PooledDictionary<TNode, int>(ClearMode.Always);
            var fScore = new PooledDictionary<TNode, int>(ClearMode.Always);
            var cameFrom = new PooledDictionary<TNode, TNode>(ClearMode.Always);

            openSet.Add(start);

            gScore[start] = 0;
            fScore[start] = heuristic(start, end);

            while (openSet.Count > 0)
            {
                var currentNode = openSet.MinBy(node => fScore[node]).First();

                if (currentNode.Equals(end))
                {
                    openSet.Dispose();
                    closedSet.Dispose();
                    gScore.Dispose();
                    fScore.Dispose();

                    foreach (var edge in ReconstructPath(cameFrom, currentNode).Reverse())
                    {
                        yield return edge;
                    }

                    cameFrom.Dispose();

                    yield break;
                }

                openSet.Remove(currentNode);
                closedSet.Add(currentNode);

                foreach (var neighbor in Neighbors(currentNode))
                {
                    if (!closedSet.Contains(neighbor))
                    {
                        var weight = weights[(currentNode, neighbor)];

                        var tentativeGScore = gScore.ContainsKey(currentNode) ? gScore[currentNode] + weight : int.MaxValue;

                        if (!openSet.Contains(neighbor) || tentativeGScore < gScore[neighbor])
                        {
                            cameFrom[neighbor] = currentNode;
                            gScore[neighbor] = tentativeGScore;
                            fScore[neighbor] = tentativeGScore + heuristic(neighbor, end);
                            openSet.Add(neighbor);
                        }
                    }
                }
            }

            openSet.Dispose();
            closedSet.Dispose();
            gScore.Dispose();
            fScore.Dispose();
            cameFrom.Dispose();

            yield break;
        }
    }
}