using EnTTSharp.Entities;
using RogueEntity.Core.Meta.ItemBuilder;

namespace RogueEntity.Core.Inputs.Commands
{
    public static class CommandTraitDeclarations
    {
        public static ReferenceItemDeclarationBuilder<TActorId> WithCommand<TActorId, TCommand>(this ReferenceItemDeclarationBuilder<TActorId> builder)
            where TActorId : IEntityKey
        {
            builder.WithTrait(CommandInProgressTrait<TActorId>.Instance);
            return builder.WithTrait(new BasicCommandTrait<TActorId, TCommand>());
        }
        
        public static ReferenceItemDeclarationBuilder<TActorId> WithCommand<TActorId, TCommand>(this ReferenceItemDeclarationBuilder<TActorId> builder,
                                                                                                CommandType<TCommand> cmd)
            where TActorId : IEntityKey
        {
            builder.WithTrait(CommandInProgressTrait<TActorId>.Instance);
            return builder.WithTrait(new BasicCommandTrait<TActorId, TCommand>());
        }
        
        public static ReferenceItemDeclarationBuilder<TActorId> WithCommand<TActorId, TCommand>(this ReferenceItemDeclarationBuilder<TActorId> builder,
                                                                                                CommandType<TActorId, TCommand> cmd)
            where TActorId : IEntityKey
        {
            builder.WithTrait(CommandInProgressTrait<TActorId>.Instance);
            return builder.WithTrait(new BasicCommandTrait<TActorId, TCommand>(cmd.Handler));
        }
    }
}
