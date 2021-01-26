using FluentAssertions;
using NUnit.Framework;
using RogueEntity.Core.Utils;
using RogueEntity.Generator.MapFragments;
using System;

namespace RogueEntity.Generator.Tests.MapFragments
{
    public class MapFragmentParserTest
    {
        public static string TemplateText = @"
Name: Template
Type: MapFragmentTemplate
    
Tags: 
    - Template

Properties:
    TemplateOnly: TemplateValue
    TemplateOverride: TemplateOverrideValue

Symbols:
    ' ': []
    .: [ground]
    c: [ground, item.corpse.rat]
";

        public static string FragmentText = @"
Name: Template
Template: TemplateText
Type: MapFragment
Size: 
    Width: 3
    Height: 1
    
Tags: 
    - Fragment

Properties:
    FragmentOnly: FragmentValue
    TemplateOverride: FragmentOverrides

Symbols:
    ' ': [empty]
    .: [ground, poison]
---
. c
";

        [Test]
        public void ParseFragmentWithInclude()
        {
            var parser = new TestParser();
            parser.TryParseFromFile(nameof(FragmentText), out var result).Should().BeTrue();

            result.Info.Name.Should().Be("Template");
            result.Info.Type.Should().Be("MapFragment");
            result.Size.Should().Be(new Dimension(3, 1));

            result.Info.Tags.Should().BeEquivalentTo("Template", "Fragment");
            result.MapData[0, 0].Should().Be(new MapFragmentTagDeclaration("ground", "poison"));
            result.MapData[1, 0].Should().Be(new MapFragmentTagDeclaration("empty"));
            result.MapData[2, 0].Should().Be(new MapFragmentTagDeclaration("ground", "item.corpse.rat"));
        }

        class TestParser : MapFragmentParser
        {
            protected override string Resolve(string context, string file)
            {
                context.Should().Be(nameof(FragmentText));
                file.Should().Be(nameof(TemplateText));
                return file;
            }

            protected override string ReadAllText(string fileName)
            {
                switch (fileName)
                {
                    case nameof(FragmentText): return FragmentText;
                    case nameof(TemplateText): return TemplateText;
                    default: throw new ArgumentException();
                }
            }
        }
    }
}
