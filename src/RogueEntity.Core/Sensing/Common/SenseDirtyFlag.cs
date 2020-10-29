using System.Diagnostics.CodeAnalysis;

namespace RogueEntity.Core.Sensing.Common
{
    [SuppressMessage("ReSharper", "UnusedTypeParameter")]
    public readonly struct SenseDirtyFlag<TSense> where TSense: ISense
    {
    }
}