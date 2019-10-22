using NUnit.Framework;
using FluentAssertions;

using System;
using System.Linq;

using MoonTools.Core.Graph;

namespace Tests
{
    public class DirectedGraphTest
    {
        [Test]
        public void AddVertex()
        {
            var myGraph = new DirectedGraph<int>();
            myGraph.AddVertex(4);

            Assert.That(myGraph.Vertices, Does.Contain(4));
        }

        [Test]
        public void AddVertices()
        {
            var myGraph = new DirectedGraph<int>();
            myGraph.AddVertices(4, 20, 69);

            Assert.IsTrue(myGraph.VertexExists(4));
            Assert.IsTrue(myGraph.VertexExists(20));
            Assert.IsTrue(myGraph.VertexExists(69));
        }

        [Test]
        public void AddEdge()
        {
            var myGraph = new DirectedGraph<int>();
            myGraph.AddVertices(5, 6);
            myGraph.AddEdge(5, 6);

            Assert.That(myGraph.Neighbors(5), Does.Contain(6));
        }

        [Test]
        public void AddEdges()
        {
            var myGraph = new DirectedGraph<int>();
            myGraph.AddVertices(1, 2, 3, 4);
            myGraph.AddEdges(
                Tuple.Create(1, 2),
                Tuple.Create(2, 3),
                Tuple.Create(2, 4),
                Tuple.Create(3, 4)
            );

            Assert.That(myGraph.Neighbors(1), Does.Contain(2));
            Assert.That(myGraph.Neighbors(2), Does.Contain(3));
            Assert.That(myGraph.Neighbors(2), Does.Contain(4));
            Assert.That(myGraph.Neighbors(3), Does.Contain(4));
            Assert.That(myGraph.Neighbors(1), Does.Not.Contain(4));
        }

        [Test]
        public void RemoveEdge()
        {
            var myGraph = new DirectedGraph<int>();
            myGraph.AddVertices(1, 2, 3, 4);
            myGraph.AddEdges(
                Tuple.Create(1, 2),
                Tuple.Create(2, 3),
                Tuple.Create(2, 4),
                Tuple.Create(3, 4)
            );

            myGraph.RemoveEdge(2, 3);

            Assert.That(myGraph.Neighbors(2), Does.Not.Contain(3));
            Assert.That(myGraph.Neighbors(2), Does.Contain(4));
        }

        [Test]
        public void RemoveVertex()
        {
            var myGraph = new DirectedGraph<int>();
            myGraph.AddVertices(1, 2, 3, 4);
            myGraph.AddEdges(
                Tuple.Create(1, 2),
                Tuple.Create(2, 3),
                Tuple.Create(2, 4),
                Tuple.Create(3, 4)
            );

            myGraph.RemoveVertex(2);

            myGraph.Vertices.Should().NotContain(2);
            myGraph.Neighbors(1).Should().NotContain(2);
            myGraph.Neighbors(3).Should().Contain(4);
        }

        [Test]
        public void NodeDFS()
        {
            var myGraph = new DirectedGraph<char>();
            myGraph.AddVertices('a', 'b', 'c', 'd');
            myGraph.AddEdges(
                Tuple.Create('a', 'b'),
                Tuple.Create('a', 'c'),
                Tuple.Create('b', 'd')
            );

            var result = myGraph.NodeDFS();

            Assert.That(result['a'][SearchSymbol.start], Is.EqualTo(1));
            Assert.That(result['a'][SearchSymbol.finish], Is.EqualTo(8));

            Assert.That(result['b'][SearchSymbol.start], Is.EqualTo(2));
            Assert.That(result['b'][SearchSymbol.finish], Is.EqualTo(5));

            Assert.That(result['c'][SearchSymbol.start], Is.EqualTo(6));
            Assert.That(result['c'][SearchSymbol.finish], Is.EqualTo(7));

            Assert.That(result['d'][SearchSymbol.start], Is.EqualTo(3));
            Assert.That(result['d'][SearchSymbol.finish], Is.EqualTo(4));
        }

        [Test]
        public void TopologicalSortSimple()
        {
            var simpleGraph = new DirectedGraph<char>();
            simpleGraph.AddVertices('a', 'b', 'c', 'd');
            simpleGraph.AddEdges(
                Tuple.Create('a', 'b'),
                Tuple.Create('a', 'c'),
                Tuple.Create('b', 'a'),
                Tuple.Create('b', 'd')
            );

            Assert.That(simpleGraph.TopologicalSort(), Is.EqualTo(new char[] { 'a', 'c', 'b', 'd' }));
        }

