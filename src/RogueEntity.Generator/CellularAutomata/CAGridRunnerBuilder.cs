using System;
using RogueEntity.Core.Infrastructure.Randomness;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.MapLayers;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.DataViews;

namespace RogueEntity.Generator.CellularAutomata
{
    public readonly struct CAGridRunnerBuilder
    {
        readonly IRandomGenerator rng;
        readonly Rectangle bounds;
        readonly string ruleString;

        public CAGridRunnerBuilder(IRandomGenerator rng, Rectangle bounds, string ruleString)
        {
            this.rng = rng;
            this.bounds = bounds;
            this.ruleString = ruleString;
        }

        public IReadOnlyDynamicDataView2D<TEntity> Generate<TEntity>(TEntity spaceEntity, TEntity wallEntity, int iterations = 100)
        {
            var runner = new CAGridRunner<TEntity>(ruleString, bounds, spaceEntity, wallEntity)
                .Reset()
                .PopulateAtCenter(rng, 3, 3)
                .Step(iterations);
            if (runner.TryGetDataView(out var result))
            {
                return result;
            }
            
            throw new Exception();
        }

        public CAGridRunner<TEntity> Start<TEntity>(TEntity spaceEntity, TEntity wallEntity)
        {
            var runner = new CAGridRunner<TEntity>(ruleString, bounds, spaceEntity, wallEntity);
            runner.Reset();
            return runner;
        }
    }

    public class CAGridRunner<TEntity>
    {
        readonly CellGridTransformSystem<TEntity> sys;
        readonly Rectangle bounds;
        readonly TEntity aliveEntity;
        readonly TEntity deadEntity;

        public CAGridRunner(string ruleString,
                            Rectangle bounds,
                            TEntity aliveEntity,
                            TEntity deadEntity)
        {
            this.bounds = bounds;
            this.aliveEntity = aliveEntity;
            this.deadEntity = deadEntity;
            var ruleSet = CARuleParser.ParseRuleString(ruleString);
            this.sys = new CellGridTransformSystem<TEntity>(new DynamicDataViewConfiguration(0, 0, 64, 64), ruleSet, aliveEntity, deadEntity);
        }

        public CAGridRunner<TEntity> Reset()
        {
            if (!sys.DataView.TryGetWritableView(0, out var baseView, DataViewCreateMode.CreateMissing))
            {
                throw new ArgumentException("Unable to create a writable data view");
            }

            foreach (var (x, y) in bounds.Contents)
            {
                baseView[x, y] = deadEntity; // rng.NextDouble() >= 0.5 ? wallEntity : spaceEntity;
            }

            return this;
        }

        public CAGridRunner<TEntity> PopulateAtCenter(IRandomGenerator rng, int extendX, int extendY)
        {
            Populate(rng, Rectangle.WithRadius(bounds.Center.X, bounds.Center.Y, extendX, extendY));
            return this;
        }
        
        public CAGridRunner<TEntity> Populate(IRandomGenerator rng, Rectangle area)
        {
            if (!sys.DataView.TryGetWritableView(0, out var baseView, DataViewCreateMode.CreateMissing))
            {
                throw new ArgumentException("Unable to create a writable data view");
            }
            
            foreach (var (x, y) in area.Contents)
            {
                baseView[x, y] = rng.Next() >= 0.5 ? deadEntity : aliveEntity;
                sys.MarkDirty(Position.Of(MapLayer.Indeterminate, x, y));
            }
            
            baseView[area.Center.X, area.Center.Y] = aliveEntity;
            return this;
        }

        public CAGridRunner<TEntity> Step(int stepCount)
        {
            for (int i = 0; i < stepCount; i += 1)
            {
                sys.ProcessAndSwap(bounds);
            }

            return this;
        }

        public bool TryGetDataView(out IReadOnlyDynamicDataView2D<TEntity> result) => sys.DataView.TryGetView(0, out result);
        public bool TryGetWritableView(out IDynamicDataView2D<TEntity> result, DataViewCreateMode mode = DataViewCreateMode.Nothing) 
            => sys.DataView.TryGetWritableView(0, out result, mode);
    }
}