namespace RogueEntity.Core.Movement.ItemCosts
{
    /// <summary>
    ///   A holder structure for the 4 commonly used RPG movement modes.
    ///
    ///   Usually when making decisions about moving we want to consider all movement options
    ///   available to a character or item. This class facilitates a combined query to get
    ///   all 4 modes in one operation.
    /// </summary>
    public readonly struct MovementCostProperties
    {
        public static readonly MovementCostProperties Empty;
        public static readonly MovementCostProperties WalkingBlocked = new MovementCostProperties(MovementCost.Blocked, MovementCost.Free, MovementCost.Free, MovementCost.Blocked);
        public static readonly MovementCostProperties WalkingAndFlyingBlocked = new MovementCostProperties(MovementCost.Blocked, MovementCost.Blocked, MovementCost.Free, MovementCost.Blocked);
        public static readonly MovementCostProperties Blocked = new MovementCostProperties(MovementCost.Blocked, MovementCost.Blocked, MovementCost.Blocked, MovementCost.Blocked);

        public readonly MovementCost Walking;
        public readonly MovementCost Flying;
        public readonly MovementCost Ethereal;
        public readonly MovementCost Swimming;

        public MovementCostProperties(MovementCost walking, MovementCost flying, MovementCost ethereal, MovementCost swimming)
        {
            this.Swimming = swimming;
            this.Walking = walking;
            this.Flying = flying;
            this.Ethereal = ethereal;
        }

        public static MovementCostProperties operator +(MovementCostProperties p, MovementCostProperties q)
        {
            return new MovementCostProperties(
                p.Walking.Combine(q.Walking),
                p.Flying.Combine(q.Flying),
                p.Ethereal.Combine(q.Ethereal),
                p.Swimming.Combine(q.Swimming)
            );
        }

        public override string ToString()
        {
            return $"{nameof(Walking)}: {Walking}, {nameof(Flying)}: {Flying}, {nameof(Ethereal)}: {Ethereal}, {nameof(Swimming)}: {Swimming}";
        }
    }
}