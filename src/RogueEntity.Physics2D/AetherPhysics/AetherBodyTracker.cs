using System.Diagnostics.CodeAnalysis;
using tainicom.Aether.Physics2D.Dynamics;

namespace RogueEntity.Physics2D.AetherPhysics;

public readonly struct AetherBodyTracker
{
    readonly Body? body;

    public AetherBodyTracker(Body? body)
    {
        this.body = body;
    }

    public bool TryGetBody([MaybeNullWhen(false)] out Body f)
    {
        f = body;
        return f != null;
    }
}