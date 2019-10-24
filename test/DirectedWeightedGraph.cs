using NUnit.Framework;
using FluentAssertions;

using MoonTools.Core.Graph;
using System;
using System.Linq;

namespace Tests
{
    public class DirectedWeightedGraphTests
    {
        EdgeData dummyEdgeData;

        [Test]
        public void AddNode()
        {
            var myGraph = new DirectedWeightedGraph<int, EdgeData>();
            myGraph.AddNode(4);

            myGraph.Exists(4).Should().BeTrue();
        }

        [Test]
        public void AddNodes()
        {
            var myGraph = new DirectedWeightedGraph<int, EdgeData>();
            myGraph.AddNodes(4, 20, 69);

            myGraph.Exists(4).Should().BeTrue();
            myGraph.Exists(20).Should().BeTrue();
            myGraph.Exists(69).Should().BeTrue();
        }

        [Test]
        public void AddEdge()
        {
            var myGraph = new DirectedWeightedGraph<int, EdgeData>();
            myGraph.AddNodes(5, 6);
            myGraph.AddEdge(5, 6, 10, dummyEdgeData);

            myGraph.Neighbors(5).Should().Contain(6);
            myGraph.Weight(5, 6).Should().Be(10);
            myGraph.EdgeData(5, 6).Should().Be(dummyEdgeData);

            myGraph.Invoking(x => x.AddEdge(5, 6, 3, dummyEdgeData)).Should().Throw<ArgumentException>();
        }

        [Test]
        public void AddEdges()
        {
            var a = new NumEdgeData { testNum = 1 };
            var b = new NumEdgeData { testNum = 2 };
            var c = new NumEdgeData { testNum = 3 };
            var d = new NumEdgeData { testNum = 4 };

            var myGraph = new DirectedWeightedGraph<int, NumEdgeData>();
            myGraph.AddNodes(1, 2, 3, 4);
            myGraph.AddEdges(
                (1, 2, 5, a),
                (2, 3, 6, b),
                (2, 4, 7, c),
                (3, 4, 8, d)
            );

            myGraph.Neighbors(1).Should().Contain(2);
            myGraph.Neighbors(2).Should().Contain(3);
            myGraph.Neighbors(2).Should().Contain(4);
            myGraph.Neighbors(3).Should().Contain(4);
            myGraph.Neighbors(1).Should().NotContain(4);

            myGraph.Weight(1, 2).Should().Be(5);
            myGraph.Weight(2, 3).Should().Be(6);
            myGraph.Weight(2, 4).Should().Be(7);
            myGraph.Weight(3, 4).Should().Be(8);

            myGraph.EdgeData(1, 2).Should().Be(a);
            myGraph.EdgeData(2, 3).Should().Be(b);
            myGraph.EdgeData(2, 4).Should().Be(c);
            myGraph.EdgeData(3, 4).Should().Be(d);

            myGraph.Invoking(x => x.AddEdge(2, 4, 9, d)).Should().Throw<ArgumentException>();
        }

        [Test]
        public void Clear()
        {
            var myGraph = new DirectedWeightedGraph<int, EdgeData>();
            myGraph.AddNodes(1, 2, 3, 4);
            myGraph.AddEdges(
                (1, 2, 5, dummyEdgeData),
                (2, 3, 6, dummyEdgeData),
                (2, 4, 7, dummyEdgeData)
            );

            myGraph.Clear();

            myGraph.Nodes.Should().BeEmpty();
            myGraph.Invoking(x => x.Neighbors(1)).Should().Throw<ArgumentException>();
            myGraph.Invoking(x => x.Weight(1, 2)).Should().Throw<ArgumentException>();
            myGraph.Invoking(x => x.EdgeData(1, 2)).Should().Throw<ArgumentException>();
        }

        [Test]
        public void NodeExists()
        {
            var myGraph = new DirectedWeightedGraph<int, EdgeData>();
            myGraph.AddNodes(1, 2, 3);
            myGraph.AddEdges(
                (1, 2, 4, dummyEdgeData),
                (2, 3, 5, dummyEdgeData)
            );

            myGraph.Exists(1).Should().BeTrue();
            myGraph.Exists(2).Should().BeTrue();
            myGraph.Exists(3).Should().BeTrue();
            myGraph.Exists(4).Should().BeFalse();
        }

        [Test]
        public void EdgeExists()
        {
            var myGraph = new DirectedWeightedGraph<int, EdgeData>();
            myGraph.AddNodes(1, 2, 3);
            myGraph.AddEdges(
                (1, 2, 4, dummyEdgeData),
                (2, 3, 5, dummyEdgeData)
            );

            myGraph.Exists(1, 2).Should().BeTrue();
            myGraph.Exists(2, 3).Should().BeTrue();
            myGraph.Exists(1, 3).Should().BeFalse();
            myGraph.Invoking(x => x.Exists(3, 4)).Should().Throw<ArgumentException>();
        }

