using NUnit.Framework;
using FluentAssertions;

using MoonTools.Core.Graph;
using System.Linq;

namespace Tests
{
    public class DirectedWeightedMultiGraphTests
    {
        EdgeData dummyEdgeData;

        [Test]
        public void AddNode()
        {
            var myGraph = new DirectedWeightedMultiGraph<int, EdgeData>();
            myGraph.AddNode(4);

            Assert.That(myGraph.Nodes, Does.Contain(4));
        }

        [Test]
        public void AddNodes()
        {
            var myGraph = new DirectedWeightedMultiGraph<int, EdgeData>();
            myGraph.AddNodes(4, 20, 69);

            myGraph.Exists(4).Should().BeTrue();
            myGraph.Exists(20).Should().BeTrue();
            myGraph.Exists(69).Should().BeTrue();
        }

        [Test]
        public void AddEdge()
        {
            var myGraph = new DirectedWeightedMultiGraph<int, EdgeData>();
            myGraph.AddNodes(5, 6);
            myGraph.AddEdge(5, 6, 10, dummyEdgeData);

            myGraph.Neighbors(5).Should().Contain(6);
        }

        [Test]
        public void AddEdges()
        {
            var myGraph = new DirectedWeightedMultiGraph<int, EdgeData>();
            myGraph.AddNodes(1, 2, 3, 4);
            myGraph.AddEdges(
                (1, 2, 5, dummyEdgeData),
                (2, 3, 6, dummyEdgeData),
                (2, 4, 7, dummyEdgeData),
                (3, 4, 8, dummyEdgeData)
            );

            myGraph.Neighbors(1).Should().Contain(2);
            myGraph.Neighbors(2).Should().Contain(3);
            myGraph.Neighbors(2).Should().Contain(4);
            myGraph.Neighbors(3).Should().Contain(4);
            myGraph.Neighbors(1).Should().NotContain(4);

            myGraph.EdgeIDs(1, 2).Should().HaveCount(1);
            myGraph.Weights(1, 2).Should().Contain(5);
        }

        [Test]
        public void AddMultiEdges()
        {
            var myGraph = new DirectedWeightedMultiGraph<int, EdgeData>();
            myGraph.AddNodes(1, 2, 3, 4);
            myGraph.AddEdges(
                (1, 2, 5, dummyEdgeData),
                (2, 3, 6, dummyEdgeData),
                (2, 4, 7, dummyEdgeData),
                (2, 4, 8, dummyEdgeData)
            );

            myGraph.Neighbors(1).Should().Contain(2);
            myGraph.Neighbors(2).Should().Contain(3);
            myGraph.Neighbors(2).Should().Contain(4);
            myGraph.Neighbors(1).Should().NotContain(4);

            myGraph.EdgeIDs(2, 4).Should().HaveCount(2);
            myGraph.Weights(2, 4).Should().HaveCount(2);
            myGraph.Weights(2, 4).Should().Contain(7);
            myGraph.Weights(2, 4).Should().Contain(8);
        }

        [Test]
        public void Clear()
        {
            var myGraph = new DirectedWeightedMultiGraph<int, EdgeData>();
            myGraph.AddNodes(1, 2, 3, 4);
            myGraph.AddEdges(
                (1, 2, 5, dummyEdgeData),
                (2, 3, 6, dummyEdgeData),
                (2, 4, 7, dummyEdgeData),
                (2, 4, 8, dummyEdgeData)
            );

            myGraph.Clear();

            myGraph.Invoking(x => x.Neighbors(1)).Should().Throw<System.ArgumentException>();
            myGraph.Invoking(x => x.Weights(1, 2)).Should().Throw<System.ArgumentException>();
            myGraph.Invoking(x => x.EdgeIDs(1, 2)).Should().Throw<System.ArgumentException>();
        }

