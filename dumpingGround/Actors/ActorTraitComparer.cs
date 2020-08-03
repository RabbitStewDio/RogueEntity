using System;
using System.Collections.Generic;
using EnttSharp.Entities;

namespace ValionRL.Core.Infrastructure.Meta.Actors
{
    public class ActorTraitComparer<TContext, TActorId> : IComparer<IActorTrait<TContext, TActorId>> 
        where TActorId : IEntityKey
    {
        public static readonly ActorTraitComparer<TContext, TActorId> Default = new ActorTraitComparer<TContext, TActorId>();

        public int Compare(IActorTrait<TContext, TActorId> x, IActorTrait<TContext, TActorId> y)
        {
            if (x == null && y == null)
            {
                return 0;
            }

            if (x == null)
            {
                return -1;
            }

            if (y == null)
            {
                return 1;
            }

            var p = x.Priority.CompareTo(y.Priority);
            if (p != 0)
            {
                return p;
            }

            return string.Compare(x.Id, y.Id, StringComparison.Ordinal);
        }
    }
}