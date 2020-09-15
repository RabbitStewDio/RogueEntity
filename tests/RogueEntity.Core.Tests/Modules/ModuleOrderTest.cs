using System;
using System.Collections.Generic;
using EnTTSharp.Entities;
using FluentAssertions;
using GoRogue;
using NUnit.Framework;
using RogueEntity.Core.Infrastructure.Modules;
using RogueEntity.Core.Meta.Items;

namespace RogueEntity.Core.Tests.Modules
{
    public class ModuleContext
    {
        public readonly List<(string, Type)> RegisteredStuff;
        public readonly List<string> RegisteredContent;

        public ModuleContext()
        {
            RegisteredContent = new List<string>();
            RegisteredStuff = new List<(string, Type)>();
        }

        public void RegisterContent(string module)
        {
            if (RegisteredContent.Contains(module))
            {
                throw new InvalidOperationException("Duplicate content call for module " + module);
            }

            RegisteredContent.Add(module);
        }

        public void RegisterEntity(string module, Type entity)
        {
            if (RegisteredStuff.Contains((module, entity)))
            {
                throw new InvalidOperationException("Duplicate entity call for " + module + " with " + entity);
            }

            RegisteredStuff.Add((module, entity));
        }
    }

    public class ModuleFixture : ModuleBase<ModuleContext>
    {
        public ModuleFixture(string id, params ModuleDependency[] deps)
        {
            Id = id;
            DeclareDependencies(deps);
        }


        public override void InitializeContent(ModuleContext context, IModuleInitializer<ModuleContext> initializer)
        {
            context.RegisterContent(Id);
        }

        [ModuleEntityInitializer]
        protected void InitializeEntity<TEntity>(ModuleContext context, IModuleInitializer<ModuleContext> initializer) where TEntity: IEntityKey
        {
            context.RegisterEntity(Id, typeof(TEntity));
        }
    }

    public class ModuleOrderTest
    {
        [Test]
        public void TestLinearModules()
        {
            var ms = new ModuleSystem<ModuleContext>();
            ms.AddModule(new ModuleFixture("Base"));
            ms.AddModule(new ModuleFixture("Mid", ModuleDependency.OfFrameworkEntity("Base")));
            ms.AddModule(new ModuleFixture("Content", ModuleDependency.OfEntity<ItemReference>("Mid")));

            var context = new ModuleContext();
            ms.Initialize(context, new ModuleInitializer<ModuleContext>());

            Console.WriteLine("Order[0]: " + context.RegisteredContent.ExtendToString());
            Console.WriteLine("Order[1]: " + context.RegisteredStuff.ExtendToString());

            context.RegisteredContent.Should().ContainInOrder("Base", "Mid", "Content");
            context.RegisteredStuff.Should().ContainInOrder(("Base", typeof(ItemReference)), ("Mid", typeof(ItemReference)));
        }

        [Test]
        public void TestParallelModules()
        {
            var ms = new ModuleSystem<ModuleContext>();
            ms.AddModule(new ModuleFixture("Base"));
            ms.AddModule(new ModuleFixture("Mid-A", ModuleDependency.OfFrameworkEntity("Base")));
            ms.AddModule(new ModuleFixture("Mid-B", ModuleDependency.OfFrameworkEntity("Base")));
            ms.AddModule(new ModuleFixture("Content", 
                                           ModuleDependency.OfEntity<ItemReference>("Mid-A"), 
                                           ModuleDependency.OfEntity<EntityKey>("Mid-B")));

            var context = new ModuleContext();
            ms.Initialize(context, new ModuleInitializer<ModuleContext>());
            
            Console.WriteLine("Order[0]: " + context.RegisteredContent.ExtendToString());
            Console.WriteLine("Order[1]: " + context.RegisteredStuff.ExtendToString());

            context.RegisteredContent.Should().ContainInOrder("Base", "Mid-A", "Mid-B", "Content");
            context.RegisteredStuff.Should().ContainInOrder(
                ("Base", typeof(EntityKey)),
                ("Base", typeof(ItemReference)), 
                ("Mid-A", typeof(ItemReference)),
                ("Mid-B", typeof(EntityKey)));
        }

        [Test]
        public void TestMissingModule()
        {
            var ms = new ModuleSystem<ModuleContext>();
            ms.AddModule(new ModuleFixture("Base"));
            ms.AddModule(new ModuleFixture("Mid-A", ModuleDependency.OfFrameworkEntity("Base")));
            ms.AddModule(new ModuleFixture("Content", 
                                           ModuleDependency.OfEntity<ItemReference>("Mid-A"), 
                                           ModuleDependency.OfEntity<EntityKey>("Mid-B")));

            var context = new ModuleContext();
            ms.Invoking(m => m.Initialize(context, new ModuleInitializer<ModuleContext>())).Should().ThrowExactly<ModuleInitializationException>();
        }

        [Test]
        public void TestCircularDependencies()
        {
            var ms = new ModuleSystem<ModuleContext>();
            ms.AddModule(new ModuleFixture("Base", ModuleDependency.OfContent("Content")));
            ms.AddModule(new ModuleFixture("Mid-A", ModuleDependency.OfFrameworkEntity("Base")));
            ms.AddModule(new ModuleFixture("Content", 
                                           ModuleDependency.OfEntity<ItemReference>("Mid-A"), 
                                           ModuleDependency.OfEntity<EntityKey>("Mid-B")));

            var context = new ModuleContext();
            ms.Invoking(m => m.Initialize(context, new ModuleInitializer<ModuleContext>())).Should().ThrowExactly<ModuleInitializationException>();
        }

    }
}