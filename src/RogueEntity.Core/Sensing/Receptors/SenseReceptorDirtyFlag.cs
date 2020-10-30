using System.Diagnostics.CodeAnalysis;

namespace RogueEntity.Core.Sensing.Receptors
{
    [SuppressMessage("ReSharper", "UnusedTypeParameter")]
    public readonly struct SenseReceptorDirtyFlag<TReceptorSense, TSourceSense> 
        where TReceptorSense: ISense
        where TSourceSense: ISense
    {
        
    }
}