        [Test]
        public void Edges()
        {
            var myGraph = new DirectedWeightedMultiGraph<int, EdgeData>();
            myGraph.AddNodes(1, 2, 3);
            myGraph.AddEdges(
                (1, 2, 4, dummyEdgeData),
                (1, 2, 3, dummyEdgeData),
                (2, 3, 5, dummyEdgeData)
            );

            myGraph.EdgeIDs(1, 2).Should().HaveCount(2);
            myGraph.EdgeIDs(2, 3).Should().HaveCount(1);
        }

        [Test]
        public void NodeExists()
        {
            var myGraph = new DirectedWeightedMultiGraph<int, EdgeData>();
            myGraph.AddNodes(1, 2, 3);
            myGraph.AddEdges(
                (1, 2, 4, dummyEdgeData),
                (1, 2, 3, dummyEdgeData),
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
            var myGraph = new DirectedWeightedMultiGraph<int, EdgeData>();
            myGraph.AddNodes(1, 2, 3);
            myGraph.AddEdges(
                (1, 2, 4, dummyEdgeData),
                (1, 2, 3, dummyEdgeData),
                (2, 3, 5, dummyEdgeData)
            );

            myGraph.Exists(1, 2).Should().BeTrue();
            myGraph.Exists(2, 3).Should().BeTrue();
            myGraph.Exists(1, 3).Should().BeFalse();
            myGraph.Invoking(x => x.Exists(3, 4)).Should().Throw<System.ArgumentException>();
        }

        [Test]
        public void Neighbors()
        {
            var myGraph = new DirectedWeightedMultiGraph<int, EdgeData>();
            myGraph.AddNodes(1, 2, 3);
            myGraph.AddEdges(
                (1, 2, 4, dummyEdgeData),
                (1, 2, 3, dummyEdgeData),
                (2, 3, 5, dummyEdgeData)
            );

            myGraph.Neighbors(1).Should().Contain(2);
            myGraph.Neighbors(2).Should().Contain(3);
            myGraph.Neighbors(1).Should().NotContain(3);
            myGraph.Invoking(x => x.Neighbors(4)).Should().Throw<System.ArgumentException>();
        }

        [Test]
        public void Weights()
        {
            var myGraph = new DirectedWeightedMultiGraph<int, EdgeData>();
            myGraph.AddNodes(1, 2, 3);
            myGraph.AddEdges(
                (1, 2, 4, dummyEdgeData),
                (1, 2, 3, dummyEdgeData),
                (2, 3, 5, dummyEdgeData)
            );

            myGraph.Weights(1, 2).Should().Contain(3);
            myGraph.Weights(1, 2).Should().Contain(4);
            myGraph.Weights(2, 3).Should().Contain(5);
            myGraph.Invoking(x => x.Weights(3, 4)).Should().Throw<System.ArgumentException>();
        }

        [Test]
        public void EdgeData()
        {
            var a = new NumEdgeData { testNum = 3 };
            var b = new NumEdgeData { testNum = 4 };
            var c = new NumEdgeData { testNum = 5 };

            var myGraph = new DirectedWeightedMultiGraph<int, NumEdgeData>();
            myGraph.AddNodes(1, 2, 3);
            myGraph.AddEdges(
                (1, 2, 4, a),
                (1, 2, 3, b),
                (2, 3, 5, c)
            );

            myGraph.EdgeIDs(1, 2).Select(id => myGraph.EdgeData(id)).Should().Contain(a);
            myGraph.EdgeIDs(1, 2).Select(id => myGraph.EdgeData(id)).Should().Contain(b);
            myGraph.EdgeIDs(2, 3).Select(id => myGraph.EdgeData(id)).Should().Contain(c);
            myGraph.Invoking(x => x.EdgeData(new System.Guid())).Should().Throw<System.ArgumentException>();
        }

        [Test]
        public void AStarShortestPath()
        {
            var run = new MoveTypeEdgeData { moveType = MoveType.Run };
            var jump = new MoveTypeEdgeData { moveType = MoveType.Jump };
            var wallJump = new MoveTypeEdgeData { moveType = MoveType.WallJump };

            var myGraph = new DirectedWeightedMultiGraph<char, MoveTypeEdgeData>();
            myGraph.AddNodes('a', 'b', 'c', 'd', 'e', 'f', 'g', 'h');
            myGraph.AddEdges(
                ('a', 'b', 2, run),
                ('a', 'c', 1, jump),
                ('a', 'c', 3, run),
                ('a', 'e', 4, wallJump),
                ('b', 'd', 2, jump),
                ('b', 'd', 5, run),
                ('b', 'e', 1, run),
                ('c', 'g', 2, run),
                ('c', 'g', 4, jump),
                ('c', 'h', 11, run),
                ('d', 'c', 3, jump),
                ('d', 'f', 2, run),
                ('d', 'h', 3, wallJump),
                ('e', 'f', 5, run),
                ('f', 'd', 2, run),
                ('f', 'h', 6, wallJump),
                ('g', 'h', 7, run),
                ('h', 'f', 1, jump),
                ('a', 'a', 3, jump) // cheeky lil self-edge
            );

            myGraph
                .AStarShortestPath('a', 'h', (x, y) => 1)
                .Select(id => myGraph.EdgeData(id))
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
        public void DijsktraSingleSourceShortestPath()
        {
            var run = new MoveTypeEdgeData { moveType = MoveType.Run };
            var jump = new MoveTypeEdgeData { moveType = MoveType.Jump };
            var wallJump = new MoveTypeEdgeData { moveType = MoveType.WallJump };

            var myGraph = new DirectedWeightedMultiGraph<char, MoveTypeEdgeData>();
            myGraph.AddNodes('a', 'b', 'c', 'd', 'e', 'f', 'g', 'h');

            var edgeA = myGraph.AddEdge('a', 'b', 2, run);
            var edgeB = myGraph.AddEdge('a', 'c', 1, jump);
            var edgeC = myGraph.AddEdge('b', 'd', 2, jump);
            var edgeD = myGraph.AddEdge('b', 'e', 1, run);
            var edgeE = myGraph.AddEdge('d', 'f', 2, run);
            var edgeF = myGraph.AddEdge('c', 'g', 2, run);
            var edgeG = myGraph.AddEdge('d', 'h', 3, wallJump);

            myGraph.AddEdges(
                ('a', 'c', 3, run),
                ('a', 'e', 4, wallJump),
                ('b', 'd', 5, run),
                ('c', 'g', 4, jump),
                ('c', 'h', 11, run),
                ('d', 'c', 3, jump),
                ('e', 'f', 5, run),
                ('f', 'd', 2, run),
                ('f', 'h', 6, wallJump),
                ('g', 'h', 7, run),
                ('h', 'f', 1, jump),
                ('a', 'a', 3, jump) // cheeky lil self-edge
            );

            myGraph
                .DijkstraSingleSourceShortestPath('a')
                .Should()
                .Contain(('b', edgeA, 2)).And
                .Contain(('c', edgeB, 1)).And
                .Contain(('d', edgeC, 4)).And
                .Contain(('e', edgeD, 3)).And
                .Contain(('f', edgeE, 6)).And
                .Contain(('g', edgeF, 3)).And
                .Contain(('h', edgeG, 7)).And
                .HaveCount(7);

            // have to call Count() because otherwise the lazy evaluation wont trigger
            myGraph.Invoking(x => x.DijkstraSingleSourceShortestPath('z').Count()).Should().Throw<System.ArgumentException>();
        }

        [Test]
        public void DijkstraShortestPath()
        {
            var run = new MoveTypeEdgeData { moveType = MoveType.Run };
            var jump = new MoveTypeEdgeData { moveType = MoveType.Jump };
            var wallJump = new MoveTypeEdgeData { moveType = MoveType.WallJump };

            var myGraph = new DirectedWeightedMultiGraph<char, MoveTypeEdgeData>();
            myGraph.AddNodes('a', 'b', 'c', 'd', 'e', 'f', 'g', 'h');

            var edgeA = myGraph.AddEdge('a', 'b', 2, run);
            var edgeB = myGraph.AddEdge('a', 'c', 1, jump);
            var edgeC = myGraph.AddEdge('b', 'd', 2, jump);
            var edgeD = myGraph.AddEdge('b', 'e', 1, run);
            var edgeE = myGraph.AddEdge('d', 'f', 2, run);
            var edgeF = myGraph.AddEdge('c', 'g', 2, run);
            var edgeG = myGraph.AddEdge('d', 'h', 3, wallJump);

            myGraph.AddEdges(
                ('a', 'c', 3, run),
                ('a', 'e', 4, wallJump),
                ('b', 'd', 5, run),
                ('c', 'g', 4, jump),
                ('c', 'h', 11, run),
                ('d', 'c', 3, jump),
                ('e', 'f', 5, run),
                ('f', 'd', 2, run),
                ('f', 'h', 6, wallJump),
                ('g', 'h', 7, run),
                ('h', 'f', 1, jump),
                ('a', 'a', 3, jump) // cheeky lil self-edge
            );

            myGraph
                .DijkstraShortestPath('a', 'h')
                .Select(edgeID => myGraph.EdgeData(edgeID))
                .Should()
                .ContainInOrder(
                    run, jump, wallJump
                )
                .And
                .HaveCount(3);

            myGraph.Invoking(x => x.DijkstraShortestPath('a', 'z').Count()).Should().Throw<System.ArgumentException>();
        }

        [Test]
        public void BellmanFordSingleSourceShortestPath()
        {
            var run = new MoveTypeEdgeData { moveType = MoveType.Run };
            var jump = new MoveTypeEdgeData { moveType = MoveType.Jump };
            var wallJump = new MoveTypeEdgeData { moveType = MoveType.WallJump };

            var myGraph = new DirectedWeightedMultiGraph<char, MoveTypeEdgeData>();
            myGraph.AddNodes('a', 'b', 'c', 'd', 'e', 'f', 'g', 'h');

            var edgeA = myGraph.AddEdge('a', 'b', 2, run);
            var edgeB = myGraph.AddEdge('a', 'c', 1, jump);
            var edgeC = myGraph.AddEdge('b', 'd', 2, jump);
            var edgeD = myGraph.AddEdge('b', 'e', 1, run);
            var edgeE = myGraph.AddEdge('d', 'f', 2, run);
            var edgeF = myGraph.AddEdge('c', 'g', 2, run);
            var edgeG = myGraph.AddEdge('d', 'h', 3, wallJump);

            myGraph.AddEdges(
                ('a', 'c', 3, run),
                ('a', 'e', 4, wallJump),
                ('b', 'd', 5, run),
                ('c', 'g', 4, jump),
                ('c', 'h', 11, run),
                ('d', 'c', 3, jump),
                ('e', 'f', 5, run),
                ('f', 'd', 2, run),
                ('f', 'h', 6, wallJump),
                ('g', 'h', 7, run),
                ('h', 'f', 1, jump),
                ('a', 'a', 3, jump) // cheeky lil self-edge
            );

            myGraph
                .BellmanFordSingleSourceShortestPath('a')
                .Should()
                .Contain(('b', edgeA, 2)).And
                .Contain(('c', edgeB, 1)).And
                .Contain(('d', edgeC, 4)).And
                .Contain(('e', edgeD, 3)).And
                .Contain(('f', edgeE, 6)).And
                .Contain(('g', edgeF, 3)).And
                .Contain(('h', edgeG, 7)).And
                .HaveCount(7);

            // have to call Count() because otherwise the lazy evaluation wont trigger
            myGraph.Invoking(x => x.BellmanFordSingleSourceShortestPath('z').Count()).Should().Throw<System.ArgumentException>();
        }

        [Test]
        public void BellmanFordSingleSourceShortestPathNegative()
        {
            var run = new MoveTypeEdgeData { moveType = MoveType.Run };
            var jump = new MoveTypeEdgeData { moveType = MoveType.Jump };
            var wallJump = new MoveTypeEdgeData { moveType = MoveType.WallJump };

            var myGraph = new DirectedWeightedMultiGraph<char, MoveTypeEdgeData>();
            myGraph.AddNodes('a', 'b', 'c', 'd', 'e', 'f', 'g', 'h');

            var edgeA = myGraph.AddEdge('a', 'b', 2, run);
            var edgeB = myGraph.AddEdge('a', 'c', 1, jump);
            var edgeC = myGraph.AddEdge('b', 'd', -1, jump);
            var edgeD = myGraph.AddEdge('b', 'e', 1, run);
            var edgeE = myGraph.AddEdge('d', 'f', 2, run);
            var edgeF = myGraph.AddEdge('c', 'g', 2, run);
            var edgeG = myGraph.AddEdge('d', 'h', 3, wallJump);

            myGraph.AddEdges(
                ('a', 'c', 3, run),
                ('a', 'e', 4, wallJump),
                ('b', 'd', 5, run),
                ('c', 'g', 4, jump),
                ('c', 'h', 11, run),
                ('d', 'c', 3, jump),
                ('e', 'f', 5, run),
                ('f', 'd', 2, run),
                ('f', 'h', 6, wallJump),
                ('g', 'h', 7, run),
                ('h', 'f', 1, jump),
                ('a', 'a', 3, jump) // cheeky lil self-edge
            );

            myGraph
                .BellmanFordSingleSourceShortestPath('a')
                .Should()
                .Contain(('b', edgeA, 2)).And
                .Contain(('c', edgeB, 1)).And
                .Contain(('d', edgeC, 1)).And
                .Contain(('e', edgeD, 3)).And
                .Contain(('f', edgeE, 3)).And
                .Contain(('g', edgeF, 3)).And
                .Contain(('h', edgeG, 4)).And
                .HaveCount(7);
        }

        [Test]
        public void BellmanFordShortestPath()
        {
            var run = new MoveTypeEdgeData { moveType = MoveType.Run };
            var jump = new MoveTypeEdgeData { moveType = MoveType.Jump };
            var wallJump = new MoveTypeEdgeData { moveType = MoveType.WallJump };

            var myGraph = new DirectedWeightedMultiGraph<char, MoveTypeEdgeData>();
            myGraph.AddNodes('a', 'b', 'c', 'd', 'e', 'f', 'g', 'h');

            var edgeA = myGraph.AddEdge('a', 'b', 2, run);
            var edgeB = myGraph.AddEdge('a', 'c', 1, jump);
            var edgeC = myGraph.AddEdge('b', 'd', 2, jump);
            var edgeD = myGraph.AddEdge('b', 'e', 1, run);
            var edgeE = myGraph.AddEdge('d', 'f', 2, run);
            var edgeF = myGraph.AddEdge('c', 'g', 2, run);
            var edgeG = myGraph.AddEdge('d', 'h', 3, wallJump);

            myGraph.AddEdges(
                ('a', 'c', 3, run),
                ('a', 'e', 4, wallJump),
                ('b', 'd', 5, run),
                ('c', 'g', 4, jump),
                ('c', 'h', 11, run),
                ('d', 'c', 3, jump),
                ('e', 'f', 5, run),
                ('f', 'd', 2, run),
                ('f', 'h', 6, wallJump),
                ('g', 'h', 7, run),
                ('h', 'f', 1, jump),
                ('a', 'a', 3, jump) // cheeky lil self-edge
            );

            myGraph
                .BellmanFordShortestPath('a', 'h')
                .Select(edgeID => myGraph.EdgeData(edgeID))
                .Should()
                .ContainInOrder(
                    run, jump, wallJump
                )
                .And
                .HaveCount(3);

            myGraph.Invoking(x => x.BellmanFordShortestPath('a', 'z').Count()).Should().Throw<System.ArgumentException>();
        }
    }
}