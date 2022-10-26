using System;
using FluentAssertions;
using NUnit.Framework;
using RogueEntity.Core.Infrastructure.Randomness;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.DataViews;
using RogueEntity.Generator.CellularAutomata;

namespace RogueEntity.Generator.Tests
{
    public class MazeGeneratorTest
    {
        [Test]
        public void TestMaze()
        {
            var bounds = new Rectangle(0, 0, 32, 32);
            var mg = new CAGridRunnerBuilder(new FixedRandomGeneratorSource().FromConstantSeed(10), bounds, CARuleStrings.MazeGeneration);
            var maze = mg.Generate('.', '#', 128);
            Console.WriteLine(maze.ExtendToString(bounds, elementSeparator: "", elementStringifier: e => $"{e}{e}"));
        }

        [Test]
        public void TestConway()
        {
            var bounds = new Rectangle(0, 0, 8, 8);
            var mg = new CAGridRunnerBuilder(new FixedRandomGeneratorSource().FromConstantSeed(10), bounds, CARuleStrings.Conway);
            var maze = mg.Start('.', '#');
            maze.TryGetWritableView(out var view).Should().BeTrue();
            // Glider: 
            // 
            //   ##.
            //   .#.
            //   #..
            //
            view.TrySet(2, 3, '.');
            view.TrySet(3, 2, '.');
            view.TrySet(4, 2, '.');
            view.TrySet(4, 3, '.');
            view.TrySet(4, 4, '.');

            for (var i = 0; i < 8; i += 1)
            {
                maze.Step(1);
                maze.TryGetDataView(out var rv).Should().BeTrue();
                Console.WriteLine($"Step: {(i + 1)}");
                Console.WriteLine(rv.ExtendToString(bounds, elementSeparator: "", elementStringifier: e => $"{e}"));
            }

        }

        [Test]
        public void TestConway2()
        {
            var bounds = new Rectangle(0, 0, 8, 8);
            var mg = new CAGridRunnerBuilder(new FixedRandomGeneratorSource().FromConstantSeed(10), bounds, CARuleStrings.Conway);
            var maze = mg.Start('.', '#');
            maze.TryGetWritableView(out var view).Should().BeTrue();
            // Glider: 
            // 
            //   ##.
            //   .#.
            //   #..
            //
            view.TrySet(2, 2, '.');
            view.TrySet(1, 2, '.');
            view.TrySet(1, 1, '.');

            for (var i = 0; i < 4; i += 1)
            {
                maze.Step(1);
                maze.TryGetDataView(out var rv).Should().BeTrue();
                Console.WriteLine($"Step: {(i + 1)}");
                Console.WriteLine(rv.ExtendToString(bounds, elementSeparator: "", elementStringifier: e => $"{e}"));
            }

        }
    }
}