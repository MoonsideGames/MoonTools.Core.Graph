using System;
using System.Collections.Generic;
using System.Linq;
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
            nodes.Add(node);
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
            if (Exists(v) && Exists(u))
            {
                var id = Guid.NewGuid();
                if (!neighbors.ContainsKey(v))
                {
                    neighbors[v] = new HashSet<TNode>();
                }
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
            else if (!Exists(v))
            {
                throw new InvalidVertexException("Vertex {0} does not exist in the graph", v);
            }
            else
            {
                throw new InvalidVertexException("Vertex {0} does not exist in the graph", u);
            }
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

        public bool Exists(TNode node)
        {
            return nodes.Contains(node);
        }

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

        private IEnumerable<Guid> ReconstructPath(Dictionary<TNode, Guid> cameFrom, TNode currentNode)
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

            openSet.Clear();
            closedSet.Clear();
            gScore.Clear();
            fScore.Clear();
            cameFrom.Clear();

            openSet.Add(start);

            gScore[start] = 0;
            fScore[start] = heuristic(start, end);

            while (openSet.Any())
            {
                var currentNode = openSet.MinBy(node => fScore[node]).First();

                if (currentNode.Equals(end))
                {
                    return ReconstructPath(cameFrom, currentNode).Reverse();
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

            return Enumerable.Empty<Guid>();
        }
    }
}