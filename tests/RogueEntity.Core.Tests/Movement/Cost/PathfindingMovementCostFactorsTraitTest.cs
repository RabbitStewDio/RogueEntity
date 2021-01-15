using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using RogueEntity.Core.Meta.EntityKeys;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Movement.Cost;
using RogueEntity.Core.Movement.MovementModes.Ethereal;
using RogueEntity.Core.Movement.MovementModes.Swimming;
using RogueEntity.Core.Movement.MovementModes.Walking;
using RogueEntity.Core.Positioning.Algorithms;
using RogueEntity.Core.Tests.Meta.Items;

namespace RogueEntity.Core.Tests.Movement.Cost
{
    [TestFixture]
    public class PathfindingMovementCostFactorsTraitTest : ItemComponentInformationTraitTestBase<ItemReference, PathfindingMovementCostFactors,
        PathfindingMovementCostFactorsTrait<ItemReference>>
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

        protected override void SetUpOverride()
        {
            EntityRegistry.RegisterNonConstructable<MovementPointCost<WalkingMovement>>();
        }

        protected override PathfindingMovementCostFactorsTrait<ItemReference> CreateTrait()
        {
            return new PathfindingMovementCostFactorsTrait<ItemReference>();
        }

        protected override BulkItemDeclaration<ItemReference> CreateBulkItemDeclaration(IBulkItemTrait<ItemReference> bulkTrait)
        {
            return base.CreateBulkItemDeclaration(bulkTrait)
                       .WithTrait(new MovementPointCostBulkItemTrait<ItemReference, WalkingMovement>(WalkingMovement.Instance, DistanceCalculation.Chebyshev, 10, 0));
        }

        protected override ReferenceItemDeclaration<ItemReference> CreateReferenceItemDeclaration(IReferenceItemTrait<ItemReference> refTrait)
        {
            return base.CreateReferenceItemDeclaration(refTrait)
                       .WithTrait(new MovementPointCostReferenceItemTrait<ItemReference, WalkingMovement>(WalkingMovement.Instance, DistanceCalculation.Chebyshev, 10, 0));
        }

        [Test]
        public void TestAccumulation()
        {
            ItemRegistry.Register(new BulkItemDeclaration<ItemReference>("Blah")
                                  .WithTrait(CreateTrait())
                                  .WithTrait(new MovementPointCostBulkItemTrait<ItemReference, WalkingMovement>(WalkingMovement.Instance,
                                                                                                                        DistanceCalculation.Euclid, 10, 10))
                                  .WithTrait(new MovementPointCostBulkItemTrait<ItemReference, SwimmingMovement>(SwimmingMovement.Instance,
                                                                                                                         DistanceCalculation.Manhattan, 10, 0))
                                  .WithTrait(new MovementPointCostBulkItemTrait<ItemReference, EtherealMovement>(EtherealMovement.Instance,
                                                                                                                         DistanceCalculation.Chebyshev, 15, -10))
            );

            var item = ItemResolver.Instantiate("Blah");
            ItemResolver.TryQueryData(item, out PathfindingMovementCostFactors factors).Should().BeTrue();
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