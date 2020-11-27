using System;
using System.IO;
using System.Reflection;
using FluentAssertions;
using NUnit.Framework;
using RogueEntity.Core.Tests;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.DataViews;

namespace RogueEntity.Performance.Tests
{
    public class MazeGeneratorTest
    {
        [Test]
        public void TestMaze()
        {
            var bounds = new Rectangle(0, 0, 256, 256);
            var mg = new MazeGenerator().WithBounds(bounds).WithSeed(10);
            var maze = mg.Generate('.', '#', 512);
            Console.WriteLine(maze.ExtendToString(bounds, elementSeparator: "", elementStringifier: e => $"{e}{e}"));
        }

        [Test]
        public void TestEmbeddedResource()
        {
            var assembly = typeof(MazeGeneratorTest).GetTypeInfo().Assembly;
            using Stream resource = assembly.GetManifestResourceStream("RogueEntity.Performance.Tests.Maze256.txt");
            resource.Should().NotBeNull();
        }
    }
}