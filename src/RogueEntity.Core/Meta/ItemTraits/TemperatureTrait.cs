﻿using System.Collections.Generic;
using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Core.Meta.Items;

namespace RogueEntity.Core.Meta.ItemTraits
{
    public class TemperatureTrait<TItemId>: StatelessItemComponentTraitBase<TItemId, Temperature>
        where TItemId : struct, IEntityKey
    {
        readonly Temperature temperature;

        public TemperatureTrait(Temperature temperature) : base("Core.Item.Temperature", 100)
        {
            this.temperature = temperature;
        }

        protected override Temperature GetData(TItemId k)
        {
            return temperature;
        }

        public override IEnumerable<EntityRoleInstance> GetEntityRoles()
        {
            yield return CoreModule.ItemRole.Instantiate<TItemId>();
        }
    }
}