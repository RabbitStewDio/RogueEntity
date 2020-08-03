namespace ValionRL.Core.Generator
{
    public readonly struct PendingPlacementRecord
    {
        public readonly int X;
        public readonly int Y;
        public readonly int DistanceFromStart;

        public PendingPlacementRecord(int x, int y, int distanceFromStart)
        {
            X = x;
            Y = y;
            DistanceFromStart = distanceFromStart;
        }

        public override string ToString()
        {
            return $"{nameof(X)}: {X}, {nameof(Y)}: {Y}, {nameof(DistanceFromStart)}: {DistanceFromStart}";
        }
    }
}