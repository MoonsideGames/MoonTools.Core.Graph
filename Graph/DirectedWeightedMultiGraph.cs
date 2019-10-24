using System;
using System.Collections.Generic;
using System.Linq;
using Collections.Pooled;
using MoreLinq;

namespace MoonTools.Core.Graph
{
    public class DirectedWeightedMultiGraph<TNode, TEdgeData> : IGraph<TNode, TEdgeData> where TNode : IEquatable<TNode>
    {
        protected HashSet<TNode> nodes = new HashSet<TNode>();
        protected Dictionary<TNode, HashSet<TNode>> neighbors = new Dictionary<TNode, HashSet<TNode>>();
        protected Dictionary<(TNode, TNode), HashSet<Guid>> edges = new Dictionary<(TNode, TNode), HashSet<Guid>>();
        protected Dictionary<Guid, (TNode, TNode)> IDToEdge = new Dictionary<Guid, (TNode, TNode)>();
        protected Dictionary<Guid, int> weights = new Dictionary<Guid, int>();
        protected Dictionary<Guid, TEdgeData> edgeToEdgeData = new Dictionary<Guid, TEdgeData>();

        // store search sets to prevent GC
        protected HashSet<TNode> openSet = new HashSet<TNode>();
        protected HashSet<TNode> closedSet = new HashSet<TNode>();
        protected Dictionary<TNode, int> gScore = new Dictionary<TNode, int>();
        protected Dictionary<TNode, int> fScore = new Dictionary<TNode, int>();
        protected Dictionary<TNode, Guid> cameFrom = new Dictionary<TNode, Guid>();

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

        public void AddEdge(TNode v, TNode u, int weight, TEdgeData data)
        {
            CheckNodes(v, u);

            var id = Guid.NewGuid();
            neighbors[v].Add(u);
            weights.Add(id, weight);
            if (!edges.ContainsKey((v, u)))
            {
                edges[(v, u)] = new HashSet<Guid>();
            }
            edges[(v, u)].Add(id);
            edgeToEdgeData.Add(id, data);
            IDToEdge.Add(id, (v, u));
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
            weights.Clear();
            edges.Clear();
            IDToEdge.Clear();
            edgeToEdgeData.Clear();
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

        public IEnumerable<Guid> EdgeIDs(TNode v, TNode u)
        {
            CheckNodes(v, u);
            return edges.ContainsKey((v, u)) ? edges[(v, u)] : Enumerable.Empty<Guid>();
        }

        public bool Exists(TNode node) => nodes.Contains(node);

        public bool Exists(TNode v, TNode u)
        {
            CheckNodes(v, u);
            return edges.ContainsKey((v, u));
        }

        public IEnumerable<TNode> Neighbors(TNode node)
        {
            CheckNodes(node);
            return neighbors.ContainsKey(node) ? neighbors[node] : Enumerable.Empty<TNode>();
        }

        public IEnumerable<int> Weights(TNode v, TNode u)
        {
            CheckNodes(v, u);
            return edges[(v, u)].Select(id => weights[id]);
        }

        public TEdgeData EdgeData(Guid id)
        {
            if (!edgeToEdgeData.ContainsKey(id))
            {
                throw new ArgumentException($"Edge {id} does not exist in the graph.");
            }

            return edgeToEdgeData[id];
        }

        private IEnumerable<Guid> ReconstructPath(PooledDictionary<TNode, Guid> cameFrom, TNode currentNode)
        {
            while (cameFrom.ContainsKey(currentNode))
            {
                var edgeID = cameFrom[currentNode];
                var edge = IDToEdge[edgeID];
                currentNode = edge.Item1;
                yield return edgeID;
            }
        }

        public IEnumerable<Guid> AStarShortestPath(TNode start, TNode end, Func<TNode, TNode, int> heuristic)
        {
            CheckNodes(start, end);

            var openSet = new PooledSet<TNode>(ClearMode.Always);
            var closedSet = new PooledSet<TNode>(ClearMode.Always);
            var gScore = new PooledDictionary<TNode, int>(ClearMode.Always);
            var fScore = new PooledDictionary<TNode, int>(ClearMode.Always);
            var cameFrom = new PooledDictionary<TNode, Guid>(ClearMode.Always);

            openSet.Add(start);

            gScore[start] = 0;
            fScore[start] = heuristic(start, end);

            while (openSet.Any())
            {
                var currentNode = openSet.MinBy(node => fScore[node]).First();

                if (currentNode.Equals(end))
                {
                    openSet.Dispose();
                    closedSet.Dispose();
                    gScore.Dispose();
                    fScore.Dispose();

                    foreach (var edgeID in ReconstructPath(cameFrom, currentNode).Reverse())
                    {
                        yield return edgeID;
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
                        var lowestEdgeID = EdgeIDs(currentNode, neighbor).MinBy(id => weights[id]).First();
                        var weight = weights[lowestEdgeID];

                        var tentativeGScore = gScore.ContainsKey(currentNode) ? gScore[currentNode] + weight : int.MaxValue;

                        if (!openSet.Contains(neighbor) || tentativeGScore < gScore[neighbor])
                        {
                            cameFrom[neighbor] = lowestEdgeID;
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