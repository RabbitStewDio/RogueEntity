using FluentAssertions;
using NUnit.Framework;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.DataViews;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RogueEntity.Core.Tests.Utils.Maps
{
    public class PooledDataViewTest
    {
        class TestPool : IBoundedDataViewPool<byte>
        {
            readonly HashSet<DefaultPooledBoundedDataView<byte>> available;
            readonly HashSet<DefaultPooledBoundedDataView<byte>> leased;
            
            public TestPool(DynamicDataViewConfiguration tileConfiguration)
            {
                TileConfiguration = tileConfiguration;
                available = new HashSet<DefaultPooledBoundedDataView<byte>>();
                leased = new HashSet<DefaultPooledBoundedDataView<byte>>();
            }

            public DynamicDataViewConfiguration TileConfiguration { get; }
            
            public IPooledBoundedDataView<byte> Lease(Rectangle bounds, long time)
            {
                var entry = available.FirstOrDefault();
                if (entry != null)
                {
                    
                    entry.Resize(bounds, true);
                    entry.BeginUseTimePeriod(time);
                    // have a leased object available
                    available.Remove(entry);
                    leased.Add(entry);
                    return entry;
                }
                
                entry = new DefaultPooledBoundedDataView<byte>(bounds, time);
                leased.Add(entry);
                return entry;
            }

            public void Return(IPooledBoundedDataView<byte> returnedValue)
            {
                if (!this.leased.Contains(returnedValue))
                    throw new ArgumentException();

                if (returnedValue is not DefaultPooledBoundedDataView<byte> rv)
                {
                    throw new ArgumentException();
                }
                
                leased.Remove(rv);
                available.Add(rv);
            }

            public bool IsLeased(IBoundedDataView<byte> b)
            {
                return leased.Contains(b);
            }
            
            public bool IsAvailable(IBoundedDataView<byte> b)
            {
                return available.Contains(b);
            }
        }

        [Test]
        public void ValidateReserveViewOnDirectWrite()
        {
            var pool = new TestPool(DynamicDataViewConfiguration.Default16X16);
            var p = new PooledDynamicDataView3D<byte>(pool);
            
            // create a map layer for all z-positions of 0
            p.TryGetWritableView(0, out var view, DataViewCreateMode.CreateMissing).Should().BeTrue();
            
            view.TryGetWriteAccess(0, 0, out _).Should().BeFalse("because we never wrote to that page");
            view.TrySet(0, 0, 1);
            view.TryGetWriteAccess(0, 0, out var rawView).Should().BeTrue("because we just wrote to that page");
            view.TryGetWriteAccess(100, 100, out _).Should().BeFalse("because we never wrote to that page");
            pool.IsLeased(rawView).Should().BeTrue();
        }

        [Test]
        public void ValidateReserveViewOnDemand()
        {
            var pool = new TestPool(DynamicDataViewConfiguration.Default16X16);
            var p = new PooledDynamicDataView3D<byte>(pool);
            
            // create a map layer for all z-positions of 0
            p.TryGetWritableView(0, out var view, DataViewCreateMode.CreateMissing).Should().BeTrue();
            view.TryGetWriteAccess(0, 0, out var rawView, DataViewCreateMode.CreateMissing).Should().BeTrue("because creating a page on demand should not fail in this implementation");
            pool.IsLeased(rawView).Should().BeTrue();
        }
        
        [Test]
        public void ValidateExpiration()
        {
            var pool = new TestPool(DynamicDataViewConfiguration.Default16X16);
            var p = new PooledDynamicDataView3D<byte>(pool);
            
            // create a map layer for all z-positions of 0
            p.TryGetWritableView(0, out var view, DataViewCreateMode.CreateMissing).Should().BeTrue();
            view.TryGetWriteAccess(0, 0, out var rawView, DataViewCreateMode.CreateMissing).Should().BeTrue("because creating a page on demand should not fail in this implementation");
            pool.IsLeased(rawView).Should().BeTrue();
            
            p.PrepareFrame(1);
            p.ExpireFrames(age: 1);
            
            pool.IsLeased(rawView).Should().BeFalse();
            pool.IsAvailable(rawView).Should().BeTrue();
        }
    }
}
