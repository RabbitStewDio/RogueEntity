using EnTTSharp.Annotations;
using EnTTSharp.Entities;
using EnTTSharp.Serialization;
using EnTTSharp.Serialization.Binary;
using EnTTSharp.Serialization.Binary.AutoRegistration;
using EnTTSharp.Serialization.Binary.Impl;
using MessagePack;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Core.Meta.Items;
using System.Collections.Generic;

namespace RogueEntity.Core.Infrastructure.Serialization
{
    public class BinarySerializer<TEntityId>
        where TEntityId : IEntityKey
    {
        readonly EntityRegistrationScanner registrationScanner;
        readonly BinaryWriteHandlerRegistry writeHandlerRegistry;
        readonly BinaryReadHandlerRegistry readHandlerRegistry;
        readonly List<EntityComponentRegistration> componentRegistrations;
        
        BinarySerializer()
        {
            registrationScanner = new EntityRegistrationScanner().With(new BinaryEntityRegistrationHandler());
            writeHandlerRegistry = new BinaryWriteHandlerRegistry();
        }

        public void RegisterComponent<TComponent>()
        {
            if (registrationScanner.TryRegisterComponent<TComponent>(out var registration))
            {
                this.writeHandlerRegistry.Register(registration);
                this.readHandlerRegistry.Register(registration);
                this.componentRegistrations.Add(registration);
            }
        }

        public void RegisterEntityKey<TEntityKey>(IItemResolver<TEntityKey> itemResolver)
            where TEntityKey : IEntityKey
        {
            if (registrationScanner.TryRegisterKey<TEntityKey>(out var reg))
            {
                this.writeHandlerRegistry.Register(reg);
                this.readHandlerRegistry.Register(reg);
                this.componentRegistrations.Add(reg);
            }
        }

        BinaryBulkArchiveReader<TEntityId> CreateReader(ISnapshotLoader<TEntityId> ld)
        {
            var readerBackend = new BinaryReaderBackend<TEntityId>(readHandlerRegistry);
            var msgPackOptions = MessagePackSerializerOptions.Standard.WithResolver(CreateMessageResolver(new DefaultEntityKeyMapper().Register(ld.Map)));
            return new BinaryBulkArchiveReader<TEntityId>(readerBackend, msgPackOptions);
        }
        
        IFormatterResolver CreateMessageResolver(IEntityKeyMapper mapper)
        {
            var bs = new BinarySerializationContext();
            
            bs.Register(new EntityKeyDataFormatter());
            
            foreach (var c in componentRegistrations)
            {
                bs.AddComponentRegistration(c);
            }

            return bs.CreateResolver(mapper);
        }

        interface IEntityKeyRegistration
        {
            void Register(BinarySerializationContext ctx);
        }

        class EntityKeyRegistration<TEntityKey>
            where TEntityKey : IEntityKey
        {
            readonly IItemResolver<TEntityKey> itemResolver;

            public void Register(BinarySerializationContext ctx, IEntityKeyMapper mapper)
            {
                var bulkIdSerializationMapper = new BulkItemIdSerializationMapper<TEntityKey>(itemResolver.EntityMetaData, 
                                                                                             itemResolver.ItemRegistry.BulkItemMapping, 
                                                                                             itemResolver.ItemRegistry.BulkItemMapping);
                ctx.Register(new BulkKeyMessagePackFormatter<TEntityKey>(itemResolver.EntityMetaData, mapper, bulkIdSerializationMapper.TryMap));
                
            }
        }
    }
}
