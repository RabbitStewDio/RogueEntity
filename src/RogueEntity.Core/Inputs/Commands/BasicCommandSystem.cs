using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Utils;
using Serilog;
using System;

namespace RogueEntity.Core.Inputs.Commands
{
    public class BasicCommandSystem<TActorId>
        where TActorId : struct, IEntityKey
    {
        readonly ILogger logger = SLog.ForContext<BasicCommandSystem<TActorId>>();
        
        readonly IItemResolver<TActorId> itemResolver;
        readonly BufferList<ICommandTrait<TActorId>> traitBuffer;

        public BasicCommandSystem(IItemResolver<TActorId> itemResolver)
        {
            this.itemResolver = itemResolver;
            this.traitBuffer = new BufferList<ICommandTrait<TActorId>>();
        }

        public void ClearHandledCommands(IEntityViewControl<TActorId> v,
                                         TActorId k, 
                                         in CommandInProgress p)
        {
            if (!p.Handled)
            {
                return;
            }

            var cmdId = p.ActiveCommand;
            if (itemResolver.TryResolve(k, out var itemDeclaration))
            {
                itemDeclaration.QueryAll(traitBuffer);
                foreach (var t in traitBuffer)
                {
                    if (t.CommandId != cmdId)
                    {
                        continue;
                    }

                    logger.Debug("Cleaning stale command data for {Command}", t.CommandId);
                    if (!t.TryRemoveCompletedCommandData(itemResolver, k))
                    {
                        throw new InvalidOperationException($"This trait seem not to understand a command type {t.Id}");
                    }
                }
            }
            
            v.RemoveComponent<CommandInProgress>(k);
        }
    }
}
