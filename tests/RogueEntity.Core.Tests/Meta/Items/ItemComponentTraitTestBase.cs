﻿using System;
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
using MessagePack.Formatters;
using MessagePack.Resolvers;
using NUnit.Framework;
using RogueEntity.Core.Infrastructure.Serialization;
using RogueEntity.Core.Meta.Items;

namespace RogueEntity.Core.Tests.Meta.Items
{
    public abstract class ItemComponentTraitTestBase<TGameContext, TItemId, TData, TItemTrait>
        where TItemId : IBulkDataStorageKey<TItemId>
        where TItemTrait : IItemComponentTrait<TGameContext, TItemId, TData>
        where TGameContext : IItemContext<TGameContext, TItemId>
    {
        protected readonly ItemDeclarationId BulkItemId = "Bulk-TestSubject";
        protected readonly ItemDeclarationId ReferenceItemId = "Reference-TestSubject";

        protected TGameContext Context { get; private set; }
        protected TItemTrait SubjectTrait { get; private set; }

        readonly List<ItemDeclarationId> activeItems;

        protected ItemComponentTraitTestBase()
        {
            activeItems = new List<ItemDeclarationId>();
            EnableSerializationTest = true;
        }

        public abstract IItemComponentTestDataFactory<TData> ProduceTestData(EntityRelations<TItemId> relations);

        [SetUp]
        public void SetUp()
        {
            activeItems.Clear();
            Context = CreateContext();

            EntityRegistry.RegisterNonConstructable<ItemDeclarationHolder<TGameContext, TItemId>>();

            SubjectTrait = CreateTrait();

            if (SubjectTrait is IBulkItemTrait<TGameContext, TItemId> bulkTrait)
            {
                ItemRegistry.Register(CreateBulkItemDeclaration(bulkTrait));
                activeItems.Add(BulkItemId);
            }

            if (SubjectTrait is IReferenceItemTrait<TGameContext, TItemId> refTrait)
            {
                ItemRegistry.Register(CreateReferenceItemDeclaration(refTrait));
                activeItems.Add(ReferenceItemId);
            }

            if (activeItems.Count == 0)
            {
                throw new InvalidOperationException("No valid implementation found. The type given is neither a reference nor a bulk item trait");
            }

            if (!EntityRegistry.IsManaged<TData>())
            {
                EntityRegistry.RegisterNonConstructable<TData>();
            }
        }

        protected virtual BulkItemDeclaration<TGameContext, TItemId> CreateBulkItemDeclaration(IBulkItemTrait<TGameContext, TItemId> bulkTrait)
        {
            return new BulkItemDeclaration<TGameContext, TItemId>(BulkItemId).WithTrait(bulkTrait);
        }

        protected virtual ReferenceItemDeclaration<TGameContext, TItemId> CreateReferenceItemDeclaration(IReferenceItemTrait<TGameContext, TItemId> refTrait)
        {
            return new ReferenceItemDeclaration<TGameContext, TItemId>(ReferenceItemId).WithTrait(refTrait);
        }

        protected abstract EntityRegistry<TItemId> EntityRegistry { get; }
        protected abstract ItemRegistry<TGameContext, TItemId> ItemRegistry { get; }
        protected abstract IBulkDataStorageMetaData<TItemId> ItemIdMetaData { get; }

        protected abstract TGameContext CreateContext();
        protected abstract TItemTrait CreateTrait();

        [Test]
        public void Validate_Initialize()
        {
            foreach (var e in activeItems)
            {
                Validate_Initialize(e);
            }
        }

        [Test]
        public void Validate_Apply()
        {
            foreach (var e in activeItems)
            {
                Validate_Apply(e);
            }
        }

        protected virtual EntityRelations<TItemId> ProduceItemRelations(TItemId self) => new EntityRelations<TItemId>(self);

        protected virtual void Validate_Apply(ItemDeclarationId itemId)
        {
            var item = Context.ItemResolver.Instantiate(Context, itemId);
            var testData = ProduceTestData(ProduceItemRelations(item));
            if (testData.UpdateAllowed)
            {
                Context.ItemResolver.TryUpdateData(item, Context, testData.ChangedValue, out item).Should().BeTrue($"because {item} has been successfully updated.");
                Context.ItemResolver.TryQueryData(item, Context, out TData data).Should().BeTrue();
                data.Should().Be(testData.ChangedValue);
            }

            Context.ItemResolver.Apply(item, Context);

            if (testData.TryGetApplyValue(out var applyValue))
            {
                Context.ItemResolver.TryQueryData(item, Context, out TData data2).Should().BeTrue();
                data2.Should().Be(applyValue, "because apply should not reset existing data.");
            }
            else
            {
                Context.ItemResolver.TryQueryData(item, Context, out TData _).Should().BeFalse();
            }
        }

        protected virtual void Validate_Initialize(ItemDeclarationId itemId)
        {
            var item = Context.ItemResolver.Instantiate(Context, itemId);
            var testData = ProduceTestData(ProduceItemRelations(item));

            if (testData.TryGetInitialValue(out var initialData))
            {
                Context.ItemResolver.TryQueryData(item, Context, out TData data).Should().BeTrue();
                data.Should().Be(initialData);
            }
            else
            {
                Context.ItemResolver.TryQueryData(item, Context, out TData _).Should().BeFalse();
            }
        }

        [Test]
        public void Validate_Remove()
        {
            foreach (var e in activeItems)
            {
                Validate_Remove(e);
            }
        }

        protected virtual void Validate_Remove(ItemDeclarationId itemId)
        {
            var item = Context.ItemResolver.Instantiate(Context, itemId);
            var testData = ProduceTestData(ProduceItemRelations(item));

            if (!testData.RemoveAllowed)
            {
                // If removing data is not allowed, the contained data should not change

                Context.ItemResolver.TryQueryData(item, Context, out TData beforeRemoved).Should().BeTrue();
                Context.ItemResolver.TryRemoveData<TData>(item, Context, out item).Should().BeFalse();
                Context.ItemResolver.TryQueryData(item, Context, out TData data).Should().BeTrue();
                data.Should().Be(beforeRemoved);
            }
            else if (testData.TryGetRemoved(out var removed))
            {
                // If removing data is allowed, the contained data should be set to a valid value
                Context.ItemResolver.TryRemoveData<TData>(item, Context, out item).Should().BeTrue();
                Context.ItemResolver.TryQueryData(item, Context, out TData data).Should().BeTrue();
                data.Should().Be(removed);
            }
            else
            {
                // If removing data is allowed, the contained data should no longer be present at all.
                Context.ItemResolver.TryRemoveData<TData>(item, Context, out item).Should().BeTrue();
                Context.ItemResolver.TryQueryData(item, Context, out TData _).Should().BeFalse();
            }
        }

        [Test]
        public void Validate_Update()
        {
            foreach (var e in activeItems)
            {
                Validate_Update(e);
            }
        }

        protected virtual void Validate_Update(ItemDeclarationId itemId)
        {
            var item = Context.ItemResolver.Instantiate(Context, itemId);
            var testData = ProduceTestData(ProduceItemRelations(item));

            if (testData.UpdateAllowed)
            {
                Context.ItemResolver.TryUpdateData(item, Context, testData.ChangedValue, out item).Should().BeTrue();

                Context.ItemResolver.TryQueryData(item, Context, out TData data).Should().BeTrue();
                data.Should().Be(testData.ChangedValue);

                if (testData.TryGetInvalid(out var invalid))
                {
                    Context.ItemResolver.TryUpdateData(item, Context, invalid, out item).Should().BeFalse();

                    Context.ItemResolver.TryQueryData(item, Context, out TData data2).Should().BeTrue();
                    data2.Should().Be(testData.ChangedValue);
                }
            }
            else
            {
                Context.ItemResolver.TryQueryData(item, Context, out TData beforeUpdate).Should().BeTrue();
                Context.ItemResolver.TryUpdateData(item, Context, testData.ChangedValue, out item).Should().BeFalse();
                Context.ItemResolver.TryQueryData(item, Context, out TData data).Should().BeTrue();
                data.Should().Be(beforeUpdate);
            }
        }

        [Test]
        public void Validate_Serialization_Xml()
        {
            if (!EnableSerializationTest)
            {
                Console.WriteLine("Serialization tests disabled");
                return;
            }

            foreach (var e in activeItems)
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

            foreach (var e in activeItems)
            {
                Validate_Serialization_Binary(e);
            }
        }

        protected bool EnableSerializationTest { get; set; }

        protected virtual void Validate_Serialization_Xml(ItemDeclarationId itemId)
        {
            EntityRegistry.Clear();

            var item = Context.ItemResolver.Instantiate(Context, itemId);
            if (!item.IsReference)
            {
                return;
            }

            var testData = ProduceTestData(ProduceItemRelations(item));
            Context.ItemResolver.TryUpdateData(item, Context, testData.ChangedValue, out item).Should().BeTrue();

            var xml = SerializeToXml();

            Console.WriteLine("XML: " + xml);

            EntityRegistry.Clear();
            EntityRegistry.Create();

            var arg = DeserializeFromXml(xml, item);
            arg.IsReference.Should().BeTrue();
            arg.Should().NotBe(item);

            Context.ItemResolver.TryQueryData(arg, Context, out TData beforeUpdate).Should().BeTrue();

            var testDataAfterSer = ProduceTestData(ProduceItemRelations(arg));
            beforeUpdate.Should().Be(testDataAfterSer.ChangedValue);
        }

        protected virtual void Validate_Serialization_Binary(ItemDeclarationId itemId)
        {
            EntityRegistry.Clear();

            var item = Context.ItemResolver.Instantiate(Context, itemId);
            if (!item.IsReference)
            {
                return;
            }

            var testData = ProduceTestData(ProduceItemRelations(item));
            Context.ItemResolver.TryUpdateData(item, Context, testData.ChangedValue, out item).Should().BeTrue();

            var xml = SerializeToBinary();

            EntityRegistry.Clear();
            EntityRegistry.Create();

            var arg = DeserializeFromBinary(xml, item);

            arg.Should().NotBe(item);

            Context.ItemResolver.TryQueryData(arg, Context, out TData beforeUpdate).Should().BeTrue();

            var testDataAfterSer = ProduceTestData(ProduceItemRelations(arg));
            beforeUpdate.Should().Be(testDataAfterSer.ChangedValue);
        }

        string SerializeToXml()
        {
            EntityRegistry.Count.Should().NotBe(0);

            var surr = new ObjectSurrogateResolver();

            var scn = new EntityRegistrationScanner().With(new XmlEntityRegistrationHandler<TItemId>(surr))
                                                     .With(new XmlDataContractRegistrationHandler<TItemId>(surr));
            if (!scn.TryRegisterComponent<ItemDeclarationHolder<TGameContext, TItemId>>(out var itemDeclarationRegistration))
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
            var scn = new EntityRegistrationScanner().With(new XmlEntityRegistrationHandler<TItemId>(surr))
                                                     .With(new XmlDataContractRegistrationHandler<TItemId>(surr));

            PerformBaseEntityComponentRegistration(scn, out var itemDeclarationRegistration, out var keyRegistration);

            var registration = PerformEntityComponentRegistration(scn);


            var reg = new XmlReadHandlerRegistry().Register(registration)
                                                  .Register(itemDeclarationRegistration);

            using (var ld = EntityRegistry.CreateLoader())
            {
                PopulateSurrogateResolver(surr, ld.Map, registration, keyRegistration);

                var reader = new XmlBulkArchiveReader<TItemId>(reg);
                var xmlReader = XmlReader.Create(new StringReader(xml));
                reader.ReadAll(xmlReader, ld);
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
            if (!scn.TryRegisterComponent<ItemDeclarationHolder<TGameContext, TItemId>>(out itemDeclarationRegistration))
            {
                Assert.Fail("Unable to register item declaration type.");
            }

            if (!scn.TryRegisterKey<TItemId>(out keyRegistration))
            {
                Assert.Fail("Unable to register key type");
            }
        }

        void PopulateSurrogateResolver(ObjectSurrogateResolver surr, 
                                                          EntityKeyMapper<TItemId> mapper,
                                                          params EntityComponentRegistration[] components)
        {
            var xmlContext = new XmlSerializationContext();
            
            var bulkIdSerializationMapper = new BulkItemIdSerializationMapper<TItemId>(ItemIdMetaData.BulkDataFactory, ItemRegistry, ItemRegistry);
            xmlContext.Register(new ItemDeclarationHolderSurrogateProvider<TGameContext, TItemId>(Context));
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
            var scn = new EntityRegistrationScanner().With(new BinaryEntityRegistrationHandler<TItemId>());
            PerformBaseEntityComponentRegistration(scn, out var itemDeclarationRegistration, out var keyRegistration);
            var registration = PerformEntityComponentRegistration(scn);

            var reg = new BinaryWriteHandlerRegistry().Register(registration)
                                                      .Register(itemDeclarationRegistration);


            using (var sn = EntityRegistry.CreateSnapshot())
            {
                var stream = new MemoryStream();

                var msgPackOptions = MessagePackSerializerOptions.Standard.WithResolver(CreateMessageResolver(WriteMapper, registration, keyRegistration));
                var writer = new BinaryArchiveWriter<TItemId>(reg, stream, msgPackOptions);

                sn.WriteAll(writer);
                return stream.ToArray();
            }
        }

        protected virtual EntityComponentRegistration PerformEntityComponentRegistration(EntityRegistrationScanner scn)
        {
            if (!scn.TryRegisterComponent<TData>(out var registration))
            {
                Assert.Fail("Unable to register component type.");
            }

            return registration;
        }

        TItemId WriteMapper(EntityKeyData d) => ItemIdMetaData.EntityKeyFactory(d.Age, d.Key);
        
        TItemId DeserializeFromBinary(byte[] data, TItemId originalId)
        {
            var scn = new EntityRegistrationScanner().With(new BinaryEntityRegistrationHandler<TItemId>());
            PerformBaseEntityComponentRegistration(scn, out var itemDeclarationRegistration, out var keyRegistration);
            var registration = PerformEntityComponentRegistration(scn);

            var reg = new BinaryReadHandlerRegistry().Register(registration)
                                                     .Register(itemDeclarationRegistration);

            var readerBackend = new BinaryReaderBackend<TItemId>(reg);

            using (var ld = EntityRegistry.CreateLoader())
            {
                var msgPackOptions = MessagePackSerializerOptions.Standard.WithResolver(CreateMessageResolver(ld.Map, registration, keyRegistration));
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

        IFormatterResolver CreateMessageResolver(EntityKeyMapper<TItemId> mapper,
                                                 params EntityComponentRegistration[] components)
        {
            var bs = new BinarySerializationContext();
            
            var bulkIdSerializationMapper = new BulkItemIdSerializationMapper<TItemId>(ItemIdMetaData.BulkDataFactory, ItemRegistry, ItemRegistry);
            bs.Register(new EntityKeyDataFormatter());
            bs.Register(new ItemDeclarationHolderMessagePackFormatter<TGameContext, TItemId>(Context));
            bs.Register(new BulkKeyMessagePackFormatter<TItemId>(ItemIdMetaData, mapper, bulkIdSerializationMapper.TryMap));
            
            foreach (var c in components)
            {
                bs.AddComponentRegistration(c);
            }

            CustomizeBinarySerializationContext(mapper, bs);
            return bs.CreateResolver(mapper);
        }

        protected virtual void CustomizeXmlSerializationContext(EntityKeyMapper<TItemId> mapper, XmlSerializationContext bs)
        {
            
        }
        
        protected virtual void CustomizeBinarySerializationContext(EntityKeyMapper<TItemId> mapper, BinarySerializationContext bs)
        {
            
        }
    }
}