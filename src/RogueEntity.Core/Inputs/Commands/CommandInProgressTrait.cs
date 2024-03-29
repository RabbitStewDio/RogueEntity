﻿using EnTTSharp;
using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Core.Meta.Items;
using System.Collections.Generic;

namespace RogueEntity.Core.Inputs.Commands
{
    public class CommandInProgressTrait<TActorId> : SimpleReferenceItemComponentTraitBase<TActorId, CommandInProgress>
        where TActorId : struct, IEntityKey
    {
        public static readonly CommandInProgressTrait<TActorId> Instance = new CommandInProgressTrait<TActorId>();
        
        public CommandInProgressTrait() : base("Core.Inputs.Commands.CommandInProgress", 100)
        {
        }

        protected override Optional<CommandInProgress> CreateInitialValue(TActorId reference)
        {
            return default;
        }

        public override IEnumerable<EntityRoleInstance> GetEntityRoles()
        {
            yield return CommandsModule.CommandExecutionTrackerRole.Instantiate<TActorId>();
        }
    }
}
