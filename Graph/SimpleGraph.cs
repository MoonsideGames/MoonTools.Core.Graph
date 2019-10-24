using System;
using System.Collections.Generic;

namespace MoonTools.Core.Graph
{
    abstract public class SimpleGraph<TNode, TEdgeData> : Graph<TNode, TEdgeData> where TNode : System.IEquatable<TNode>
    {
        protected HashSet<(TNode, TNode)> edges = new HashSet<(TNode, TNode)>();
        protected Dictionary<(TNode, TNode), TEdgeData> edgeToEdgeData = new Dictionary<(TNode, TNode), TEdgeData>();

        public IEnumerable<(TNode, TNode)> Edges => edges;

        public int Size => edges.Count;

        protected void BaseAddEdge(TNode v, TNode u, TEdgeData edgeData)
        {
            CheckNodes(v, u);
            if (Exists(v, u)) { throw new ArgumentException($"Edge between {v} and {u} already exists in the graph"); }

            if (v.Equals(u)) { throw new ArgumentException("Self-edges are not allowed in a simple graph. Use a multigraph instead"); }

            neighbors[v].Add(u);
            edges.Add((v, u));
            edgeToEdgeData.Add((v, u), edgeData);
        }

        public bool Exists(TNode v, TNode u)
        {
            CheckNodes(v, u);
            return edges.Contains((v, u));
        }

        public TEdgeData EdgeData(TNode v, TNode u)
        {
            CheckEdge(v, u);
            return edgeToEdgeData[(v, u)];
        }

        protected void CheckEdge(TNode v, TNode u)
        {
            CheckNodes(v, u);
            if (!Exists(v, u)) { throw new ArgumentException($"Edge between vertex {v} and vertex {u} does not exist in the graph"); }
        }

        public override void Clear()
        {
            base.Clear();
            edges.Clear();
            edgeToEdgeData.Clear();
        }
    }
}