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
            return edges[(v, u)].Select(id => weights[id]);
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