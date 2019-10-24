using System.Collections.Generic;
using Collections.Pooled;

namespace MoonTools.Core.Graph
{
    abstract public class Graph<TNode, TEdgeData> where TNode : System.IEquatable<TNode>
    {
        protected HashSet<TNode> nodes = new HashSet<TNode>();
        protected Dictionary<TNode, PooledSet<TNode>> neighbors = new Dictionary<TNode, PooledSet<TNode>>();

        public IEnumerable<TNode> Nodes => nodes;

        public int Order => nodes.Count;

        public void AddNode(TNode node)
        {
            if (!Exists(node))
            {
                nodes.Add(node);
                neighbors.Add(node, new PooledSet<TNode>(ClearMode.Always));
            }
        }

        public void AddNodes(params TNode[] nodes)
        {
            foreach (var node in nodes)
            {
                AddNode(node);
            }
        }

        public int Degree(TNode node)
        {
            CheckNodes(node);
            return neighbors[node].Count;
        }

        public bool Exists(TNode node)
        {
            return nodes.Contains(node);
        }

        public IEnumerable<TNode> Neighbors(TNode node)
        {
            CheckNodes(node);
            return neighbors[node];
        }

        protected void CheckNodes(params TNode[] givenNodes)
        {
            foreach (var node in givenNodes)
            {
                if (!Exists(node))
                {
                    throw new System.ArgumentException($"Vertex {node} does not exist in the graph");
                }
            }
        }

        public virtual void Clear()
        {
            nodes.Clear();
            neighbors.Clear();
        }
    }
}