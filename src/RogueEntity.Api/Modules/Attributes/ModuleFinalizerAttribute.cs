using JetBrains.Annotations;
using System;

namespace RogueEntity.Api.Modules.Attributes
{
    /// <summary>
    ///   Marks general module finalizer. Those methods must have the signature
    ///  <![CDATA[
    ///      void YourFinalizerName(in ModuleInitializerParameter serviceResolver, IModuleInitializer initializer);
    ///  ]]>
    /// </summary>
    [MeansImplicitUse]
    [AttributeUsage(AttributeTargets.Method)]
    public class ModuleFinalizerAttribute: Attribute
    {
    }
}
