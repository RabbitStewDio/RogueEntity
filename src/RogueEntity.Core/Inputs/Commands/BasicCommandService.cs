using EnTTSharp.Entities;
using JetBrains.Annotations;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Utils;
using Serilog;
using System;

namespace RogueEntity.Core.Inputs.Commands
{
    public class BasicCommandService<TActor>
        where TActor : IEntityKey
    {
        static readonly ILogger Logger = SLog.ForContext<BasicCommandService<TActor>>();
        readonly IItemResolver<TActor> itemResolver;

        public BasicCommandService([NotNull] IItemResolver<TActor> itemResolver)
        {
            this.itemResolver = itemResolver ?? throw new ArgumentNullException(nameof(itemResolver));
        }
        
        public bool IsActive(TActor actor)
        {
            return !itemResolver.IsDestroyed(actor);
        }

        public bool IsValid<TCommand>(TActor actor)
        {
            if (itemResolver.TryQueryTrait(actor, out ICommandTrait<TActor, TCommand> trait))
            {
                return trait.IsCommandValidForState(actor);
            }

            return false;
        }

        public bool IsValid<TCommand>(TActor actor, TCommand cmd)
        {
            if (itemResolver.TryQueryData(actor, out CommandInProgress _))
            {
                return false;
            }
            
            if (itemResolver.TryQueryTrait(actor, out ICommandTrait<TActor, TCommand> trait))
            {
                return trait.IsCommandValidForState(actor, cmd);
            }

            return false;
        }

        public bool TrySubmit<TCommand>(TActor actor, TCommand cmd)
        {
            if (!IsValid(actor, cmd))
            {
                return false;
            }

            if (!itemResolver.TryQueryTrait(actor, out ICommandTrait<TActor, TCommand> trait))
            {
                return false;
            }
            
            if (!itemResolver.TryUpdateData(actor, cmd, out _))
            {
                return false;
            }

            if (!itemResolver.TryUpdateData(actor, new CommandInProgress(false, CommandTypeId.Create<TCommand>()), out _))
            {
                itemResolver.TryRemoveData<TCommand>(actor, out _);
                return false;
            }

            Logger.Information("Submitted command {Command}", cmd);            
            return true;
        }
    }
}
