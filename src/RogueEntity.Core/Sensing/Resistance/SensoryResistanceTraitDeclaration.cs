using EnTTSharp.Entities;
using RogueEntity.Core.Meta.ItemBuilder;
using RogueEntity.Core.Utils;

namespace RogueEntity.Core.Sensing.Resistance
{
    public static class SensoryResistanceTraitDeclaration
    {
        public static BulkItemDeclarationBuilder< TItemId> WithSensoryResistance< TItemId, TSense>(this BulkItemDeclarationBuilder< TItemId> builder,
                                                                                                                             SensoryResistance<TSense> resistance)
            where TItemId : struct, IEntityKey
        {
            builder.Declaration.WithTrait(new SensoryResistanceTrait< TItemId, TSense>(resistance));
            return builder;
        }

        public static ReferenceItemDeclarationBuilder< TItemId> WithSensoryResistance< TItemId, TSense>(this ReferenceItemDeclarationBuilder< TItemId> builder,
                                                                                                                                  SensoryResistance<TSense> resistance)
            where TItemId : struct, IEntityKey
        {
            builder.Declaration.WithTrait(new SensoryResistanceTrait< TItemId, TSense>(resistance));
            return builder;
        }

        public static BulkItemDeclarationBuilder< TItemId> WithLightResistance< TItemId>(this BulkItemDeclarationBuilder< TItemId> builder,
                                                                                                                   Percentage pct)
            where TItemId : struct, IEntityKey
        {
            builder.Declaration.WithTrait(new SensoryResistanceTrait< TItemId, VisionSense>(pct));
            return builder;
        }

        public static ReferenceItemDeclarationBuilder< TItemId> WithLightResistance< TItemId>(this ReferenceItemDeclarationBuilder< TItemId> builder,
                                                                                                                        Percentage pct)
            where TItemId : struct, IEntityKey
        {
            builder.Declaration.WithTrait(new SensoryResistanceTrait< TItemId, VisionSense>(pct));
            return builder;
        }

        public static BulkItemDeclarationBuilder< TItemId> WithHeatResistance< TItemId>(this BulkItemDeclarationBuilder< TItemId> builder,
                                                                                                                  Percentage pct)
            where TItemId : struct, IEntityKey
        {
            builder.Declaration.WithTrait(new SensoryResistanceTrait< TItemId, TemperatureSense>(pct));
            return builder;
        }

        public static ReferenceItemDeclarationBuilder< TItemId> WithHeatResistance< TItemId>(this ReferenceItemDeclarationBuilder< TItemId> builder,
                                                                                                                       Percentage pct)
            where TItemId : struct, IEntityKey
        {
            builder.Declaration.WithTrait(new SensoryResistanceTrait< TItemId, TemperatureSense>(pct));
            return builder;
        }

        public static BulkItemDeclarationBuilder< TItemId> WithSmellResistance< TItemId>(this BulkItemDeclarationBuilder< TItemId> builder,
                                                                                                                   Percentage pct)
            where TItemId : struct, IEntityKey
        {
            builder.Declaration.WithTrait(new SensoryResistanceTrait< TItemId, SmellSense>(pct));
            return builder;
        }

        public static ReferenceItemDeclarationBuilder< TItemId> WithSmellResistance< TItemId>(this ReferenceItemDeclarationBuilder< TItemId> builder,
                                                                                                                        Percentage pct)
            where TItemId : struct, IEntityKey
        {
            builder.Declaration.WithTrait(new SensoryResistanceTrait< TItemId, SmellSense>(pct));
            return builder;
        }

        public static BulkItemDeclarationBuilder< TItemId> WithNoiseResistance< TItemId>(this BulkItemDeclarationBuilder< TItemId> builder,
                                                                                                                   Percentage pct)
            where TItemId : struct, IEntityKey
        {
            builder.Declaration.WithTrait(new SensoryResistanceTrait< TItemId, NoiseSense>(pct));
            return builder;
        }

        public static ReferenceItemDeclarationBuilder< TItemId> WithNoiseResistance< TItemId>(this ReferenceItemDeclarationBuilder< TItemId> builder,
                                                                                                                        Percentage pct)
            where TItemId : struct, IEntityKey
        {
            builder.Declaration.WithTrait(new SensoryResistanceTrait< TItemId, NoiseSense>(pct));
            return builder;
        }
        
        public static BulkItemDeclarationBuilder< TItemId> WithTouchResistance< TItemId>(this BulkItemDeclarationBuilder< TItemId> builder,
                                                                                                                   Percentage pct)
            where TItemId : struct, IEntityKey
        {
            builder.Declaration.WithTrait(new SensoryResistanceTrait< TItemId, TouchSense>(pct));
            return builder;
        }

        public static ReferenceItemDeclarationBuilder< TItemId> WithTouchResistance< TItemId>(this ReferenceItemDeclarationBuilder< TItemId> builder,
                                                                                                                        Percentage pct)
            where TItemId : struct, IEntityKey
        {
            builder.Declaration.WithTrait(new SensoryResistanceTrait< TItemId, TouchSense>(pct));
            return builder;
        }
    }
}