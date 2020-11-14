using RogueEntity.Core.Infrastructure.Modules.Attributes;
using RogueEntity.Core.Infrastructure.Services;
using RogueEntity.Core.Infrastructure.Time;
using RogueEntity.Core.Sensing.Cache;
using RogueEntity.Core.Sensing.Common;
using RogueEntity.Core.Sensing.Common.Physics;
using RogueEntity.Core.Sensing.Resistance;
using RogueEntity.Core.Sensing.Resistance.Directions;
using RogueEntity.Core.Utils.DataViews;

namespace RogueEntity.Core.Sensing.Sources.Heat
{
    [Module]
    public class HeatSourceModule : SenseSourceModuleBase<TemperatureSense, HeatSourceDefinition>
    {
        public static readonly string ModuleId = "Core.Senses.Source.Temperature";

        public HeatSourceModule()
        {
            Id = ModuleId;

            Author = "RogueEntity.Core";
            Name = "RogueEntity Core Module - Sense Source - Temperature";
            Description = "Provides sense sources and sense resistance for heat and cold";
            IsFrameworkModule = true;
        }

        protected override SenseSourceSystem<TemperatureSense, HeatSourceDefinition> GetOrCreateSenseSourceSystem<TGameContext, TItemId>(IServiceResolver serviceResolver)
        {
            if (!serviceResolver.TryResolve(out SenseSourceSystem<TemperatureSense, HeatSourceDefinition> ls))
            {
                var physics = serviceResolver.Resolve<IHeatPhysicsConfiguration>();
                ls = new HeatSourceSystem(serviceResolver.ResolveToReference<IReadOnlyDynamicDataView3D<SensoryResistance<TemperatureSense>>>(),
                                    serviceResolver.ResolveToReference<IGlobalSenseStateCacheProvider>(),
                                    serviceResolver.ResolveToReference<ITimeSource>(),
                                    serviceResolver.Resolve<ISensoryResistanceDirectionView<TemperatureSense>>(),
                                    serviceResolver.Resolve<ISenseStateCacheControl>(),
                                    physics.CreateHeatPropagationAlgorithm(),
                                    physics);
                serviceResolver.Store(ls);
            }

            return ls;
        }

        protected override (ISensePropagationAlgorithm, ISensePhysics) GetOrCreateSensePhysics(IServiceResolver resolver)
        {
            var physics = resolver.Resolve<IHeatPhysicsConfiguration>();
            return (physics.CreateHeatPropagationAlgorithm(), physics.HeatPhysics);
        }

    }
}