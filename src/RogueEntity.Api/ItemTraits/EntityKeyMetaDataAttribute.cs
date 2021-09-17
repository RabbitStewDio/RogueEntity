using JetBrains.Annotations;
using System;

namespace RogueEntity.Api.ItemTraits
{
    [MeansImplicitUse]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class EntityKeyMetaDataAttribute : Attribute
    {
        public Type MetaData
        {
            get;
            set;
        }

        public EntityKeyMetaDataAttribute(Type metaData)
        {
            MetaData = metaData;
        }
    }
}
