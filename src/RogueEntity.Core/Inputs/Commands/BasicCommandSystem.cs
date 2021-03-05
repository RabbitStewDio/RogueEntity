using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Utils;
using System;

namespace RogueEntity.Core.Inputs.Commands
{
    public class BasicCommandSystem<TActorId>
        where TActorId : IEntityKey
    {
        readonly IItemResolver<TActorId> itemResolver;
        readonly BufferList<ICommandTrait<TActorId>> traitBuffer;
        readonly RemoveCommand removeCommandHandler;

        public BasicCommandSystem(IItemResolver<TActorId> itemResolver)
        {
            this.itemResolver = itemResolver;
            this.traitBuffer = new BufferList<ICommandTrait<TActorId>>();
            this.removeCommandHandler = new RemoveCommand(itemResolver);
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


                    if (!t.TryActionOn(itemResolver, k, removeCommandHandler, out var result))
                    {
                        throw new InvalidOperationException($"This trait seem not to understand a command type {t.Id}");
                    }

                    if (!result)
                    {
                        throw new InvalidOperationException($"This trait did not remove the command artifact for {t.Id}");
                    }
                    
                }
            }
            
            v.RemoveComponent<CommandInProgress>(k);
        }

        class RemoveCommand : ICommandLift<TActorId, bool>
        {
            readonly IItemResolver<TActorId> itemResolver;

            public RemoveCommand(IItemResolver<TActorId> itemResolver)
            {
                this.itemResolver = itemResolver;
            }

            public bool PerformCommandAction<TCommand>(TActorId k, TCommand cmd)
            {
                return this.itemResolver.TryRemoveData<TCommand>(k, out _);
            }
        }
    }
}
