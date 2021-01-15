﻿using System.Collections.Generic;
using System.Linq;
using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Core.Meta.Items;

namespace RogueEntity.Core.Infrastructure.Commands
{
    public class CommandReceiverTrait< TActorId> : IReferenceItemTrait< TActorId> 
        where TActorId : IEntityKey
    {
        public CommandReceiverTrait()
        {
            Id = "Actor.Generic.CommandReceiver";
            Priority = 1;
        }

        public ItemTraitId Id { get; }
        public int Priority { get; }

        public IReferenceItemTrait< TActorId> CreateInstance()
        {
            return this;
        }

        public void Initialize(IEntityViewControl<TActorId> v, TActorId k, IItemDeclaration item)
        {
            v.AssignComponent<IdleMarker>(k);
            v.AssignComponent<CommandQueueComponent>(k);
        }

        public void Apply(IEntityViewControl<TActorId> v, TActorId k, IItemDeclaration item)
        {
        }
        
        public IEnumerable<EntityRoleInstance> GetEntityRoles()
        {
            return Enumerable.Empty<EntityRoleInstance>();
        }

        public IEnumerable<EntityRelationInstance> GetEntityRelations()
        {
            return Enumerable.Empty<EntityRelationInstance>();
        }
    }
}