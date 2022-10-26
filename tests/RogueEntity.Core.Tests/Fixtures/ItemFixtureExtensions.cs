using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.Grid;

namespace RogueEntity.Core.Tests.Fixtures
{
    public interface ITestFixture<TSelf, TEntity> : IEntityFixture<TEntity>
        where TSelf : IEntityFixture<TEntity>, ITestFixture<TSelf, TEntity>
        where TEntity : struct, IBulkDataStorageKey<TEntity>
    {
    }

    public abstract class ItemTestFixtureBase<TSelf, TEntity> : WhenFixtureSupport, IEntityFixture<TEntity>
        where TSelf : ItemTestFixtureBase<TSelf, TEntity>
        where TEntity : struct, IBulkDataStorageKey<TEntity>
    {
        public abstract IItemResolver<TEntity> ItemResolver { get; }
        public abstract IGridMapContext<TEntity> ItemMapContext { get; }

        public EntityContext<TSelf, TEntity> GivenAnEntity(ItemDeclarationId item)
            => new EntityContext<TSelf, TEntity>((TSelf)this, item);

        public EntityContext<TSelf, TEntity> GivenAnEmptySpace()
            => new EntityContext<TSelf, TEntity>((TSelf)this);

        public EntityContext<TSelf, TEntity> GivenAnExistingEntity(TEntity item)
            => new EntityContext<TSelf, TEntity>((TSelf)this, item);

        public PlacementAssertions<TEntity, TSelf> ThenPosition(Position position)
            => new PlacementAssertions<TEntity, TSelf>((TSelf)this, position);

        public ItemAssertions<TSelf, TEntity> ThenItem(in TEntity item)
            => new ItemAssertions<TSelf, TEntity>((TSelf)this, item);
    }
}