        [Test]
        public void TopologicalSortComplex()
        {
            var complexGraph = new DirectedGraph<char>();
            complexGraph.AddVertices('a', 'b', 'c', 'd', 'e', 'f', 'g', 't', 'm');
            complexGraph.AddEdges(
                Tuple.Create('a', 'b'),
                Tuple.Create('a', 'c'),
                Tuple.Create('a', 'd'),
                Tuple.Create('b', 'f'),
                Tuple.Create('b', 'g'),
                Tuple.Create('c', 'g'),
                Tuple.Create('e', 't'),
                Tuple.Create('t', 'm')
            );

            Assert.That(
                complexGraph.TopologicalSort(),
                Is.EqualTo(new char[] { 'e', 't', 'm', 'a', 'd', 'c', 'b', 'g', 'f' })
            );
        }

        [Test]
        public void StronglyConnectedComponentsSimple()
        {
            var simpleGraph = new DirectedGraph<int>();
            simpleGraph.AddVertices(1, 2, 3);
            simpleGraph.AddEdges(
                Tuple.Create(1, 2),
                Tuple.Create(2, 3),
                Tuple.Create(3, 2),
                Tuple.Create(2, 1)
            );

            var result = simpleGraph.StronglyConnectedComponents();
            var scc = new int[] { 1, 2, 3 };

            result.Should().ContainEquivalentOf(scc);
            Assert.That(result.Count, Is.EqualTo(1));
        }

        [Test]
        public void StronglyConnectedComponentsMedium()
        {
            var mediumGraph = new DirectedGraph<int>();
            mediumGraph.AddVertices(1, 2, 3, 4);
            mediumGraph.AddEdges(
                Tuple.Create(1, 2),
                Tuple.Create(1, 3),
                Tuple.Create(1, 4),
                Tuple.Create(4, 2),
                Tuple.Create(3, 4),
                Tuple.Create(2, 3)
            );

            var result = mediumGraph.StronglyConnectedComponents();
            var sccA = new int[] { 2, 3, 4 };
            var sccB = new int[] { 1 };

            result.Should().ContainEquivalentOf(sccA);
            result.Should().ContainEquivalentOf(sccB);
            Assert.That(result.Count, Is.EqualTo(2));
        }

        [Test]
        public void StronglyConnectedComponentsComplex()
        {
            var complexGraph = new DirectedGraph<int>();
            complexGraph.AddVertices(1, 2, 3, 4, 5, 6, 7, 8);
            complexGraph.AddEdges(
                Tuple.Create(1, 2),
                Tuple.Create(2, 3),
                Tuple.Create(2, 8),
                Tuple.Create(3, 4),
                Tuple.Create(3, 7),
                Tuple.Create(4, 5),
                Tuple.Create(5, 3),
                Tuple.Create(5, 6),
                Tuple.Create(7, 4),
                Tuple.Create(7, 6),
                Tuple.Create(8, 1),
                Tuple.Create(8, 7)
            );

            var result = complexGraph.StronglyConnectedComponents();
            var sccA = new int[] { 3, 4, 5, 7 };
            var sccB = new int[] { 1, 2, 8 };
            var sccC = new int[] { 6 };

            result.Should().ContainEquivalentOf(sccA);
            result.Should().ContainEquivalentOf(sccB);
            result.Should().ContainEquivalentOf(sccC);
            Assert.That(result.Count, Is.EqualTo(3));
        }

        [Test]
        public void Clone()
        {
            var myGraph = new DirectedGraph<int>();
            myGraph.AddVertices(1, 2, 3, 4);
            myGraph.AddEdges(
                Tuple.Create(1, 1),
                Tuple.Create(1, 2),
                Tuple.Create(2, 3),
                Tuple.Create(2, 1),
                Tuple.Create(3, 4)
            );

            var clone = myGraph.Clone();
            Assert.That(clone, Is.Not.EqualTo(myGraph));
            clone.Vertices.Should().BeEquivalentTo(1, 2, 3, 4);
            clone.Neighbors(1).Should().BeEquivalentTo(1, 2);
            clone.Neighbors(2).Should().BeEquivalentTo(3, 1);
            clone.Neighbors(3).Should().BeEquivalentTo(4);
        }

