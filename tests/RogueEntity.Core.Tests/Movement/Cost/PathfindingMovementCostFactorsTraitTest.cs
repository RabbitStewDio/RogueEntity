using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using RogueEntity.Core.Meta.EntityKeys;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Movement.Cost;
using RogueEntity.Core.Movement.MovementModes.Ethereal;
using RogueEntity.Core.Movement.MovementModes.Swimming;
using RogueEntity.Core.Movement.MovementModes.Walking;
using RogueEntity.Core.Tests.Meta.Items;
using RogueEntity.Core.Utils.Algorithms;

namespace RogueEntity.Core.Tests.Movement.Cost
{
    [TestFixture]
    public class PathfindingMovementCostFactorsTraitTest : ItemComponentInformationTraitTestBase<object, ItemReference, PathfindingMovementCostFactors,
        PathfindingMovementCostFactorsTrait<object, ItemReference>>
    {
        public PathfindingMovementCostFactorsTraitTest() : base(new ItemReferenceMetaData())
        {
        }

        protected override IItemComponentTestDataFactory<PathfindingMovementCostFactors> ProduceTestData(EntityRelations<ItemReference> relations)
        {
            return new ItemComponentTestDataFactory<PathfindingMovementCostFactors>(new PathfindingMovementCostFactors(new List<MovementCost>()
            {
                new MovementCost(WalkingMovement.Instance, DistanceCalculation.Chebyshev, 10)
            }));
        }

        protected override object CreateContext()
        {
            return new object();
        }

        protected override void SetUpOverride()
        {
            EntityRegistry.RegisterNonConstructable<MovementPointCost<WalkingMovement>>();
        }

        protected override PathfindingMovementCostFactorsTrait<object, ItemReference> CreateTrait()
        {
            return new PathfindingMovementCostFactorsTrait<object, ItemReference>();
        }

        protected override BulkItemDeclaration<object, ItemReference> CreateBulkItemDeclaration(IBulkItemTrait<object, ItemReference> bulkTrait)
        {
            return base.CreateBulkItemDeclaration(bulkTrait)
                       .WithTrait(new MovementPointCostBulkItemTrait<object, ItemReference, WalkingMovement>(WalkingMovement.Instance, DistanceCalculation.Chebyshev, 10, 0));
        }

        protected override ReferenceItemDeclaration<object, ItemReference> CreateReferenceItemDeclaration(IReferenceItemTrait<object, ItemReference> refTrait)
        {
            return base.CreateReferenceItemDeclaration(refTrait)
                       .WithTrait(new MovementPointCostReferenceItemTrait<object, ItemReference, WalkingMovement>(WalkingMovement.Instance, DistanceCalculation.Chebyshev, 10, 0));
        }

        [Test]
        public void TestAccumulation()
        {
            ItemRegistry.Register(new BulkItemDeclaration<object, ItemReference>("Blah")
                                  .WithTrait(CreateTrait())
                                  .WithTrait(new MovementPointCostBulkItemTrait<object, ItemReference, WalkingMovement>(WalkingMovement.Instance,
                                                                                                                        DistanceCalculation.Euclid, 10, 10))
                                  .WithTrait(new MovementPointCostBulkItemTrait<object, ItemReference, SwimmingMovement>(SwimmingMovement.Instance,
                                                                                                                         DistanceCalculation.Manhattan, 10, 0))
                                  .WithTrait(new MovementPointCostBulkItemTrait<object, ItemReference, EtherealMovement>(EtherealMovement.Instance,
                                                                                                                         DistanceCalculation.Chebyshev, 15, -10))
            );

            var item = ItemResolver.Instantiate(Context, "Blah");
            ItemResolver.TryQueryData(item, Context, out PathfindingMovementCostFactors factors).Should().BeTrue();
            factors.Should()
                   .Be(new PathfindingMovementCostFactors(new List<MovementCost>()
                   {
                       new MovementCost(SwimmingMovement.Instance, DistanceCalculation.Manhattan, 10),
                       new MovementCost(WalkingMovement.Instance, DistanceCalculation.Euclid, 10, 10),
                       new MovementCost(EtherealMovement.Instance, DistanceCalculation.Chebyshev, 15, -10)
                   }));
        }
    }
}