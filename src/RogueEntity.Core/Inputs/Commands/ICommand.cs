﻿namespace RogueEntity.Core.Inputs.Commands
{
    /// <summary>
    ///  Commands are data packages that tell the backend about player intentions.
    /// </summary>
    public interface ICommand
    {
        string Id { get; }
    }
}