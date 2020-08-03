using EnttSharp.Entities;

namespace ValionRL.Core.Infrastructure.Meta.Actors
{
    public readonly struct ActorReference : IEntityKey
    {
        public EntityKey Data { get; }

        public ActorReference(byte age, int key)
        {
            this.Data = EntityKey.Create(age, key);
        }

        public bool Empty
        {
            get { return Data.Age == 0 && Data.Key == -1; }
        }

        public override string ToString()
        {
            if (Empty)
            {
                return "ActorRef[Empty]";
            }

            return $"ActorRef[Named]{Data}";
        }


        public bool Equals(ActorReference other)
        {
            return Data == other.Data;
        }

        public bool Equals(IEntityKey obj)
        {
            return obj is ActorReference other && Equals(other);
        }

        public override bool Equals(object obj)
        {
            return obj is ActorReference other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Data.GetHashCode();
        }

        public static bool operator ==(ActorReference left, ActorReference right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ActorReference left, ActorReference right)
        {
            return !left.Equals(right);
        }

        public byte Age
        {
            get { return Data.Age; }
        }

        public int Key
        {
            get { return Data.Key; }
        }
    }
}