﻿using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using Serilog;
using System.Diagnostics.CodeAnalysis;

namespace RogueEntity.Api.Tests
{
    [SetUpFixture]
    [SuppressMessage("ReSharper", "CheckNamespace")]
    public class SetupLoggingFixture
    {
        [OneTimeSetUp]
        public void RunBeforeAnyTests()
        {
            var configuration = new ConfigurationBuilder()
                                .AddJsonFile("appsettings.json")
                                .Build();

            var logger = new LoggerConfiguration()
                         .ReadFrom.Configuration(configuration)
                         .CreateLogger();
            Log.Logger = logger;
        }

        [OneTimeTearDown]
        public void RunAfterAnyTests()
        {
            // ...
        }
    }
}
