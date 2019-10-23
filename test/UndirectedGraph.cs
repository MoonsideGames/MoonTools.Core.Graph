using NUnit.Framework;
using FluentAssertions;

using MoonTools.Core.Graph;

namespace Tests
{
    public class UndirectedGraphTests
    {
        EdgeData dummyEdgeData;

        [Test]
        public void Size()
        {
            var graph = new UndirectedGraph<char, EdgeData>();
            graph.AddNodes('a', 'b', 'c', 'd', 'e');
            graph.AddEdges(
                ('a', 'b', dummyEdgeData),
                ('a', 'c', dummyEdgeData),
                ('a', 'd', dummyEdgeData),
                ('b', 'c', dummyEdgeData),
                ('b', 'd', dummyEdgeData),
                ('c', 'd', dummyEdgeData),
                ('c', 'e', dummyEdgeData)
            );

            graph.Size.Should().Be(7);
        }

        [Test]
        public void Degree()
        {
            var graph = new UndirectedGraph<char, EdgeData>();
            graph.AddNodes('a', 'b', 'c', 'd', 'e');
            graph.AddEdges(
                ('a', 'b', dummyEdgeData),
                ('a', 'c', dummyEdgeData),
                ('a', 'd', dummyEdgeData),
                ('b', 'c', dummyEdgeData),
                ('b', 'd', dummyEdgeData),
                ('c', 'd', dummyEdgeData),
                ('c', 'e', dummyEdgeData)
            );

            graph.Degree('a').Should().Be(3);
            graph.Degree('b').Should().Be(3);
            graph.Degree('c').Should().Be(4);
            graph.Degree('d').Should().Be(3);
            graph.Degree('e').Should().Be(1);
        }

        [Test]
        public void Clique()
        {
            var graph = new UndirectedGraph<char, EdgeData>();
            graph.AddNodes('a', 'b', 'c', 'd', 'e');
            graph.AddEdges(
                ('a', 'b', dummyEdgeData),
                ('a', 'c', dummyEdgeData),
                ('a', 'd', dummyEdgeData),
                ('b', 'c', dummyEdgeData),
                ('b', 'd', dummyEdgeData),
                ('c', 'd', dummyEdgeData),
                ('c', 'e', dummyEdgeData)
            );

            graph.Clique(new char[] { 'a', 'b', 'c', 'd' }).Should().BeTrue();
            graph.Clique(new char[] { 'a', 'b', 'c', 'd', 'e' }).Should().BeFalse();
        }

        [Test]
        public void Chordal()
        {
            var graph = new UndirectedGraph<char, EdgeData>();
            graph.AddNodes('a', 'b', 'c', 'd', 'e');
            graph.AddEdges(
                ('a', 'b', dummyEdgeData),
                ('a', 'c', dummyEdgeData),
                ('b', 'c', dummyEdgeData),
                ('b', 'd', dummyEdgeData),
                ('b', 'e', dummyEdgeData),
                ('c', 'd', dummyEdgeData),
                ('d', 'e', dummyEdgeData)
            );

            graph.Chordal.Should().BeTrue();

            graph.AddNode('f');
            graph.AddEdge('a', 'f', dummyEdgeData);

            graph.Chordal.Should().BeFalse();
        }

        [Test]
        public void Complete()
        {
            var graph = new UndirectedGraph<char, EdgeData>();
            graph.AddNodes('a', 'b', 'c', 'd');
            graph.AddEdges(
                ('a', 'b', dummyEdgeData),
                ('a', 'c', dummyEdgeData),
                ('a', 'd', dummyEdgeData),
                ('b', 'c', dummyEdgeData),
                ('b', 'd', dummyEdgeData),
                ('c', 'd', dummyEdgeData)
            );

            graph.Complete.Should().BeTrue();

            graph.RemoveEdge('b', 'c');

            graph.Complete.Should().BeFalse();
        }

        [Test]
        public void Bipartite()
        {
            var graph = new UndirectedGraph<char, EdgeData>();
            graph.AddNodes('a', 'b', 'c', 'd', 'e');
            graph.AddEdges(
                ('a', 'b', dummyEdgeData),
                ('a', 'c', dummyEdgeData),
                ('b', 'd', dummyEdgeData),
                ('c', 'e', dummyEdgeData)
            );

            graph.Bipartite.Should().BeTrue();

            graph.AddEdge('a', 'e', dummyEdgeData);

            graph.Bipartite.Should().BeFalse();
        }
    }
}