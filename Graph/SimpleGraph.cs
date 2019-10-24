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