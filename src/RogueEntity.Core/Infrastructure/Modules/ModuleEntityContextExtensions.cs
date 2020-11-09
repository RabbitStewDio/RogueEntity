using EnTTSharp.Entities;

namespace RogueEntity.Core.Infrastructure.Modules
{
    public static class ModuleEntityContextExtensions 
    {
        public static EntitySystemRegistrationDelegate<TGameContext, TEntityId> Empty<TGameContext, TEntityId>(this IModuleEntityContext<TGameContext, TEntityId> ctx)
            where TEntityId : IEntityKey
        {
            return (a, b, c, d) =>
            {
            };
        }
    }
}