using System.Collections.Generic;

namespace MoonTools.Core.Graph
{
    public interface IGraph<TNode, TEdgeData> where TNode : System.IEquatable<TNode>
    {
        IEnumerable<TNode> Nodes { get; }

        void AddNode(TNode node);
        void AddNodes(params TNode[] nodes);
        bool Exists(TNode node);
        IEnumerable<TNode> Neighbors(TNode node);
        void Clear();
    }
}