        [Test]
        public void SubGraph()
        {
            var myGraph = new DirectedGraph<int>();
            myGraph.AddVertices(1, 2, 3, 4);
            myGraph.AddEdges(
                Tuple.Create(1, 1),
                Tuple.Create(1, 2),
                Tuple.Create(2, 3),
                Tuple.Create(2, 1),
                Tuple.Create(3, 4)
            );

            var subGraph = myGraph.SubGraph(1, 2, 3);
            subGraph.Vertices.Should().BeEquivalentTo(1, 2, 3);
            subGraph.Neighbors(1).Should().BeEquivalentTo(1, 2);
            subGraph.Neighbors(2).Should().BeEquivalentTo(1, 3);
            subGraph.Neighbors(3).Should().NotContain(4);
        }

        [Test]
        public void SimpleCyclesSimple()
        {
            var myGraph = new DirectedGraph<int>();
            myGraph.AddVertices(0, 1, 2);
            myGraph.AddEdges(
                Tuple.Create(0, 0),
                Tuple.Create(0, 1),
                Tuple.Create(0, 2),
                Tuple.Create(1, 2),
                Tuple.Create(2, 0),
                Tuple.Create(2, 1),
                Tuple.Create(2, 2)
            );

            var result = myGraph.SimpleCycles();

            var cycleA = new int[] { 0 };
            var cycleB = new int[] { 0, 1, 2 };
            var cycleC = new int[] { 0, 2 };
            var cycleD = new int[] { 1, 2 };
            var cycleE = new int[] { 2 };

            result.Should().ContainEquivalentOf(cycleA);
            result.Should().ContainEquivalentOf(cycleB);
            result.Should().ContainEquivalentOf(cycleC);
            result.Should().ContainEquivalentOf(cycleD);
            result.Should().ContainEquivalentOf(cycleE);
            result.Should().HaveCount(5);
        }

        [Test]
        public void SimpleCyclesComplex()
        {
            var myGraph = new DirectedGraph<int>();
            myGraph.AddVertices(0, 1, 2, 3, 4, 5, 6, 7, 8, 9);
            myGraph.AddEdges(
                Tuple.Create(0, 1),
                Tuple.Create(1, 2),
                Tuple.Create(2, 3),
                Tuple.Create(3, 0),
                Tuple.Create(0, 3),
                Tuple.Create(3, 4),
                Tuple.Create(4, 5),
                Tuple.Create(5, 0),
                Tuple.Create(0, 1),
                Tuple.Create(1, 6),
                Tuple.Create(6, 7),
                Tuple.Create(7, 8),
                Tuple.Create(8, 0),
                Tuple.Create(8, 9)
            );

            var result = myGraph.SimpleCycles();
            var cycleA = new int[] { 0, 3 };
            var cycleB = new int[] { 0, 1, 2, 3, 4, 5 };
            var cycleC = new int[] { 0, 1, 2, 3 };
            var cycleD = new int[] { 0, 3, 4, 5 };
            var cycleE = new int[] { 0, 1, 6, 7, 8 };

            result.Should().ContainEquivalentOf(cycleA);
            result.Should().ContainEquivalentOf(cycleB);
            result.Should().ContainEquivalentOf(cycleC);
            result.Should().ContainEquivalentOf(cycleD);
            result.Should().ContainEquivalentOf(cycleE);
            result.Should().HaveCount(5);
        }

        [Test]
        public void Cyclic()
        {
            var myGraph = new DirectedGraph<int>();
            myGraph.AddVertices(1, 2, 3, 4);
            myGraph.AddEdges(
                Tuple.Create(1, 2),
                Tuple.Create(2, 3),
                Tuple.Create(3, 1),
                Tuple.Create(3, 4)
            );

            Assert.That(myGraph.Cyclic(), Is.True);
        }

        [Test]
        public void Acyclic()
        {
            var myGraph = new DirectedGraph<int>();
            myGraph.AddVertices(1, 2, 3, 4);
            myGraph.AddEdges(
                Tuple.Create(1, 2),
                Tuple.Create(2, 3),
                Tuple.Create(3, 4)
            );

            Assert.That(myGraph.Cyclic(), Is.False);
        }
    }
}
