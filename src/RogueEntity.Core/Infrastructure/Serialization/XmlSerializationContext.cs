using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using EnTTSharp.Annotations;
using EnTTSharp.Serialization;
using EnTTSharp.Serialization.Xml;

namespace RogueEntity.Core.Infrastructure.Serialization
{
    public class XmlSerializationContext
    {
        readonly Dictionary<Type, ISerializationSurrogateProvider> surrogateMappings;
        readonly List<EntityComponentRegistration> componentRegistrations;

        public XmlSerializationContext()
        {
            surrogateMappings = new Dictionary<Type, ISerializationSurrogateProvider>();
            componentRegistrations = new List<EntityComponentRegistration>();
        }

        public void Register<TTarget, TSurrogate>(SerializationSurrogateProviderBase<TTarget, TSurrogate> provider)
        {
            Register(typeof(TTarget), provider);
        }

        public void Register(Type targetType, ISerializationSurrogateProvider provider)
        {
            if (provider == null)
            {
                throw new ArgumentNullException(nameof(provider));
            }

            surrogateMappings[targetType] = provider;
        }

        public void AddComponentRegistration(EntityComponentRegistration reg) => componentRegistrations.Add(reg);
        
        public ObjectSurrogateResolver Populate<TItemId>(ObjectSurrogateResolver os, EntityKeyMapper<TItemId> mapper)
        {
            foreach (var s in surrogateMappings)
            {
                os.Register(s.Key, s.Value);
            }
            
            foreach (var c in componentRegistrations)
            {
                if (c == null)
                {
                    continue;
                }

                if (c.TryGet(out XmlWriteHandlerRegistration w))
                {
                    if (w.TryGetResolverFactory<TItemId>(out var factory))
                    {
                        var resolver = factory(mapper);
                        if (resolver != null)
                        {
                            os.Register(w.TargetType, resolver);
                        }
                    }
                }
                else if (c.TryGet(out XmlReadHandlerRegistration r))
                {
                    if (r.TryGetResolverFactory<TItemId>(out var factory))
                    {
                        var resolver = factory(mapper);
                        if (resolver != null)
                        {
                            os.Register(r.TargetType, resolver);
                        }
                    }
                }
            }

            return os;
        }
    }
}