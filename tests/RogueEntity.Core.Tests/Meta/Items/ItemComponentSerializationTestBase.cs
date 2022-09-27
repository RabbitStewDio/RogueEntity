using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using EnTTSharp.Annotations;
using EnTTSharp.Entities;
using EnTTSharp.Serialization;
using EnTTSharp.Serialization.Binary;
using EnTTSharp.Serialization.Binary.AutoRegistration;
using EnTTSharp.Serialization.Binary.Impl;
using EnTTSharp.Serialization.Xml;
using EnTTSharp.Serialization.Xml.AutoRegistration;
using FluentAssertions;
using MessagePack;
using NUnit.Framework;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Core.Infrastructure.Serialization;
using RogueEntity.Core.Meta.Items;

namespace RogueEntity.Core.Tests.Meta.Items
{
    public abstract class ItemComponentSerializationTestBase<TItemId, TData>
        where TItemId : struct, IBulkDataStorageKey<TItemId>
    {
        protected bool EnableSerializationTest { get; set; } = true;
        protected abstract List<ItemDeclarationId> ActiveItems { get; }
        protected EntityRegistry<TItemId> EntityRegistry => itemContext.EntityRegistry;
        protected IItemRegistryBackend< TItemId> ItemRegistry => itemContext.ItemRegistry;
        protected IItemResolver< TItemId> ItemResolver => itemContext.ItemResolver;
        protected IBulkDataStorageMetaData<TItemId> ItemIdMetaData { get; }
        protected virtual EntityRelations<TItemId> ProduceItemRelations(TItemId self) => new EntityRelations<TItemId>(self);
        protected abstract IItemComponentTestDataFactory<TData> ProduceTestData(EntityRelations<TItemId> relations);
        ItemContextBackend< TItemId> itemContext; 

        protected ItemComponentSerializationTestBase(IBulkDataStorageMetaData<TItemId> metaData)
        {
            ItemIdMetaData = metaData;
        }

        protected void SetUpItems()
        {
            itemContext = new ItemContextBackend< TItemId>(ItemIdMetaData);
        }

        [Test]
        public void Validate_Serialization_Xml()
        {
            if (!EnableSerializationTest)
            {
                Console.WriteLine("Serialization tests disabled");
                return;
            }

            foreach (var e in ActiveItems)
            {
                Validate_Serialization_Xml(e);
            }
        }

        [Test]
        public void Validate_Serialization_Binary()
        {
            if (!EnableSerializationTest)
            {
                Console.WriteLine("Serialization tests disabled");
                return;
            }

            foreach (var e in ActiveItems)
            {
                Validate_Serialization_Binary(e);
            }
        }

        protected virtual void Validate_Serialization_Xml(ItemDeclarationId itemId)
        {
            EntityRegistry.Clear();

            var item = ItemResolver.Instantiate(itemId);
            if (!this.ItemIdMetaData.IsReferenceEntity(item))
            {
                return;
            }

            var testData = ProduceTestData(ProduceItemRelations(item));
            if (testData.UpdateAllowed)
            {
                ItemResolver.TryUpdateData(item, testData.ChangedValue, out item).Should().BeTrue();
            }

            var xml = SerializeToXml();

            Console.WriteLine("XML: " + xml);

            EntityRegistry.Clear();
            EntityRegistry.Create();

            var arg = DeserializeFromXml(xml, item);
            ItemIdMetaData.IsReferenceEntity(arg).Should().BeTrue();
            arg.Should().NotBe(item);

            ItemResolver.TryQueryData(arg, out TData beforeUpdate).Should().BeTrue();

            var testDataAfterSer = ProduceTestData(ProduceItemRelations(arg));
            if (testData.UpdateAllowed)
            {
                beforeUpdate.Should().Be(testDataAfterSer.ChangedValue);
            }
            else
            {
                testDataAfterSer.TryGetInitialValue(out var changedVal).Should().BeTrue();
                beforeUpdate.Should().Be(changedVal);
            }
        }

        protected virtual void Validate_Serialization_Binary(ItemDeclarationId itemId)
        {
            EntityRegistry.Clear();

            var item = ItemResolver.Instantiate(itemId);
            if (!ItemIdMetaData.IsReferenceEntity(item))
            {
                return;
            }

            var testData = ProduceTestData(ProduceItemRelations(item));
            if (testData.UpdateAllowed)
            {
                ItemResolver.TryUpdateData(item, testData.ChangedValue, out item).Should().BeTrue();
            }

            var xml = SerializeToBinary();

            EntityRegistry.Clear();
            EntityRegistry.Create();

            var arg = DeserializeFromBinary(xml, item);

            arg.Should().NotBe(item);

            ItemResolver.TryQueryData(arg, out TData beforeUpdate).Should().BeTrue();

            var testDataAfterSer = ProduceTestData(ProduceItemRelations(arg));
            if (testData.UpdateAllowed)
            {
                beforeUpdate.Should().Be(testDataAfterSer.ChangedValue);
            }
            else
            {
                testDataAfterSer.TryGetInitialValue(out var changedVal).Should().BeTrue();
                beforeUpdate.Should().Be(changedVal);
            }
        }

        string SerializeToXml()
        {
            EntityRegistry.Count.Should().NotBe(0);

            var surr = new ObjectSurrogateResolver();

            var scn = new EntityRegistrationScanner().With(new XmlEntityRegistrationHandler(surr))
                                                     .With(new XmlDataContractRegistrationHandler(surr));
            if (!scn.TryRegisterComponent<ItemDeclarationHolder< TItemId>>(out var itemDeclarationRegistration))
            {
                Assert.Fail("Unable to register item declaration type.");
            }

            var registration = PerformEntityComponentRegistration(scn);

            if (!scn.TryRegisterKey<TItemId>(out var keyRegistration))
            {
                Assert.Fail("Unable to register key type");
            }

            var reg = new XmlWriteHandlerRegistry().Register(registration)
                                                   .Register(itemDeclarationRegistration);

            var xmlWriterSettings = new XmlWriterSettings
            {
                Indent = true
            };

            using (var sn = EntityRegistry.CreateSnapshot())
            {
                PopulateSurrogateResolver(surr, null, registration, keyRegistration);

                var sbuff = new StringWriter();
                var xml = XmlWriter.Create(sbuff, xmlWriterSettings);
                var wrt = new XmlArchiveWriter<TItemId>(reg, xml);

                sn.WriteAll(wrt);
                wrt.FlushFrame();
                return sbuff.ToString();
            }
        }

        TItemId DeserializeFromXml(string xml, TItemId originalId)
        {
            var surr = new ObjectSurrogateResolver();
            var scn = new EntityRegistrationScanner().With(new XmlEntityRegistrationHandler(surr))
                                                     .With(new XmlDataContractRegistrationHandler(surr));

            PerformBaseEntityComponentRegistration(scn, out var itemDeclarationRegistration, out var keyRegistration);

            var registration = PerformEntityComponentRegistration(scn);


            var reg = new XmlReadHandlerRegistry().Register(registration)
                                                  .Register(itemDeclarationRegistration);

            using (var ld = EntityRegistry.CreateLoader())
            {
                var mapper = new DefaultEntityKeyMapper().Register(ld.Map);
                PopulateSurrogateResolver(surr, mapper, registration, keyRegistration);

                var reader = new XmlBulkArchiveReader<TItemId>(reg);
                var xmlReader = XmlReader.Create(new StringReader(xml));
                reader.ReadAll(xmlReader, ld, mapper);
                if (!ld.TryLookupMapping(originalId, out var retval))
                {
                    Assert.Fail("Unable to locate input in deserialized data.");
                }

                return retval;
            }
        }

        static void PerformBaseEntityComponentRegistration(EntityRegistrationScanner scn,
                                                           out EntityComponentRegistration itemDeclarationRegistration,
                                                           out EntityComponentRegistration keyRegistration)
        {
            if (!scn.TryRegisterComponent<ItemDeclarationHolder< TItemId>>(out itemDeclarationRegistration))
            {
                Assert.Fail("Unable to register item declaration type.");
            }

            if (!scn.TryRegisterKey<TItemId>(out keyRegistration))
            {
                Assert.Fail("Unable to register key type");
            }
        }

        void PopulateSurrogateResolver(ObjectSurrogateResolver surr, 
                                       IEntityKeyMapper mapper,
                                       params EntityComponentRegistration[] components)
        {
            var xmlContext = new XmlSerializationContext();
            
            var bulkIdSerializationMapper = new BulkItemIdSerializationMapper<TItemId>(ItemIdMetaData, ItemRegistry, ItemRegistry);
            xmlContext.Register(new ItemDeclarationHolderSurrogateProvider< TItemId>(ItemResolver));
            xmlContext.Register(new BulkKeySurrogateProvider<TItemId>(ItemIdMetaData, mapper, bulkIdSerializationMapper.TryMap));
            foreach (var c in components)
            {
                xmlContext.AddComponentRegistration(c);
            }

            CustomizeXmlSerializationContext(mapper, xmlContext);
            
            xmlContext.Populate(surr, mapper);
        }


        byte[] SerializeToBinary()
        {
            var scn = new EntityRegistrationScanner().With(new BinaryEntityRegistrationHandler());
            PerformBaseEntityComponentRegistration(scn, out var itemDeclarationRegistration, out var keyRegistration);
            var registration = PerformEntityComponentRegistration(scn);

            var reg = new BinaryWriteHandlerRegistry().Register(registration)
                                                      .Register(itemDeclarationRegistration);


            using (var sn = EntityRegistry.CreateSnapshot())
            {
                var stream = new MemoryStream();

                var msgPackOptions = MessagePackSerializerOptions.Standard
                                                                 .WithResolver(CreateMessageResolver(new DefaultEntityKeyMapper().Register(WriteMapper), 
                                                                                                     registration, 
                                                                                                     keyRegistration)
                                                                               );
                var writer = new BinaryArchiveWriter<TItemId>(reg, stream, msgPackOptions);

                sn.WriteAll(writer);
                return stream.ToArray();
            }
        }

        protected virtual EntityComponentRegistration PerformEntityComponentRegistration(EntityRegistrationScanner scn)
        {
            if (!scn.TryRegisterComponent<TData>(out var registration))
            {
                Assert.Fail("Unable to register component type " + typeof(TData));
            }

            return registration;
        }

        TItemId WriteMapper(EntityKeyData d) => ItemIdMetaData.CreateReferenceKey(d.Age, d.Key);
        
        TItemId DeserializeFromBinary(byte[] data, TItemId originalId)
        {
            var scn = new EntityRegistrationScanner().With(new BinaryEntityRegistrationHandler());
            PerformBaseEntityComponentRegistration(scn, out var itemDeclarationRegistration, out var keyRegistration);
            var registration = PerformEntityComponentRegistration(scn);

            var reg = new BinaryReadHandlerRegistry().Register(registration)
                                                     .Register(itemDeclarationRegistration);


            using (var ld = EntityRegistry.CreateLoader())
            {
                var readerBackend = new BinaryReaderBackend<TItemId>(reg);
                var msgPackOptions = MessagePackSerializerOptions.Standard.WithResolver(CreateMessageResolver(new DefaultEntityKeyMapper().Register(ld.Map), 
                                                                                                              registration, keyRegistration));
                var reader = new BinaryBulkArchiveReader<TItemId>(readerBackend, msgPackOptions);

                var stream = new MemoryStream(data);

                reader.ReadAll(stream, ld);
                if (!ld.TryLookupMapping(originalId, out var retval))
                {
                    Assert.Fail("Unable to locate input in deserialized data.");
                }

                return retval;
            }
        }

        IFormatterResolver CreateMessageResolver(IEntityKeyMapper mapper,
                                                 params EntityComponentRegistration[] components)
        {
            var bs = new BinarySerializationContext();
            
            var bulkIdSerializationMapper = new BulkItemIdSerializationMapper<TItemId>(ItemIdMetaData, ItemRegistry, ItemRegistry);
            bs.Register(new EntityKeyDataFormatter());
            bs.Register(new ItemDeclarationHolderMessagePackFormatter< TItemId>(ItemResolver));
            bs.Register(new BulkKeyMessagePackFormatter<TItemId>(ItemIdMetaData, mapper, bulkIdSerializationMapper.TryMap));
            
            foreach (var c in components)
            {
                bs.AddComponentRegistration(c);
            }

            CustomizeBinarySerializationContext(mapper, bs);
            return bs.CreateResolver(mapper);
        }

        protected virtual void CustomizeXmlSerializationContext(IEntityKeyMapper mapper, XmlSerializationContext bs)
        {
            
        }
        
        protected virtual void CustomizeBinarySerializationContext(IEntityKeyMapper mapper, BinarySerializationContext bs)
        {
            
        }        
    }
}