        [Test]
        public void Neighbors()
        {
            var myGraph = new DirectedWeightedGraph<int, EdgeData>();
            myGraph.AddNodes(1, 2, 3);
            myGraph.AddEdges(
                (1, 2, 4, dummyEdgeData),
                (2, 3, 5, dummyEdgeData)
            );

            myGraph.Neighbors(1).Should().Contain(2);
            myGraph.Neighbors(2).Should().Contain(3);
            myGraph.Neighbors(1).Should().NotContain(3);
            myGraph.Invoking(x => x.Neighbors(4)).Should().Throw<System.ArgumentException>();
        }

        [Test]
        public void Weight()
        {
            var myGraph = new DirectedWeightedGraph<int, EdgeData>();
            myGraph.AddNodes(1, 2, 3);
            myGraph.AddEdges(
                (1, 2, 4, dummyEdgeData),
                (2, 3, 5, dummyEdgeData)
            );

            myGraph.Weight(1, 2).Should().Be(4);
            myGraph.Weight(2, 3).Should().Be(5);
            myGraph.Invoking(x => x.Weight(3, 4)).Should().Throw<System.ArgumentException>();
        }

        [Test]
        public void EdgeData()
        {
            var a = new NumEdgeData { testNum = 3 };
            var b = new NumEdgeData { testNum = 5 };

            var myGraph = new DirectedWeightedGraph<int, NumEdgeData>();
            myGraph.AddNodes(1, 2, 3);
            myGraph.AddEdges(
                (1, 2, 4, a),
                (2, 3, 5, b)
            );

            myGraph.EdgeData(1, 2).Should().Be(a);
            myGraph.EdgeData(2, 3).Should().Be(b);
            myGraph.Invoking(x => x.EdgeData(2, 4)).Should().Throw<ArgumentException>();
        }

        [Test]
        public void AStarShortestPath()
        {
            var run = new MoveTypeEdgeData { moveType = MoveType.Run };
            var jump = new MoveTypeEdgeData { moveType = MoveType.Jump };
            var wallJump = new MoveTypeEdgeData { moveType = MoveType.WallJump };

            var myGraph = new DirectedWeightedGraph<char, MoveTypeEdgeData>();
            myGraph.AddNodes('a', 'b', 'c', 'd', 'e', 'f', 'g', 'h');
            myGraph.AddEdges(
                ('a', 'b', 2, run),
                ('a', 'c', 3, run),
                ('a', 'e', 4, wallJump),
                ('b', 'd', 2, jump),
                ('b', 'e', 1, run),
                ('c', 'g', 4, jump),
                ('c', 'h', 11, run),
                ('d', 'c', 3, jump),
                ('d', 'f', 2, run),
                ('d', 'h', 3, wallJump),
                ('e', 'f', 5, run),
                ('f', 'd', 2, run),
                ('f', 'h', 6, wallJump),
                ('g', 'h', 7, run),
                ('h', 'f', 1, jump)
            );

            myGraph
                .AStarShortestPath('a', 'h', (x, y) => 1)
                .Select(edge => myGraph.EdgeData(edge.Item1, edge.Item2))
                .Should()
                .ContainInOrder(
                    run, jump, wallJump
                )
                .And
                .HaveCount(3);

            // have to call Count() because otherwise the lazy evaluation wont trigger
            myGraph.Invoking(x => x.AStarShortestPath('a', 'z', (x, y) => 1).Count()).Should().Throw<System.ArgumentException>();
        }

        [Test]
        public void DijkstraSingleSourceShortestPath()
        {
            var run = new MoveTypeEdgeData { moveType = MoveType.Run };
            var jump = new MoveTypeEdgeData { moveType = MoveType.Jump };
            var wallJump = new MoveTypeEdgeData { moveType = MoveType.WallJump };

            var myGraph = new DirectedWeightedGraph<char, MoveTypeEdgeData>();
            myGraph.AddNodes('a', 'b', 'c', 'd', 'e', 'f', 'g', 'h');
            myGraph.AddEdges(
                ('a', 'b', 2, run),
                ('a', 'c', 3, run),
                ('a', 'e', 4, wallJump),
                ('b', 'd', 2, jump),
                ('b', 'e', 1, run),
                ('c', 'g', 4, jump),
                ('c', 'h', 11, run),
                ('d', 'c', 3, jump),
                ('d', 'f', 2, run),
                ('d', 'h', 3, wallJump),
                ('e', 'f', 5, run),
                ('f', 'd', 2, run),
                ('f', 'h', 6, wallJump),
                ('g', 'h', 7, run),
                ('h', 'f', 1, jump)
            );

            myGraph
                .DijkstraSingleSourceShortestPath('a')
                .Should()
                .Contain(('b', 'a', 2)).And
                .Contain(('c', 'a', 3)).And
                .Contain(('d', 'b', 4)).And
                .Contain(('e', 'b', 3)).And
                .Contain(('f', 'd', 6)).And
                .Contain(('g', 'c', 7)).And
                .Contain(('h', 'd', 7)).And
                .HaveCount(7);

            // have to call Count() because otherwise the lazy evaluation wont trigger
            myGraph.Invoking(x => x.DijkstraSingleSourceShortestPath('z').Count()).Should().Throw<System.ArgumentException>();
        }
    }
}