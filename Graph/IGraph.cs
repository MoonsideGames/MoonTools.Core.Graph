using System.Collections.Generic;

namespace MoonTools.Core.Graph
{
    public interface IGraph<TNode, TEdgeData>
    {
        IEnumerable<TNode> Nodes { get; }

        void AddNode(TNode node);
        void AddNodes(params TNode[] nodes);
        bool Exists(TNode node);
        IEnumerable<TNode> Neighbors(TNode node);
        void Clear();
    }
}