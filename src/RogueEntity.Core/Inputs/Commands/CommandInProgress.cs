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

        public CommandInProgress(Optional<ItemTraitId> activeCommand)
        {
            ActiveCommand = activeCommand;
        }
    }
}
