using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace RogueEntity.Core.Utils
{
    public static class Assert
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void NotNull<T>([NotNull] in T? checkedObject, [CallerArgumentExpression("checkedObject")] string? argHint = null)
        {
            #if DEBUG
            if (checkedObject == null) throw new ArgumentNullException(argHint);
            #endif
        }
    }
}