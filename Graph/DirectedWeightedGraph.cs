using System;
using System.Collections.Generic;
using System.Linq;
using Collections.Pooled;
using MoreLinq;

namespace MoonTools.Core.Graph
{
    public class DirectedWeightedGraph<TNode, TEdgeData> : SimpleGraph<TNode, TEdgeData> where TNode : System.IEquatable<TNode>
    {
        protected Dictionary<(TNode, TNode), int> weights = new Dictionary<(TNode, TNode), int>();

        public void AddEdge(TNode v, TNode u, int weight, TEdgeData edgeData)
        {
            BaseAddEdge(v, u, edgeData);
            weights.Add((v, u), weight);
        }

        public void AddEdges(params (TNode, TNode, int weight, TEdgeData)[] edges)
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

        public int Weight(TNode v, TNode u)
        {
            CheckEdge(v, u);
            return weights[(v, u)];
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

        public IEnumerable<(TNode, TNode, int)> DijkstraSingleSourceShortestPath(TNode source)
        {
            CheckNodes(source);

            var distance = new PooledDictionary<TNode, int>(ClearMode.Always);
            var previous = new PooledDictionary<TNode, TNode>(ClearMode.Always);

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
                    var alt = distance[node] + Weight(node, neighbor);
                    if (alt < distance[neighbor])
                    {
                        distance[neighbor] = alt;
                        previous[neighbor] = node;
                    }
                }
            }

            foreach (var node in Nodes)
            {
                if (!node.Equals(source))
                {
                    yield return (node, previous[node], distance[node]);
                }
            }

            distance.Dispose();
            previous.Dispose();
        }
    }
}