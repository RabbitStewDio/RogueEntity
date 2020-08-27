using System;
using EnTTSharp.Entities;

namespace RogueEntity.Core.Infrastructure.Actions.Idle
{
    public class IdleAction<TGameContext, TActorId> : IAction<TGameContext, TActorId> 
        where TActorId : IEntityKey
    {
        readonly int turns;

        public IdleAction(int turns = 1)
        {
            this.turns = turns;
        }

        public ActionResult Perform(IEntityViewControl<TActorId> v, TGameContext context, TActorId k, out int actionCost)
        {
            if (!v.IsValid(k))
            {
                throw new ArgumentException("This entity is not an valid actor.");
            }

            // context.Log.Log(actorInfo.Reference, 
            //                 $"{actorInfo.Reference.ToDefiniteName(context)} stood around doing nothing at all.");
            actionCost = Math.Max(turns, 1);
            return ActionResult.Success;
        }

        public bool IsMovement => true;

        public override string ToString()
        {
            return $"Idle({turns})";
        }
    }
}