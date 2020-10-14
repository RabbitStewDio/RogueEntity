using EnTTSharp.Entities;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Sensing.Receptors.Light;

namespace RogueEntity.Core.Sensing.Receptors.Temperature
{
    public class TemperatureSenseTrait<TGameContext, TActorId> : IReferenceItemTrait<TGameContext, TActorId>,
                                                                 IItemComponentTrait<TGameContext, TActorId, TemperatureSense>,
                                                                 IItemComponentInformationTrait<TGameContext, TActorId, SensoryReceptor<TemperatureSense>>
        where TActorId : IBulkDataStorageKey<TActorId>
    {
        readonly TemperatureSense sense;

        public TemperatureSenseTrait(TemperatureSense sense)
        {
            this.sense = sense;
        }

        public string Id => "Core.Sense.Receptor.Temperature";
        public int Priority => 100;

        public void Initialize(IEntityViewControl<TActorId> v, TGameContext context, TActorId k, IItemDeclaration item)
        {
            v.AssignComponent(k, sense);
            v.AssignComponent(k, new SensoryReceptor<TemperatureSense>(sense));
        }

        public void Apply(IEntityViewControl<TActorId> v, TGameContext context, TActorId k, IItemDeclaration item)
        {
        }

        public bool TryQuery(IEntityViewControl<TActorId> v, TGameContext context, TActorId k, out SensoryReceptor<TemperatureSense> t)
        {
            return v.GetComponent(k, out t);
        }

        public bool TryQuery(IEntityViewControl<TActorId> v, TGameContext context, TActorId k, out TemperatureSense t)
        {
            return v.GetComponent(k, out t);
        }

        public bool TryUpdate(IEntityViewControl<TActorId> v, TGameContext context, TActorId k, in TemperatureSense t, out TActorId changedK)
        {
            if (v.GetComponent(k, out SensoryReceptor<TemperatureSense> receptor))
            {
                if (v.GetComponent(k, out TemperatureSense existing) && existing == t)
                {
                    changedK = k;
                    return true;
                }

                receptor.Configure(t);
                v.AssignOrReplace(k, in receptor);
            }
            else
            {
                receptor = new SensoryReceptor<TemperatureSense>(t);
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