using System;
using JetBrains.Annotations;

namespace RogueEntity.Api.Modules.Attributes
{
    /// <summary>
    ///   Marks general module initializers. Those methods must have the signature
    ///  <![CDATA[
    ///      void YourInitializerName(in ModuleInitializerParameter serviceResolver, IModuleInitializer initializer);
    ///  ]]>
    /// </summary>
    [MeansImplicitUse]
    [AttributeUsage(AttributeTargets.Method)]
    public class ModuleInitializerAttribute: Attribute
    {
    }
}