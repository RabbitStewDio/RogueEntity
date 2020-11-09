using EnTTSharp.Entities;
using RogueEntity.Core.Meta.ItemBuilder;
using RogueEntity.Core.Utils;

namespace RogueEntity.Core.Sensing.Resistance
{
    public static class SensoryResistanceTraitDeclaration
    {
        public static BulkItemDeclarationBuilder<TGameContext, TItemId> WithSensoryResistance<TGameContext, TItemId, TSense>(this BulkItemDeclarationBuilder<TGameContext, TItemId> builder,
                                                                                                                             SensoryResistance<TSense> resistance)
            where TItemId : IEntityKey
        {
            builder.Declaration.WithTrait(new SensoryResistanceTrait<TGameContext, TItemId, TSense>(resistance));
            return builder;
        }

        public static ReferenceItemDeclarationBuilder<TGameContext, TItemId> WithSensoryResistance<TGameContext, TItemId, TSense>(this ReferenceItemDeclarationBuilder<TGameContext, TItemId> builder,
                                                                                                                                  SensoryResistance<TSense> resistance)
            where TItemId : IEntityKey
        {
            builder.Declaration.WithTrait(new SensoryResistanceTrait<TGameContext, TItemId, TSense>(resistance));
            return builder;
        }

        public static BulkItemDeclarationBuilder<TGameContext, TItemId> WithLightResistance<TGameContext, TItemId>(this BulkItemDeclarationBuilder<TGameContext, TItemId> builder,
                                                                                                                   Percentage pct)
            where TItemId : IEntityKey
        {
            builder.Declaration.WithTrait(new SensoryResistanceTrait<TGameContext, TItemId, VisionSense>(pct));
            return builder;
        }

        public static ReferenceItemDeclarationBuilder<TGameContext, TItemId> WithLightResistance<TGameContext, TItemId>(this ReferenceItemDeclarationBuilder<TGameContext, TItemId> builder,
                                                                                                                        Percentage pct)
            where TItemId : IEntityKey
        {
            builder.Declaration.WithTrait(new SensoryResistanceTrait<TGameContext, TItemId, VisionSense>(pct));
            return builder;
        }

        public static BulkItemDeclarationBuilder<TGameContext, TItemId> WithHeatResistance<TGameContext, TItemId>(this BulkItemDeclarationBuilder<TGameContext, TItemId> builder,
                                                                                                                  Percentage pct)
            where TItemId : IEntityKey
        {
            builder.Declaration.WithTrait(new SensoryResistanceTrait<TGameContext, TItemId, TemperatureSense>(pct));
            return builder;
        }

        public static ReferenceItemDeclarationBuilder<TGameContext, TItemId> WithHeatResistance<TGameContext, TItemId>(this ReferenceItemDeclarationBuilder<TGameContext, TItemId> builder,
                                                                                                                       Percentage pct)
            where TItemId : IEntityKey
        {
            builder.Declaration.WithTrait(new SensoryResistanceTrait<TGameContext, TItemId, TemperatureSense>(pct));
            return builder;
        }

        public static BulkItemDeclarationBuilder<TGameContext, TItemId> WithSmellResistance<TGameContext, TItemId>(this BulkItemDeclarationBuilder<TGameContext, TItemId> builder,
                                                                                                                   Percentage pct)
            where TItemId : IEntityKey
        {
            builder.Declaration.WithTrait(new SensoryResistanceTrait<TGameContext, TItemId, SmellSense>(pct));
            return builder;
        }

        public static ReferenceItemDeclarationBuilder<TGameContext, TItemId> WithSmellResistance<TGameContext, TItemId>(this ReferenceItemDeclarationBuilder<TGameContext, TItemId> builder,
                                                                                                                        Percentage pct)
            where TItemId : IEntityKey
        {
            builder.Declaration.WithTrait(new SensoryResistanceTrait<TGameContext, TItemId, SmellSense>(pct));
            return builder;
        }

        public static BulkItemDeclarationBuilder<TGameContext, TItemId> WithNoiseResistance<TGameContext, TItemId>(this BulkItemDeclarationBuilder<TGameContext, TItemId> builder,
                                                                                                                   Percentage pct)
            where TItemId : IEntityKey
        {
            builder.Declaration.WithTrait(new SensoryResistanceTrait<TGameContext, TItemId, NoiseSense>(pct));
            return builder;
        }

        public static ReferenceItemDeclarationBuilder<TGameContext, TItemId> WithNoiseResistance<TGameContext, TItemId>(this ReferenceItemDeclarationBuilder<TGameContext, TItemId> builder,
                                                                                                                        Percentage pct)
            where TItemId : IEntityKey
        {
            builder.Declaration.WithTrait(new SensoryResistanceTrait<TGameContext, TItemId, NoiseSense>(pct));
            return builder;
        }
        
        public static BulkItemDeclarationBuilder<TGameContext, TItemId> WithTouchResistance<TGameContext, TItemId>(this BulkItemDeclarationBuilder<TGameContext, TItemId> builder,
                                                                                                                   Percentage pct)
            where TItemId : IEntityKey
        {
            builder.Declaration.WithTrait(new SensoryResistanceTrait<TGameContext, TItemId, TouchSense>(pct));
            return builder;
        }

        public static ReferenceItemDeclarationBuilder<TGameContext, TItemId> WithTouchResistance<TGameContext, TItemId>(this ReferenceItemDeclarationBuilder<TGameContext, TItemId> builder,
                                                                                                                        Percentage pct)
            where TItemId : IEntityKey
        {
            builder.Declaration.WithTrait(new SensoryResistanceTrait<TGameContext, TItemId, TouchSense>(pct));
            return builder;
        }
    }
}