using NUnit.Framework;
using FluentAssertions;

using System;
using System.Linq;

using MoonTools.Core.Graph;

namespace Tests
{
    struct EdgeData { }

    public class DirectedGraphTest
    {
        EdgeData dummyEdgeData;

        [Test]
        public void AddNode()
        {
            var myGraph = new DirectedGraph<int, EdgeData>();
            myGraph.AddNode(4);

            Assert.That(myGraph.Nodes, Does.Contain(4));
        }

        [Test]
        public void AddNodes()
        {
            var myGraph = new DirectedGraph<int, EdgeData>();
            myGraph.AddNodes(4, 20, 69);

            Assert.IsTrue(myGraph.Exists(4));
            Assert.IsTrue(myGraph.Exists(20));
            Assert.IsTrue(myGraph.Exists(69));
        }

        [Test]
        public void AddEdge()
        {
            var myGraph = new DirectedGraph<int, EdgeData>();
            myGraph.AddNodes(5, 6);
            myGraph.AddEdge(5, 6, dummyEdgeData);

            Assert.That(myGraph.Neighbors(5), Does.Contain(6));
        }

        [Test]
        public void AddEdges()
        {
            var myGraph = new DirectedGraph<int, EdgeData>();
            myGraph.AddNodes(1, 2, 3, 4);
            myGraph.AddEdges(
                (1, 2, dummyEdgeData),
                (2, 3, dummyEdgeData),
                (2, 4, dummyEdgeData),
                (3, 4, dummyEdgeData)
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
            var myGraph = new DirectedGraph<int, EdgeData>();
            myGraph.AddNodes(1, 2, 3, 4);
            myGraph.AddEdges(
                (1, 2, dummyEdgeData),
                (2, 3, dummyEdgeData),
                (2, 4, dummyEdgeData),
                (3, 4, dummyEdgeData)
            );

            myGraph.RemoveEdge(2, 3);

            Assert.That(myGraph.Neighbors(2), Does.Not.Contain(3));
            Assert.That(myGraph.Neighbors(2), Does.Contain(4));
        }

        [Test]
        public void RemoveNode()
        {
            var myGraph = new DirectedGraph<int, EdgeData>();
            myGraph.AddNodes(1, 2, 3, 4);
            myGraph.AddEdges(
                (1, 2, dummyEdgeData),
                (2, 3, dummyEdgeData),
                (2, 4, dummyEdgeData),
                (3, 4, dummyEdgeData)
            );

            myGraph.RemoveNode(2);

            myGraph.Nodes.Should().NotContain(2);
            myGraph.Neighbors(1).Should().NotContain(2);
            myGraph.Neighbors(3).Should().Contain(4);
        }

        [Test]
        public void NodeDFS()
        {
            var myGraph = new DirectedGraph<char, EdgeData>();
            myGraph.AddNodes('a', 'b', 'c', 'd');
            myGraph.AddEdges(
                ('a', 'b', dummyEdgeData),
                ('a', 'c', dummyEdgeData),
                ('b', 'd', dummyEdgeData)
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
            var simpleGraph = new DirectedGraph<char, EdgeData>();
            simpleGraph.AddNodes('a', 'b', 'c', 'd');
            simpleGraph.AddEdges(
                ('a', 'b', dummyEdgeData),
                ('a', 'c', dummyEdgeData),
                ('b', 'a', dummyEdgeData),
                ('b', 'd', dummyEdgeData)
            );

            Assert.That(simpleGraph.TopologicalSort(), Is.EqualTo(new char[] { 'a', 'c', 'b', 'd' }));
        }

        [Test]
        public void TopologicalSortComplex()
        {
            var complexGraph = new DirectedGraph<char, EdgeData>();
            complexGraph.AddNodes('a', 'b', 'c', 'd', 'e', 'f', 'g', 't', 'm');
            complexGraph.AddEdges(
                ('a', 'b', dummyEdgeData),
                ('a', 'c', dummyEdgeData),
                ('a', 'd', dummyEdgeData),
                ('b', 'f', dummyEdgeData),
                ('b', 'g', dummyEdgeData),
                ('c', 'g', dummyEdgeData),
                ('e', 't', dummyEdgeData),
                ('t', 'm', dummyEdgeData)
            );

            Assert.That(
                complexGraph.TopologicalSort(),
                Is.EqualTo(new char[] { 'e', 't', 'm', 'a', 'd', 'c', 'b', 'g', 'f' })
            );
        }

        [Test]
        public void StronglyConnectedComponentsSimple()
        {
            var simpleGraph = new DirectedGraph<int, EdgeData>();
            simpleGraph.AddNodes(1, 2, 3);
            simpleGraph.AddEdges(
                (1, 2, dummyEdgeData),
                (2, 3, dummyEdgeData),
                (3, 2, dummyEdgeData),
                (2, 1, dummyEdgeData)
            );

            var result = simpleGraph.StronglyConnectedComponents();
            var scc = new int[] { 1, 2, 3 };

            result.Should().ContainEquivalentOf(scc);
            Assert.That(result.Count, Is.EqualTo(1));
        }

        [Test]
        public void StronglyConnectedComponentsMedium()
        {
            var mediumGraph = new DirectedGraph<int, EdgeData>();
            mediumGraph.AddNodes(1, 2, 3, 4);
            mediumGraph.AddEdges(
                (1, 2, dummyEdgeData),
                (1, 3, dummyEdgeData),
                (1, 4, dummyEdgeData),
                (4, 2, dummyEdgeData),
                (3, 4, dummyEdgeData),
                (2, 3, dummyEdgeData)
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
            var complexGraph = new DirectedGraph<int, EdgeData>();
            complexGraph.AddNodes(1, 2, 3, 4, 5, 6, 7, 8);
            complexGraph.AddEdges(
                (1, 2, dummyEdgeData),
                (2, 3, dummyEdgeData),
                (2, 8, dummyEdgeData),
                (3, 4, dummyEdgeData),
                (3, 7, dummyEdgeData),
                (4, 5, dummyEdgeData),
                (5, 3, dummyEdgeData),
                (5, 6, dummyEdgeData),
                (7, 4, dummyEdgeData),
                (7, 6, dummyEdgeData),
                (8, 1, dummyEdgeData),
                (8, 7, dummyEdgeData)
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
            var myGraph = new DirectedGraph<int, EdgeData>();
            myGraph.AddNodes(1, 2, 3, 4);
            myGraph.AddEdges(
                (1, 1, dummyEdgeData),
                (1, 2, dummyEdgeData),
                (2, 3, dummyEdgeData),
                (2, 1, dummyEdgeData),
                (3, 4, dummyEdgeData)
            );

            var clone = myGraph.Clone();
            Assert.That(clone, Is.Not.EqualTo(myGraph));
            clone.Nodes.Should().BeEquivalentTo(1, 2, 3, 4);
            clone.Neighbors(1).Should().BeEquivalentTo(1, 2);
            clone.Neighbors(2).Should().BeEquivalentTo(3, 1);
            clone.Neighbors(3).Should().BeEquivalentTo(4);
        }

        [Test]
        public void SubGraph()
        {
            var myGraph = new DirectedGraph<int, EdgeData>();
            myGraph.AddNodes(1, 2, 3, 4);
            myGraph.AddEdges(
                (1, 1, dummyEdgeData),
                (1, 2, dummyEdgeData),
                (2, 3, dummyEdgeData),
                (2, 1, dummyEdgeData),
                (3, 4, dummyEdgeData)
            );

            var subGraph = myGraph.SubGraph(1, 2, 3);
            subGraph.Nodes.Should().BeEquivalentTo(1, 2, 3);
            subGraph.Neighbors(1).Should().BeEquivalentTo(1, 2);
            subGraph.Neighbors(2).Should().BeEquivalentTo(1, 3);
            subGraph.Neighbors(3).Should().NotContain(4);
        }

        [Test]
        public void SimpleCyclesSimple()
        {
            var myGraph = new DirectedGraph<int, EdgeData>();
            myGraph.AddNodes(0, 1, 2);
            myGraph.AddEdges(
                (0, 0, dummyEdgeData),
                (0, 1, dummyEdgeData),
                (0, 2, dummyEdgeData),
                (1, 2, dummyEdgeData),
                (2, 0, dummyEdgeData),
                (2, 1, dummyEdgeData),
                (2, 2, dummyEdgeData)
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
            var myGraph = new DirectedGraph<int, EdgeData>();
            myGraph.AddNodes(0, 1, 2, 3, 4, 5, 6, 7, 8, 9);
            myGraph.AddEdges(
                (0, 1, dummyEdgeData),
                (1, 2, dummyEdgeData),
                (2, 3, dummyEdgeData),
                (3, 0, dummyEdgeData),
                (0, 3, dummyEdgeData),
                (3, 4, dummyEdgeData),
                (4, 5, dummyEdgeData),
                (5, 0, dummyEdgeData),
                (1, 6, dummyEdgeData),
                (6, 7, dummyEdgeData),
                (7, 8, dummyEdgeData),
                (8, 0, dummyEdgeData),
                (8, 9, dummyEdgeData)
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
            var myGraph = new DirectedGraph<int, EdgeData>();
            myGraph.AddNodes(1, 2, 3, 4);
            myGraph.AddEdges(
                (1, 2, dummyEdgeData),
                (2, 3, dummyEdgeData),
                (3, 1, dummyEdgeData),
                (3, 4, dummyEdgeData)
            );

            Assert.That(myGraph.Cyclic(), Is.True);
        }

        [Test]
        public void Acyclic()
        {
            var myGraph = new DirectedGraph<int, EdgeData>();
            myGraph.AddNodes(1, 2, 3, 4);
            myGraph.AddEdges(
                (1, 2, dummyEdgeData),
                (2, 3, dummyEdgeData),
                (3, 4, dummyEdgeData)
            );

            Assert.That(myGraph.Cyclic(), Is.False);
        }

        [Test]
        public void Clear()
        {
            var myGraph = new DirectedGraph<int, EdgeData>();
            myGraph.AddNodes(1, 2, 3, 4);
            myGraph.AddEdges(
                (1, 2, dummyEdgeData),
                (2, 3, dummyEdgeData),
                (3, 4, dummyEdgeData)
            );

            myGraph.Clear();

            myGraph.Nodes.Should().BeEmpty();
        }

        [Test]
        public void EdgeExists()
        {
            var myGraph = new DirectedGraph<int, EdgeData>();
            myGraph.AddNodes(1, 2);
            myGraph.AddEdge(1, 2, dummyEdgeData);

            myGraph.Exists((1, 2)).Should().BeTrue();
        }

        struct TestEdgeData
        {
            public int testNum;
        }

        [Test]
        public void EdgeData()
        {
            var myGraph = new DirectedGraph<int, TestEdgeData>();
            myGraph.AddNodes(1, 2);
            myGraph.AddEdge(1, 2, new TestEdgeData { testNum = 4 });

            myGraph.EdgeData((1, 2)).testNum.Should().Be(4);
        }
    }
}
