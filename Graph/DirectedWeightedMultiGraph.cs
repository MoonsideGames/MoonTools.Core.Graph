using System;
using System.Collections.Generic;
using System.Linq;
using Collections.Pooled;
using MoreLinq;

namespace MoonTools.Core.Graph
{
    public class DirectedWeightedMultiGraph<TNode, TEdgeData> : MultiGraph<TNode, TEdgeData> where TNode : IEquatable<TNode>
    {
        protected Dictionary<Guid, int> weights = new Dictionary<Guid, int>();

        public Guid AddEdge(TNode v, TNode u, int weight, TEdgeData data)
        {
            var id = BaseAddEdge(v, u, data);
            weights.Add(id, weight);
            return id;
        }

        public void AddEdges(params (TNode, TNode, int, TEdgeData)[] edges)
        {
            foreach (var edge in edges)
            {
                AddEdge(edge.Item1, edge.Item2, edge.Item3, edge.Item4);
            }
        }

        public override void Clear()
        {
            base.Clear();
            weights.Clear();
        }

        public IEnumerable<int> Weights(TNode v, TNode u)
        {
            CheckNodes(v, u);
            return edges[(v, u)].Select(id => Weight(id));
        }

        public int Weight(Guid id)
        {
            if (!IDToEdge.ContainsKey(id)) { throw new ArgumentException($"Edge with id {id} does not exist in the graph"); }
            return weights[id];
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

            while (openSet.Count > 0)
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

        private IEnumerable<Guid> ShortestPath(TNode start, TNode end, Func<TNode, IEnumerable<(TNode, Guid, int)>> SSSPAlgorithm)
        {
            CheckNodes(start, end);

            var cameFrom = new PooledDictionary<TNode, Guid>(ClearMode.Always);
            var reachable = new PooledSet<TNode>(ClearMode.Always);

            foreach (var (node, previous, weight) in SSSPAlgorithm(start))
            {
                cameFrom[node] = previous;
                reachable.Add(node);
            }

            if (!reachable.Contains(end))
            {
                cameFrom.Dispose();
                reachable.Dispose();
                yield break;
            }

            foreach (var edge in ReconstructPath(cameFrom, end).Reverse())
            {
                yield return edge;
            }

            cameFrom.Dispose();
            reachable.Dispose();
        }

        public IEnumerable<(TNode, Guid, int)> DijkstraSingleSourceShortestPath(TNode source)
        {
            if (weights.Values.Any(w => w < 0)) { throw new NegativeWeightException("Dijkstra cannot be used on a graph with negative edge weights. Try Bellman-Ford"); }
            CheckNodes(source);

            var distance = new PooledDictionary<TNode, int>(ClearMode.Always);
            var previousEdgeIDs = new PooledDictionary<TNode, Guid>(ClearMode.Always);

            foreach (var node in Nodes)
            {
                distance[node] = int.MaxValue;
            }

            distance[source] = 0;

            var q = Nodes.ToPooledList();

            while (q.Count > 0)
            {
                var node = q.MinBy(n => distance[n]).First();
                q.Remove(node);
                if (distance[node] == int.MaxValue) { break; }

                foreach (var neighbor in Neighbors(node))
                {
                    foreach (var edgeID in EdgeIDs(node, neighbor))
                    {
                        var weight = Weight(edgeID);

                        var alt = distance[node] + weight;
                        if (alt < distance[neighbor])
                        {
                            distance[neighbor] = alt;
                            previousEdgeIDs[neighbor] = edgeID;
                        }
                    }
                }
            }

            foreach (var node in Nodes)
            {
                if (previousEdgeIDs.ContainsKey(node) && distance.ContainsKey(node))
                {
                    yield return (node, previousEdgeIDs[node], distance[node]);
                }
            }

            distance.Dispose();
            previousEdgeIDs.Dispose();
        }

        public IEnumerable<Guid> DijkstraShortestPath(TNode start, TNode end)
        {
            return ShortestPath(start, end, DijkstraSingleSourceShortestPath);
        }

        public IEnumerable<(TNode, Guid, int)> BellmanFordSingleSourceShortestPath(TNode source)
        {
            CheckNodes(source);

            var distance = new PooledDictionary<TNode, int>(ClearMode.Always);
            var previous = new PooledDictionary<TNode, Guid>(ClearMode.Always);

            foreach (var node in Nodes)
            {
                distance[node] = int.MaxValue;
            }

            distance[source] = 0;

            for (int i = 0; i < Order; i++)
            {
                foreach (var edgeID in IDToEdge.Keys)
                {
                    var weight = Weight(edgeID);
                    var (v, u) = IDToEdge[edgeID];

                    if (distance[v] + weight < distance[u])
                    {
                        distance[u] = distance[v] + weight;
                        previous[u] = edgeID;
                    }
                }
            }

            foreach (var edgeID in IDToEdge.Keys)
            {
                var (v, u) = IDToEdge[edgeID];

                foreach (var weight in Weights(v, u))
                {
                    if (distance[v] + weight < distance[u])
                    {
                        throw new NegativeCycleException();
                    }
                }
            }

            foreach (var node in Nodes)
            {
                if (previous.ContainsKey(node) && distance.ContainsKey(node))
                {
                    yield return (node, previous[node], distance[node]);
                }
            }

            distance.Dispose();
            previous.Dispose();
        }

        public IEnumerable<Guid> BellmanFordShortestPath(TNode start, TNode end)
        {
            return ShortestPath(start, end, BellmanFordSingleSourceShortestPath);
        }
    }
}