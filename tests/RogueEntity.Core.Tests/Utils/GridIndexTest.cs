using FluentAssertions;
using NUnit.Framework;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.DataViews;
using RogueEntity.Core.Utils.SpatialIndex;
using System;

namespace RogueEntity.Core.Tests.Utils
{
    public class GridIndexTest
    {
        [Test]
        public void ValidateBasicOperation()
        {
            var qt = new GridIndex2D<string>(new DynamicDataViewConfiguration(0, 0, 128, 128));
            var keyA = qt.Insert("elementA", new Rectangle(0, 0, 1, 1));
            var keyB = qt.Insert("elementB", new Rectangle(120, 0, 1, 1));
            var keyC = qt.Insert("elementC", new Rectangle(120, 1, 1, 1));
            var keyD = qt.Insert("elementD", new Rectangle(110, 1, 10, 10));

            qt[keyA].Data.Should().Be("elementA");
            qt[keyB].Data.Should().Be("elementB");
            qt[keyC].Data.Should().Be("elementC");
            qt[keyD].Data.Should().Be("elementD");
        }

        [Test]
        public void ValidateBasicOperation2()
        {
            var qt = new GridIndex2D<string>(new DynamicDataViewConfiguration(0, 0, 128, 128));
            var keyA = qt.Insert("elementA", new Rectangle(0, 0, 1, 1));
            var keyB = qt.Insert("elementB", new Rectangle(120, 0, 1, 1));
            var keyC = qt.Insert("elementC", new Rectangle(120, 1, 1, 1));
            var keyD = qt.Insert("elementD", new Rectangle(110, 1, 10, 10));

            qt[keyA].Data.Should().Be("elementA");
            qt[keyB].Data.Should().Be("elementB");
            qt[keyC].Data.Should().Be("elementC");
            qt[keyD].Data.Should().Be("elementD");
            
            Console.WriteLine(qt.Print());
        }

        [Test]
        public void ValidateBasicQuery()
        {
            var qt = new GridIndex2D<string>(new DynamicDataViewConfiguration(0, 0, 128, 128));
            var keyA = qt.Insert("elementA", new Rectangle(0, 0, 1, 1));
            var keyB = qt.Insert("elementB", new Rectangle(120, 0, 1, 1));
            var keyC = qt.Insert("elementC", new Rectangle(120, 1, 1, 1));
            var keyD = qt.Insert("elementD", new Rectangle(110, 1, 10, 10));

            Console.WriteLine(qt.Print());

            var result = qt.Query(new Rectangle(0, 0, 128, 128));
            result.Should().BeEquivalentTo(new[] {keyA, keyB, keyC, keyD});

            result = qt.Query(new Rectangle(0, 0, 8, 8));
            result.Should().BeEquivalentTo(new[] {keyA});
        }

        [Test]
        public void ValidatePathologicalInsert()
        {
            var qt = new GridIndex2D<string>(new DynamicDataViewConfiguration(0, 0, 128, 128));
            var arr = new int[100];
            for (var x = 0; x < arr.Length; x += 1)
            {
                arr[x] = qt.Insert("element-" + x, new Rectangle(0, 0, 1, 1));
            }
            
            var result = qt.Query(new Rectangle(0, 0, 8, 8));
            result.Should().BeEquivalentTo(arr);

            Console.WriteLine(qt.Print());
        }


        [Test]
        public void ValidateQuadNodeLeaf()
        {
            var n = QuadNode.Leaf();
            n.IsLeaf.Should().BeTrue();
        }
    }
}
