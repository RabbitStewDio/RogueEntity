using FluentAssertions;
using NUnit.Framework;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.DataViews;

namespace RogueEntity.Core.Tests.Utils.Maps
{
    public class DynamicDataViewTest
    {
        [Test]
        public void ValidateBasicOperations()
        {
            var dataView = new DynamicDataView<int>(4, 5, 16, 18);
            dataView[4, 5] = 1;
            dataView[4, 5].Should().Be(1);
            
            dataView[-12, -13] = 2;
            dataView[-12, -13].Should().Be(2);
        }
        
        [Test]
        public void ValidateTileBounds()
        {
            var dataView = new DynamicDataView<int>(4, 5, 16, 18);
            dataView[4, 5] = 1;
            dataView.TryGetData(4, 5, out var tile).Should().BeTrue();
            tile.Bounds.Should().Be(new Rectangle(4, 5, 16, 18));
            dataView.TryGetData(3, 5, out tile).Should().BeFalse();
            dataView.TryGetData(4, 4, out tile).Should().BeFalse();
            
            dataView[-12, -13] = 2;
            dataView.TryGetData(-12, -13, out tile).Should().Be(true);
            tile.Bounds.Should().Be(new Rectangle(-12, -13, 16, 18));

            dataView.GetActiveTiles().Should().BeEquivalentTo(new Rectangle(-12, -13, 16, 18), new Rectangle(4, 5, 16, 18));
        }
    }
}