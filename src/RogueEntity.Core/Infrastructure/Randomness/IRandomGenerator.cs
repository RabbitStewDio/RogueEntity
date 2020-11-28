using System;

namespace RogueEntity.Core.Infrastructure.Randomness
{
    public interface IRandomGenerator: IDisposable
    {
        double Next();
    }
}