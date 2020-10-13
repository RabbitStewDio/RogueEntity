using System;

namespace RogueEntity.Core.Sensing.Common.Ripple
{
    public interface IRipplePropagationWorkingStateSource : IDisposable
    {
        RippleSenseData CreateData(int radius);
    }
}