using System;
using System.Collections.Generic;
using EnTTSharp.Annotations;
using EnTTSharp.Entities;
using EnTTSharp.Serialization;
using EnTTSharp.Serialization.Binary;
using MessagePack;
using MessagePack.Formatters;
using MessagePack.Resolvers;

namespace RogueEntity.Core.Infrastructure.Serialization
{
    public class BinarySerializationContext
    {
        readonly List<IFormatterResolver> formatterResolvers;
        readonly Dictionary<Type, IMessagePackFormatter> messageFormatters;
        readonly List<EntityComponentRegistration> componentRegistrations;

        public BinarySerializationContext()
        {
            componentRegistrations = new List<EntityComponentRegistration>();
            formatterResolvers = new List<IFormatterResolver>();
            messageFormatters = new Dictionary<Type, IMessagePackFormatter>();
        }

        public void Register(IFormatterResolver resolver)
        {
            if (resolver == null)
            {
                throw new ArgumentNullException(nameof(resolver));
            }

            formatterResolvers.Add(resolver);
        }

        public void Register<TData>(IMessagePackFormatter<TData> fmt)
        {
            if (fmt == null)
            {
                throw new ArgumentNullException(nameof(fmt));
            }

            if (messageFormatters.TryGetValue(typeof(TData), out _))
            {
                // maybe that will change later to collate them into a polymorphic wrapper,
                // but for now assume there are no duplicates at the lowest level.
                throw new ArgumentException("Duplicate message formatter registered.");
            }

            messageFormatters.Add(typeof(TData), fmt);
        }

        public void AddComponentRegistration(EntityComponentRegistration reg) => componentRegistrations.Add(reg);


        public IFormatterResolver CreateResolver<TItemId>(EntityKeyMapper<TItemId> mapper) where TItemId : IEntityKey
        {
            var resolvers = new List<IFormatterResolver>();
            resolvers.AddRange(this.formatterResolvers);
            
            var formatters = new List<IMessagePackFormatter>();
            formatters.AddRange(this.messageFormatters.Values);

            foreach (var c in componentRegistrations)
            {
                if (c == null)
                {
                    continue;
                }

                if (c.TryGet(out BinaryWriteHandlerRegistration w))
                {
                    if (w.TryGetResolverFactory<TItemId>(out var factory))
                    {
                        var resolver = factory(mapper);
                        if (resolver != null)
                        {
                            resolvers.Add(resolver);
                        }
                    }
                    else if (w.TryGetMessagePackFormatterFactory<TItemId>(out var messageFormatter))
                    {
                        var fmt = messageFormatter(mapper);
                        if (fmt != null)
                        {
                            formatters.Add(fmt);
                        }
                    }
                }
                else if (c.TryGet(out BinaryReadHandlerRegistration r))
                {
                    if (r.TryGetResolverFactory<TItemId>(out var factory))
                    {
                        var resolver = factory(mapper);
                        if (resolver != null)
                        {
                            resolvers.Add(resolver);
                        }
                    }
                    else if (r.TryGetMessagePackFormatterFactory<TItemId>(out var messageFormatter))
                    {
                        var fmt = messageFormatter(mapper);
                        if (fmt != null)
                        {
                            formatters.Add(fmt);
                        }
                    }
                }
            }

            
            resolvers.Add(StandardResolverAllowPrivate.Instance);
            return CompositeResolver.Create(formatters, resolvers);
        }
    }
}