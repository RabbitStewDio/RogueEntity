using System;
using EnTTSharp.Entities;
using GoRogue.MapViews;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.Continuous;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Sensing.Cache;
using RogueEntity.Core.Sensing.Resistance.Maps;
using RogueEntity.Core.Sensing.Sources;
using RogueEntity.Core.Utils.Maps;

namespace RogueEntity.Core.Sensing.Receptors.Light
{
    /// <summary>
    ///   Computes a local character's vision area. The vision area is all the space
    ///   a character can see under perfect light conditions. 
    /// </summary>
    /// <remarks>
    ///   To calculate the visibility of a objects for a given character, combine the
    ///   environment's brightness map (see sense-sources module) with this vision area.
    /// </remarks>
    public class VisionSenseSystem
    {
        readonly Lazy<ISensePropertiesSource> sensePropertiesSource;
        readonly Lazy<ISenseStateCacheProvider> senseCacheContext;

        public VisionSenseSystem(Lazy<ISensePropertiesSource> sensePropertiesSource, 
                                 Lazy<ISenseStateCacheProvider> senseCacheContext)
        {
            this.sensePropertiesSource = sensePropertiesSource ?? throw new ArgumentNullException(nameof(sensePropertiesSource));
            this.senseCacheContext = senseCacheContext ?? throw new ArgumentNullException(nameof(senseCacheContext));
        }

        public void UpdateLocalVisionGrid<TGameContext, TActorId>(IEntityViewControl<TActorId> v,
                                                                  TGameContext context,
                                                                  TActorId actor,
                                                                  in EntityGridPosition position,
                                                                  in VisionSense sense,
                                                                  in SensoryReceptor<VisionSense> sensor)
            where TActorId : IEntityKey
        {
            UpdateLocalVisionImpl(v, context, actor, in position, in sense, in sensor);
        }

        public void UpdateLocalVisionContinuous<TGameContext, TActorId>(IEntityViewControl<TActorId> v,
                                                                        TGameContext context,
                                                                        TActorId actor,
                                                                        in ContinuousMapPosition position,
                                                                        in VisionSense sense,
                                                                        in SensoryReceptor<VisionSense> sensor)
            where TActorId : IEntityKey
        {
            UpdateLocalVisionImpl(v, context, actor, in position, in sense, in sensor);
        }


        void UpdateLocalVisionImpl<TGameContext, TActorId, TPosition>(IEntityViewControl<TActorId> v,
                                                                      TGameContext context,
                                                                      TActorId actor,
                                                                      in TPosition position,
                                                                      in VisionSense sense,
                                                                      in SensoryReceptor<VisionSense> sensor)
            where TActorId : IEntityKey
            where TPosition : IPosition
        {
            if (position.IsInvalid)
            {
                sensor.DisableSense();
            }
            else
            {
                sensor.EnableSenseAt(position.GridX, position.GridY, sense);
            }

            if (!sensor.IsDirty() && 
                senseCacheContext.Value.TryGetSenseCache<VisionSense>(out var cache) && 
                !cache.IsDirty(position, (int)Math.Ceiling(sense.SenseRadius)))
            {
                return;
            }

            sensor.MarkDirty();

            if (sensePropertiesSource.Value.TryGet(position.GridZ, out var senseProperties))
            {
                var resistor = new SmartSenseResistor(senseProperties);
                sensor.SenseData.Calculate(resistor);
            }
        }

        public void MarkVisionClean<TGameContext, TActorId>(IEntityViewControl<TActorId> v,
                                                            TGameContext context,
                                                            TActorId actor,
                                                            in SensoryReceptor<VisionSense> sensor)
            where TActorId : IEntityKey
        {
            sensor.MarkClean();
        }

        readonly struct SmartSenseResistor : IMapView<float>
        {
            public int Height { get; }
            public int Width { get; }
            readonly IReadOnlyMapData<SenseProperties> senseProperties;

            public SmartSenseResistor(IReadOnlyMapData<SenseProperties> senseProperties)
            {

                this.senseProperties = senseProperties;
                Width = senseProperties.Width;
                Height = senseProperties.Height;
            }

            public float this[int x, int y]
            {
                get
                {
                    if (senseProperties == null)
                    {
                        return 0;
                    }

                    return senseProperties[x, y].blocksLight;
                }
            }
        }
    }
}