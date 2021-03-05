using EnTTSharp.Entities.Attributes;
using MessagePack;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Utils;
using System.Runtime.Serialization;

namespace RogueEntity.Core.Inputs.Commands
{
    [EntityComponent]
    [DataContract]
    [MessagePackObject]
    public readonly struct CommandInProgress
    {
        public readonly Optional<ItemTraitId> ActiveCommand;
        public readonly bool Handled;

        public CommandInProgress(bool handled, Optional<ItemTraitId> activeCommand)
        {
            this.Handled = handled;
            this.ActiveCommand = activeCommand;
        }

        public CommandInProgress MarkHandled()
        {
            return new CommandInProgress(true, ActiveCommand);
        }
    }
}
