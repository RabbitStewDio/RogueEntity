using System;
using EnTTSharp.Entities;
using MessagePack;
using MessagePack.Formatters;

namespace RogueEntity.Core.Infrastructure.Actions
{
    /// <summary>
    ///   Actions are commonly stored in polymorphic lists. When deserializing those
    ///   lists, we won't normally have the required type information to correctly
    ///   create instances of the concrete action type in MessagePack. (For performance
    ///   and space conservation MessagePack does not serialize type information by
    ///   default, as most data types are static and not prone to change).
    ///
    ///   This action resolver writes actions that are referenced by the action interface
    ///   as message pack objects with additional type information. Its slightly more
    ///   convoluted than just slapping a "use the TypedFormatterResolver" on each action
    ///   reference, but grants action authors the freedom to implement their own serialization
    ///   if desired.
    /// </summary>
    /// <typeparam name="TContext"></typeparam>
    /// <typeparam name="TActorId"></typeparam>
    public class ActionFormatterResolver<TContext, TActorId> : IFormatterResolver
        where TActorId : IEntityKey
    {
        public IMessagePackFormatter<T> GetFormatter<T>()
        {
            if (typeof(IAction<TContext, TActorId>) == typeof(T))
            {
                return (IMessagePackFormatter<T>)Default;
            }

            return null;
        }

        readonly BaseActionFormatter Default = new BaseActionFormatter();

        class BaseActionFormatter : IMessagePackFormatter<IAction<TContext, TActorId>>
        {
            public void Serialize(ref MessagePackWriter writer, IAction<TContext, TActorId> value, MessagePackSerializerOptions options)
            {
                if (value == null)
                {
                    writer.WriteNil();
                    return;
                }

                writer.WriteArrayHeader(2);
                writer.Write(value.GetType().AssemblyQualifiedName);
                MessagePackSerializer.Serialize(value.GetType(), ref writer, value, options);
            }

            public IAction<TContext, TActorId> Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
            {
                if (reader.TryReadNil())
                {
                    return null;
                }

                var count = reader.ReadArrayHeader();
                if (count != 2)
                {
                    throw new MessagePackSerializationException("Unexpected number of array elements for typed IAction object.");
                }
                var typeName = reader.ReadString();
                var type = Type.GetType(typeName);
                if (type == null || !typeof(IAction<TContext, TActorId>).IsAssignableFrom(type))
                {
                    throw new MessagePackSerializationException($"Unable to load referenced action type {typeName}");
                }

                return (IAction<TContext, TActorId>) MessagePackSerializer.Deserialize(type, ref reader, options);
            }
        }
    }
}