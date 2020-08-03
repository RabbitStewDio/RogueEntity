using System;

namespace ValionRL.Core.Generator
{
    public readonly struct Edge<TNode> where TNode : class
    {
        public readonly TNode Source;
        public readonly TNode Target;
        public readonly bool Horizontal;

        Edge(TNode source, TNode target, bool horizontal)
        {
            Source = source;
            Target = target;
            Horizontal = horizontal;
        }

        public Edge(TNode source, bool horizontal)
        {
            Source = source;
            Horizontal = horizontal;
            Target = default;
        }

        public Edge<TNode> WithTarget(TNode target)
        {
            if (Target == null)
            {
                return new Edge<TNode>(Source, target, Horizontal);
            }

            throw new ArgumentException("Target is already occupied: " + Target + " vs " + target);
        }

        public bool ValidSource()
        {
            return Source != null;
        }

        public bool ValidTarget()
        {
            return Target != null;
        }

        public override string ToString()
        {
            return $"Edge({nameof(Source)}: {Source}, {nameof(Target)}: {Target}, {nameof(Horizontal)}: {Horizontal})";
        }
    }
}