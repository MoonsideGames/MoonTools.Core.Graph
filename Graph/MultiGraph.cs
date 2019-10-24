using System;
using System.Collections.Generic;
using System.Linq;

namespace MoonTools.Core.Graph
{
    abstract public class MultiGraph<TNode, TEdgeData> : Graph<TNode, TEdgeData> where TNode : System.IEquatable<TNode>
    {
        protected Dictionary<(TNode, TNode), HashSet<Guid>> edges = new Dictionary<(TNode, TNode), HashSet<Guid>>();
        protected Dictionary<Guid, (TNode, TNode)> IDToEdge = new Dictionary<Guid, (TNode, TNode)>();
        protected Dictionary<Guid, TEdgeData> edgeToEdgeData = new Dictionary<Guid, TEdgeData>();

        protected Guid BaseAddEdge(TNode v, TNode u, TEdgeData edgeData)
        {
            CheckNodes(v, u);

            var id = Guid.NewGuid();
            neighbors[v].Add(u);
            if (!edges.ContainsKey((v, u)))
            {
                edges[(v, u)] = new HashSet<Guid>();
            }
            edges[(v, u)].Add(id);
            edgeToEdgeData.Add(id, edgeData);
            IDToEdge.Add(id, (v, u));

            return id;
        }

        public override void Clear()
        {
            base.Clear();
            edges.Clear();
            IDToEdge.Clear();
            edgeToEdgeData.Clear();
        }

        public IEnumerable<Guid> EdgeIDs(TNode v, TNode u)
        {
            CheckNodes(v, u);
            return edges.ContainsKey((v, u)) ? edges[(v, u)] : Enumerable.Empty<Guid>();
        }

        public bool Exists(TNode v, TNode u)
        {
            CheckNodes(v, u);
            return edges.ContainsKey((v, u));
        }

        public TEdgeData EdgeData(Guid id)
        {
            if (!edgeToEdgeData.ContainsKey(id))
            {
                throw new ArgumentException($"Edge {id} does not exist in the graph.");
            }

            return edgeToEdgeData[id];
        }
    }
}