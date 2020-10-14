using EnTTSharp.Entities;
using RogueEntity.Core.Meta.Items;

namespace RogueEntity.Core.Sensing.Receptors.Light
{
    public class VisionSenseTrait<TGameContext, TActorId> : IReferenceItemTrait<TGameContext, TActorId>,
                                                            IItemComponentTrait<TGameContext, TActorId, VisionSense>,
                                                            IItemComponentInformationTrait<TGameContext, TActorId, SensoryReceptor<VisionSense>>
        where TActorId : IBulkDataStorageKey<TActorId>
    {
        readonly VisionSense sense;

        public VisionSenseTrait(VisionSense sense)
        {
            this.sense = sense;
        }

        public string Id => "Core.Sense.Receptor.Vision";
        public int Priority => 100;

        public void Initialize(IEntityViewControl<TActorId> v, TGameContext context, TActorId k, IItemDeclaration item)
        {
            v.AssignComponent(k, sense);
            v.AssignComponent(k, new SensoryReceptor<VisionSense>(sense));
        }

        public void Apply(IEntityViewControl<TActorId> v, TGameContext context, TActorId k, IItemDeclaration item)
        {
        }

        public bool TryQuery(IEntityViewControl<TActorId> v, TGameContext context, TActorId k, out SensoryReceptor<VisionSense> t)
        {
            return v.GetComponent(k, out t);
        }

        public bool TryQuery(IEntityViewControl<TActorId> v, TGameContext context, TActorId k, out VisionSense t)
        {
            return v.GetComponent(k, out t);
        }

        public bool TryUpdate(IEntityViewControl<TActorId> v, TGameContext context, TActorId k, in VisionSense t, out TActorId changedK)
        {
            if (v.GetComponent(k, out SensoryReceptor<VisionSense> receptor))
            {
                if (v.GetComponent(k, out VisionSense existing) && existing == t)
                {
                    changedK = k;
                    return true;
                }
                
                receptor.Configure(t);
                v.AssignOrReplace(k, in receptor);
            }
            else
            {
                receptor = new SensoryReceptor<VisionSense>(t);
                v.AssignOrReplace(k, in receptor);
            }

            changedK = k;
            return true;
        }

        public bool TryRemove(IEntityViewControl<TActorId> v, TGameContext context, TActorId k, out TActorId changedK)
        {
            changedK = k;
            return false;
        }
    }
}