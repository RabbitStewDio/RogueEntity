using System;
using JetBrains.Annotations;

namespace RogueEntity.Core.Infrastructure.Modules.Attributes
{
    /// <summary>
    ///   Marks general module initializers. Those methods must have the signature
    ///  <![CDATA[
    ///      void YourInitializerName<TGameContext>(IServiceResolver serviceResolver, IModuleInitializer<TGameContext> initializer);
    ///  ]]>
    /// </summary>
    [MeansImplicitUse]
    [AttributeUsage(AttributeTargets.Method)]
    public class ModuleInitializerAttribute: Attribute
    {
    }
}