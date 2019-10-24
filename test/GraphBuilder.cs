using NUnit.Framework;
using FluentAssertions;

using MoonTools.Core.Graph.Extensions;

namespace Tests
{
    public class GraphBuilderTests
    {
        [Test]
        public void DirectedGraph()
        {
            var directedGraph = GraphBuilder.DirectedGraph<int>();
            directedGraph.AddNodes(1, 2, 3);
            directedGraph.AddEdge(1, 2);
            directedGraph.AddEdges(
                (2, 1),
                (1, 3)
            );

            directedGraph.Neighbors(1).Should().Equal(2, 3);
            directedGraph.Neighbors(2).Should().Equal(1);
        }

        [Test]
        public void DirectedMultiGraph()
        {
            var directedMultiGraph = GraphBuilder.DirectedMultiGraph<int>();
            directedMultiGraph.AddNodes(1, 2, 3, 4);
            directedMultiGraph.AddEdge(1, 2);
            directedMultiGraph.AddEdges(
                (2, 3),
                (3, 4),
                (1, 2)
            );

            directedMultiGraph.Neighbors(1).Should().Equal(2);
            directedMultiGraph.Neighbors(2).Should().Equal(3);
            directedMultiGraph.Neighbors(3).Should().Equal(4);
            directedMultiGraph.EdgeIDs(1, 2).Should().HaveCount(2);
        }

        [Test]
        public void DirectedWeightedGraph()
        {
            var directedWeightedGraph = GraphBuilder.DirectedWeightedGraph<int>();
            directedWeightedGraph.AddNodes(1, 2, 3, 4);
            directedWeightedGraph.AddEdge(1, 2, 10);
            directedWeightedGraph.AddEdges(
                (2, 3, 5),
                (3, 4, 1),
                (4, 2, 2)
            );

            directedWeightedGraph.Neighbors(1).Should().Equal(2);
            directedWeightedGraph.Neighbors(2).Should().Equal(3);
            directedWeightedGraph.Neighbors(3).Should().Equal(4);
            directedWeightedGraph.Neighbors(4).Should().Equal(2);
            directedWeightedGraph.Weight(1, 2).Should().Be(10);
            directedWeightedGraph.Weight(2, 3).Should().Be(5);
            directedWeightedGraph.Weight(3, 4).Should().Be(1);
            directedWeightedGraph.Weight(4, 2).Should().Be(2);
        }

        [Test]
        public void DirectedWeightedMultiGraph()
        {
            var directedWeightedMultiGraph = GraphBuilder.DirectedWeightedMultiGraph<int>();
            directedWeightedMultiGraph.AddNodes(1, 2, 3, 4);
            directedWeightedMultiGraph.AddEdge(1, 2, 10);
            directedWeightedMultiGraph.AddEdges(
                (1, 2, 4),
                (2, 3, 5),
                (3, 4, 1),
                (4, 2, 2)
            );

            directedWeightedMultiGraph.Neighbors(1).Should().Equal(2);
            directedWeightedMultiGraph.Neighbors(2).Should().Equal(3);
            directedWeightedMultiGraph.Neighbors(3).Should().Equal(4);
            directedWeightedMultiGraph.Neighbors(4).Should().Equal(2);
            directedWeightedMultiGraph.Weights(1, 2).Should().Equal(10, 4);
            directedWeightedMultiGraph.Weights(2, 3).Should().Equal(5);
            directedWeightedMultiGraph.Weights(3, 4).Should().Equal(1);
            directedWeightedMultiGraph.Weights(4, 2).Should().Equal(2);
        }

        [Test]
        public void UndirectedGraph()
        {
            var undirectedGraph = GraphBuilder.UndirectedGraph<int>();
            undirectedGraph.AddNodes(1, 2, 3, 4);
            undirectedGraph.AddEdge(1, 2);
            undirectedGraph.AddEdges(
                (2, 3),
                (1, 4)
            );

            undirectedGraph.Neighbors(1).Should().Equal(2, 4);
            undirectedGraph.Neighbors(2).Should().Equal(1, 3);
            undirectedGraph.Neighbors(3).Should().Equal(2);
            undirectedGraph.Neighbors(4).Should().Equal(1);
        }

        [Test]
        public void UndirectedWeightedGraph()
        {
            var undirectedWeightedGraph = GraphBuilder.UndirectedWeightedGraph<int>();
            undirectedWeightedGraph.AddNodes(1, 2, 3, 4);
            undirectedWeightedGraph.AddEdge(1, 2, 10);
            undirectedWeightedGraph.AddEdges(
                (2, 3, 4),
                (1, 4, 2)
            );

            undirectedWeightedGraph.Neighbors(1).Should().Equal(2, 4);
            undirectedWeightedGraph.Neighbors(2).Should().Equal(1, 3);
            undirectedWeightedGraph.Neighbors(3).Should().Equal(2);
            undirectedWeightedGraph.Neighbors(4).Should().Equal(1);
            undirectedWeightedGraph.Weight(1, 2).Should().Be(10);
            undirectedWeightedGraph.Weight(2, 3).Should().Be(4);
            undirectedWeightedGraph.Weight(3, 2).Should().Be(4);
            undirectedWeightedGraph.Weight(4, 1).Should().Be(2);
        }
    }
}