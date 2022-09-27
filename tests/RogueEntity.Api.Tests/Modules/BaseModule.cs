using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Modules;
using RogueEntity.Api.Modules.Attributes;

namespace RogueEntity.Api.Tests.Modules
{
    [Module]
    public class BaseModule : ModuleBase
    {
        public static readonly EntityRole BaseRole = new EntityRole("Role.Test.Base");

        public BaseModule()
        {
            Id = "Base";

            RequireRole(BaseRole);
        }
    }

    [Module]
    public class InvalidModule : ModuleBase
    {
        // this module intentionally does not define an Id or any other metadata.
    }

    
    public class ActivateEntityModule<TItemId> : ModuleBase
        where TItemId : struct, IEntityKey
    {
        public static readonly EntityRole ActivatorRole = new EntityRole("Role.Test.ActiveEntity+" + typeof(TItemId).Name);

        public ActivateEntityModule()
        {
            Id = "ActivateEntity+" + typeof(TItemId).Name;
        }

        [ContentInitializer]
        public void DeclareEntity(in ModuleInitializationParameter mp, IModuleInitializer ctx)
        {
            ctx.DeclareContentContext<TItemId>().DeclareTraitRoles<TItemId>(ActivatorRole);
        }
    }
    
}
