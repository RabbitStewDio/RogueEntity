using tainicom.Aether.Physics2D.Common;

namespace RogueEntity.Physics2D.AetherPhysics;

public readonly struct AetherMomentumRecord
{
    public readonly Vector2 LinearVelocity;
    public readonly float AngularVelocity;
    public readonly int TimeOfRecord;

    public AetherMomentumRecord(Vector2 linearVelocity, float angularVelocity, int timeOfRecord)
    {
        LinearVelocity = linearVelocity;
        AngularVelocity = angularVelocity;
        TimeOfRecord = timeOfRecord;
    }
}