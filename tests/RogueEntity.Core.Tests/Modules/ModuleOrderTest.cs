﻿using System;
using System.Collections.Generic;
using EnTTSharp.Entities;
using FluentAssertions;
using NUnit.Framework;
using RogueEntity.Core.Infrastructure.Modules;
using RogueEntity.Core.Infrastructure.Modules.Helpers;
using RogueEntity.Core.Infrastructure.Services;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Utils;

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
        public ModuleFixture(string id, params ModuleDependency[] deps)
        {
            Id = id;
            DeclareDependencies(deps);
        }


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
            var ms = new ModuleSystem<ModuleContext>(new DefaultServiceResolver());
            ms.AddModule(new ModuleFixture("Base"));
            ms.AddModule(new ModuleFixture("Mid", ModuleDependency.Of("Base")));

            var context = new ModuleContext();
            ms.Initialize(context);

            Console.WriteLine("Order[0]: " + context.RegisteredContent.ExtendToString());
            Console.WriteLine("Order[1]: " + context.RegisteredStuff.ExtendToString());

            context.RegisteredContent.Should().ContainInOrder("Base", "Mid", "Content");
            context.RegisteredStuff.Should().ContainInOrder(("Base", typeof(ItemReference)), ("Mid", typeof(ItemReference)));
        }

        [Test]
        public void TestParallelModules()
        {
            var ms = new ModuleSystem<ModuleContext>(new DefaultServiceResolver());
            ms.AddModule(new ModuleFixture("Base"));
            ms.AddModule(new ModuleFixture("Mid-A", ModuleDependency.Of("Base")));
            ms.AddModule(new ModuleFixture("Mid-B", ModuleDependency.Of("Base")));

            var context = new ModuleContext();
            ms.Initialize(context);
            
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
            var ms = new ModuleSystem<ModuleContext>(new DefaultServiceResolver());
            ms.AddModule(new ModuleFixture("Base"));
            ms.AddModule(new ModuleFixture("Mid-A", ModuleDependency.Of("Base")));
            ms.AddModule(new ModuleFixture("Content", 
                                           ModuleDependency.Of("Mid-A"), 
                                           ModuleDependency.Of("Mid-B")));

            var context = new ModuleContext();
            ms.Invoking(m => m.Initialize(context)).Should().ThrowExactly<ModuleInitializationException>();
        }

        [Test]
        public void TestCircularDependencies()
        {
            var ms = new ModuleSystem<ModuleContext>(new DefaultServiceResolver());
            ms.AddModule(new ModuleFixture("Base", ModuleDependency.Of("Content")));
            ms.AddModule(new ModuleFixture("Mid-A", ModuleDependency.Of("Base")));
            ms.AddModule(new ModuleFixture("Content", 
                                           ModuleDependency.Of("Mid-A"), 
                                           ModuleDependency.Of("Mid-B")));

            var context = new ModuleContext();
            ms.Invoking(m => m.Initialize(context)).Should().ThrowExactly<ModuleInitializationException>();
        }

    }
}