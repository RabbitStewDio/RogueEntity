using System;
using System.Collections.Generic;
using EnTTSharp.Entities;
using FluentAssertions;
using NUnit.Framework;
using RogueEntity.Api.Modules;
using RogueEntity.Api.Modules.Helpers;
using RogueEntity.Core.Infrastructure.Services;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Meta.EntityKeys;

namespace RogueEntity.Core.Tests.Modules
{
    public class ModuleContext
    {
        public readonly List<(ModuleId, Type)> RegisteredStuff;
        public readonly List<ModuleId> RegisteredContent;

        public ModuleContext()
        {
            RegisteredContent = new List<ModuleId>();
            RegisteredStuff = new List<(ModuleId, Type)>();
        }

        public void RegisterContent(ModuleId module)
        {
            if (RegisteredContent.Contains(module))
            {
                throw new InvalidOperationException("Duplicate content call for module " + module);
            }

            RegisteredContent.Add(module);
        }

        public void RegisterEntity(ModuleId module, Type entity)
        {
            if (RegisteredStuff.Contains((module, entity)))
            {
                throw new InvalidOperationException("Duplicate entity call for " + module + " with " + entity);
            }

            RegisteredStuff.Add((module, entity));
        }
    }

    public class ModuleFixture : ModuleBase
    {
        readonly ModuleContext context;

        public ModuleFixture(ModuleContext context, string id, params ModuleDependency[] deps)
        {
            this.context = context;
            Id = id;
            DeclareDependencies(deps);
        }


        protected void InitializeEntity<TEntity>(IModuleInitializer initializer)
            where TEntity : IEntityKey
        {
            context.RegisterEntity(Id, typeof(TEntity));
        }
    }

    public class ModuleOrderTest
    {
        [Test]
        public void TestLinearModules()
        {
            var context = new ModuleContext();

            var ms = new ModuleSystem(new DefaultServiceResolver());
            ms.AddModule(new ModuleFixture(context, "Base"));
            ms.AddModule(new ModuleFixture(context, "Mid", ModuleDependency.Of("Base")));

            ms.Initialize();

            Console.WriteLine("Order[0]: " + context.RegisteredContent.ExtendToString());
            Console.WriteLine("Order[1]: " + context.RegisteredStuff.ExtendToString());

            context.RegisteredContent.Should().ContainInOrder("Base", "Mid", "Content");
            context.RegisteredStuff.Should().ContainInOrder(("Base", typeof(ItemReference)), ("Mid", typeof(ItemReference)));
        }

        [Test]
        public void TestParallelModules()
        {
            var context = new ModuleContext();

            var ms = new ModuleSystem(new DefaultServiceResolver());
            ms.AddModule(new ModuleFixture(context, "Base"));
            ms.AddModule(new ModuleFixture(context, "Mid-A", ModuleDependency.Of("Base")));
            ms.AddModule(new ModuleFixture(context, "Mid-B", ModuleDependency.Of("Base")));

            ms.Initialize();

            Console.WriteLine("Order[0]: " + context.RegisteredContent.ExtendToString());
            Console.WriteLine("Order[1]: " + context.RegisteredStuff.ExtendToString());

            context.RegisteredContent.Should().ContainInOrder("Base", "Mid-A", "Mid-B", "Content");
            context.RegisteredStuff.Should()
                   .ContainInOrder(
                       ("Base", typeof(EntityKey)),
                       ("Base", typeof(ItemReference)),
                       ("Mid-A", typeof(ItemReference)),
                       ("Mid-B", typeof(EntityKey)));
        }

        [Test]
        public void TestMissingModule()
        {
            var context = new ModuleContext();
            
            var ms = new ModuleSystem(new DefaultServiceResolver());
            ms.AddModule(new ModuleFixture(context, "Base"));
            ms.AddModule(new ModuleFixture(context, "Mid-A", ModuleDependency.Of("Base")));
            ms.AddModule(new ModuleFixture(context, "Content",
                                           ModuleDependency.Of("Mid-A"),
                                           ModuleDependency.Of("Mid-B")));

            ms.Invoking(m => m.Initialize()).Should().ThrowExactly<ModuleInitializationException>();
        }

        [Test]
        public void TestCircularDependencies()
        {
            var context = new ModuleContext();
            
            var ms = new ModuleSystem(new DefaultServiceResolver());
            ms.AddModule(new ModuleFixture(context, "Base", ModuleDependency.Of("Content")));
            ms.AddModule(new ModuleFixture(context, "Mid-A", ModuleDependency.Of("Base")));
            ms.AddModule(new ModuleFixture(context, "Content",
                                           ModuleDependency.Of("Mid-A"),
                                           ModuleDependency.Of("Mid-B")));

            ms.Invoking(m => m.Initialize()).Should().ThrowExactly<ModuleInitializationException>();
        }
    }
}
