using FluentAssertions;
using NUnit.Framework;
using RogueEntity.Api.Utils;
using RogueEntity.Core.GridProcessing.Directionality;
using RogueEntity.Core.Movement;
using RogueEntity.Core.Movement.CostModifier.Directions;
using RogueEntity.Core.Movement.MovementModes;
using RogueEntity.Core.Movement.MovementModes.Swimming;
using RogueEntity.Core.Movement.MovementModes.Walking;
using RogueEntity.Core.MovementPlaning.Pathfinding.Hierarchical;
using RogueEntity.Core.Positioning.Algorithms;
using RogueEntity.Core.Tests.Fixtures;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.DataViews;
using Serilog;
using System;
using System.Collections.Generic;
using Assert = NUnit.Framework.Assert;

namespace RogueEntity.Core.Tests.Movement.Pathfinding;

public class HierarchicalPathfinderTest
{
    const string EmptyRoom = @"
// 9x9; an empty room
 ### , ### , ### , ### , ### , ### , ### , ### , ###  
 ### ,  '  ,  '  ,  '  ,  .  ,  .  ,  .  ,  .  , ###
 ### ,  '  ,  '  ,  '  ,  .  ,  .  ,  .  ,  .  , ###
 ### ,  '  ,  '  ,  _  ,  .  ,  .  ,  .  ,  .  , ###
 ### ,  .  ,  _  ,  _  ,  _  ,  _  ,  .  ,  .  , ###
 ### ,  .  ,  _  ,  .  ,  .  ,  _  ,  .  ,  .  , ###
 ### ,  .  ,  _  ,  .  ,  .  ,  _  ,  .  ,  .  , ###
 ### ,  .  ,  _  ,  _  ,  _  ,  _  ,  .  ,  .  , ###
 ### ,  .  ,  .  ,  .  ,  .  ,  .  ,  .  ,  .  , ###
 ### , ### , ### , ### , ### , ### , ### , ### , ###  
";

    const string EmptyRoomResult = @"
... ... ... ... ... ... ... ... ... ... ... ... ... ... ... ... 
._. .a. .b. .c. .d. .e. .f. .g. ._. ._. ._. ._. ._. ._. ._. ._. 
... ... ... ... ... ... ... ... ... ... ... ... ... ... ... ... 
... ... ... ... ... ... ... ... ... ... ... ... ... ... ... ... 
.h. .i- -i- -i. .j- -j- -j- -j. .k. ._. ._. ._. ._. ._. ._. ._. 
... .|. ... .|. .|. ... ... .|. ... ... ... ... ... ... ... ... 
... .|. ... .|. .|. ... ... .|. ... ... ... ... ... ... ... ... 
.l. .i. .i. .i. .j. .j. .j. .j. .m. ._. ._. ._. ._. ._. ._. ._. 
... .|. ... /.. .|. ... ... .|. ... ... ... ... ... ... ... ... 
... .|. ../ ... .|. ... ... .|. ... ... ... ... ... ... ... ... 
.n. .i- -i. .o. .j- -j. .j. .j. .p. ._. ._. ._. ._. ._. ._. ._. 
... ... ... ... ... ..\ ... .|. ... ... ... ... ... ... ... ... 
... ... ... ... ... ... \.. .|. ... ... ... ... ... ... ... ... 
.q. .j. .r. .s. .t. .u. .j. .j. .v. ._. ._. ._. ._. ._. ._. ._. 
... .|. ... ... ... ... .|. .|. ... ... ... ... ... ... ... ... 
... .|. ... ... ... ... .|. .|. ... ... ... ... ... ... ... ... 
.w. .j. .x. .y- -y. .@. .j. .j. .A. ._. ._. ._. ._. ._. ._. ._. 
... .|. ... .|. .|. ... .|. .|. ... ... ... ... ... ... ... ... 
... .|. ... .|. .|. ... .|. .|. ... ... ... ... ... ... ... ... 
.B. .j. .C. .y- -y. .D. .j. .j. .E. ._. ._. ._. ._. ._. ._. ._. 
... .|. ... ... ... ... .|. .|. ... ... ... ... ... ... ... ... 
... .|. ... ... ... ... .|. .|. ... ... ... ... ... ... ... ... 
.F. .j. .G. .H. .I. .J. .j. .j. .K. ._. ._. ._. ._. ._. ._. ._. 
... .|\ ... ... ... ... /.. .|. ... ... ... ... ... ... ... ... 
... .|. \.. ... ... ../ ... .|. ... ... ... ... ... ... ... ... 
.L. .j- -j- -j- -j- -j- -j- -j. .M. ._. ._. ._. ._. ._. ._. ._. 
... ... ... ... ... ... ... ... ... ... ... ... ... ... ... ... 
... ... ... ... ... ... ... ... ... ... ... ... ... ... ... ... 
._. .N. .O. .P. .Q. .R. .S. .T. ._. ._. ._. ._. ._. ._. ._. ._. 
... ... ... ... ... ... ... ... ... ... ... ... ... ... ... ... 
... ... ... ... ... ... ... ... ... ... ... ... ... ... ... ... 
._. ._. ._. ._. ._. ._. ._. ._. ._. ._. ._. ._. ._. ._. ._. ._. 
... ... ... ... ... ... ... ... ... ... ... ... ... ... ... ... 
... ... ... ... ... ... ... ... ... ... ... ... ... ... ... ... 
._. ._. ._. ._. ._. ._. ._. ._. ._. ._. ._. ._. ._. ._. ._. ._. 
... ... ... ... ... ... ... ... ... ... ... ... ... ... ... ... 
... ... ... ... ... ... ... ... ... ... ... ... ... ... ... ... 
._. ._. ._. ._. ._. ._. ._. ._. ._. ._. ._. ._. ._. ._. ._. ._. 
... ... ... ... ... ... ... ... ... ... ... ... ... ... ... ... 
... ... ... ... ... ... ... ... ... ... ... ... ... ... ... ... 
._. ._. ._. ._. ._. ._. ._. ._. ._. ._. ._. ._. ._. ._. ._. ._. 
... ... ... ... ... ... ... ... ... ... ... ... ... ... ... ... 
... ... ... ... ... ... ... ... ... ... ... ... ... ... ... ... 
._. ._. ._. ._. ._. ._. ._. ._. ._. ._. ._. ._. ._. ._. ._. ._. 
... ... ... ... ... ... ... ... ... ... ... ... ... ... ... ... 
... ... ... ... ... ... ... ... ... ... ... ... ... ... ... ... 
._. ._. ._. ._. ._. ._. ._. ._. ._. ._. ._. ._. ._. ._. ._. ._. 
... ... ... ... ... ... ... ... ... ... ... ... ... ... ... ...
";

    const string SimpleRoom = @"
 // 9x9; an empty room
 ### , ### , ### , ### , ### , ### , ### , ### , ###  
 ### ,  .  ,  .  ,  .  ,  .  ,  .  ,  .  ,  .  , ###
 ### ,  .  ,  .  ,  .  ,  .  ,  .  ,  .  ,  .  , ###
 ### ,  .  ,  .  ,  .  , ### ,  .  ,  .  ,  .  , ###
 ### ,  .  ,  .  , ### ,  .  ,  .  ,  .  ,  .  , ###
 ### ,  .  ,  .  ,  .  ,  .  ,  .  ,  .  ,  .  , ###
 ### ,  .  ,  .  ,  .  ,  .  ,  .  ,  .  ,  .  , ###
 ### ,  .  ,  .  ,  .  ,  .  ,  .  ,  .  ,  .  , ###
 ### ,  .  ,  .  ,  .  ,  .  ,  .  ,  .  ,  .  , ###
 ### , ### , ### , ### , ### , ### , ### , ### , ###  
";

    const string SimpleRoomResult = @"
... ... ... ... ... ... ... ... ... ... ... ... ... ... ... ... 
._. .a. .b. .c. .d. .e. .f. .g. ._. ._. ._. ._. ._. ._. ._. ._. 
... ... ... ... ... ... ... ... ... ... ... ... ... ... ... ... 
... ... ... ... ... ... ... ... ... ... ... ... ... ... ... ... 
.h. .i- -i- -i- -i- -i- -i- -i. .j. ._. ._. ._. ._. ._. ._. ._. 
... .|. ... ... ... ... ... .|. ... ... ... ... ... ... ... ... 
... .|. ... ... ... ... ... .|. ... ... ... ... ... ... ... ... 
.k. .i. .i. .i. .i. .i. .i. .i. .l. ._. ._. ._. ._. ._. ._. ._. 
... .|. ... ... /.\ ... ... .|. ... ... ... ... ... ... ... ... 
... .|. ... ../ ... \.. ... .|. ... ... ... ... ... ... ... ... 
.m. .i. .i. .i. .n. .i. .i. .i. .o. ._. ._. ._. ._. ._. ._. ._. 
... .|. ... /.. ... /.. ... .|. ... ... ... ... ... ... ... ... 
... .|. ../ ... ../ ... ... .|. ... ... ... ... ... ... ... ... 
.p. .i. .i. .q. .i. .i. .i. .i. .r. ._. ._. ._. ._. ._. ._. ._. 
... .|. ..\ ... /.. ... ... .|. ... ... ... ... ... ... ... ... 
... .|. ... \./ ... ... ... .|. ... ... ... ... ... ... ... ... 
.s. .i. .i. .i. .i. .i. .i. .i. .t. ._. ._. ._. ._. ._. ._. ._. 
... .|. ... ... ... ... ... .|. ... ... ... ... ... ... ... ... 
... .|. ... ... ... ... ... .|. ... ... ... ... ... ... ... ... 
.u. .i. .i. .i. .i. .i. .i. .i. .v. ._. ._. ._. ._. ._. ._. ._. 
... .|. ... ... ... ... ... .|. ... ... ... ... ... ... ... ... 
... .|. ... ... ... ... ... .|. ... ... ... ... ... ... ... ... 
.w. .i. .i. .i. .i. .i. .i. .i. .x. ._. ._. ._. ._. ._. ._. ._. 
... .|. ... ... ... ... ... .|. ... ... ... ... ... ... ... ... 
... .|. ... ... ... ... ... .|. ... ... ... ... ... ... ... ... 
.y. .i- -i- -i- -i- -i- -i- -i. .@. ._. ._. ._. ._. ._. ._. ._. 
... ... ... ... ... ... ... ... ... ... ... ... ... ... ... ... 
... ... ... ... ... ... ... ... ... ... ... ... ... ... ... ... 
._. .A. .B. .C. .D. .E. .F. .G. ._. ._. ._. ._. ._. ._. ._. ._. 
... ... ... ... ... ... ... ... ... ... ... ... ... ... ... ... 
... ... ... ... ... ... ... ... ... ... ... ... ... ... ... ... 
._. ._. ._. ._. ._. ._. ._. ._. ._. ._. ._. ._. ._. ._. ._. ._. 
... ... ... ... ... ... ... ... ... ... ... ... ... ... ... ... 
... ... ... ... ... ... ... ... ... ... ... ... ... ... ... ... 
._. ._. ._. ._. ._. ._. ._. ._. ._. ._. ._. ._. ._. ._. ._. ._. 
... ... ... ... ... ... ... ... ... ... ... ... ... ... ... ... 
... ... ... ... ... ... ... ... ... ... ... ... ... ... ... ... 
._. ._. ._. ._. ._. ._. ._. ._. ._. ._. ._. ._. ._. ._. ._. ._. 
... ... ... ... ... ... ... ... ... ... ... ... ... ... ... ... 
... ... ... ... ... ... ... ... ... ... ... ... ... ... ... ... 
._. ._. ._. ._. ._. ._. ._. ._. ._. ._. ._. ._. ._. ._. ._. ._. 
... ... ... ... ... ... ... ... ... ... ... ... ... ... ... ... 
... ... ... ... ... ... ... ... ... ... ... ... ... ... ... ... 
._. ._. ._. ._. ._. ._. ._. ._. ._. ._. ._. ._. ._. ._. ._. ._. 
... ... ... ... ... ... ... ... ... ... ... ... ... ... ... ... 
... ... ... ... ... ... ... ... ... ... ... ... ... ... ... ... 
._. ._. ._. ._. ._. ._. ._. ._. ._. ._. ._. ._. ._. ._. ._. ._. 
... ... ... ... ... ... ... ... ... ... ... ... ... ... ... ...
";

    const string DonutRoom = @"
 // 9x9; an empty room
 ### , ### , ### , ### , ### , ### , ###  
 ### ,  .  ,  .  ,  .  ,  .  ,  .  , ###
 ### ,  .  , ### , ### , ### ,  .  , ###
 ### ,  .  , ### ,  .  , ### ,  .  , ###
 ### ,  .  , ### , ### , ### ,  .  , ###
 ### ,  .  ,  .  ,  .  ,  .  ,  .  , ###
 ### , ### , ### , ### , ### , ### , ###  
";

    const string DonutRoomResult = @"
... ... ... ... ... ... ... ... ... ... ... ... ... ... ... ... 
._. .a. .b. .c. .d. .e. ._. ._. ._. ._. ._. ._. ._. ._. ._. ._. 
... ... ... ... ... ... ... ... ... ... ... ... ... ... ... ... 
... ... ... ... ... ... ... ... ... ... ... ... ... ... ... ... 
.f. .g- -g- -g- -g- -g. .h. ._. ._. ._. ._. ._. ._. ._. ._. ._. 
... .|. /.. ... ..\ .|. ... ... ... ... ... ... ... ... ... ... 
... .|/ ... ... ... \|. ... ... ... ... ... ... ... ... ... ... 
.i. .g. .j. .k. .l. .g. .m. ._. ._. ._. ._. ._. ._. ._. ._. ._. 
... .|. ... ... ... .|. ... ... ... ... ... ... ... ... ... ... 
... .|. ... ... ... .|. ... ... ... ... ... ... ... ... ... ... 
.n. .g. .o. .p. .q. .g. .r. ._. ._. ._. ._. ._. ._. ._. ._. ._. 
... .|. ... ... ... .|. ... ... ... ... ... ... ... ... ... ... 
... .|. ... ... ... .|. ... ... ... ... ... ... ... ... ... ... 
.s. .g. .t. .u. .v. .g. .w. ._. ._. ._. ._. ._. ._. ._. ._. ._. 
... .|\ ... ... ... /|. ... ... ... ... ... ... ... ... ... ... 
... .|. \.. ... ../ .|. ... ... ... ... ... ... ... ... ... ... 
.x. .g- -g- -g- -g- -g. .y. ._. ._. ._. ._. ._. ._. ._. ._. ._. 
... ... ... ... ... ... ... ... ... ... ... ... ... ... ... ... 
... ... ... ... ... ... ... ... ... ... ... ... ... ... ... ... 
._. .@. .A. .B. .C. .D. ._. ._. ._. ._. ._. ._. ._. ._. ._. ._. 
... ... ... ... ... ... ... ... ... ... ... ... ... ... ... ... 
... ... ... ... ... ... ... ... ... ... ... ... ... ... ... ... 
._. ._. ._. ._. ._. ._. ._. ._. ._. ._. ._. ._. ._. ._. ._. ._. 
... ... ... ... ... ... ... ... ... ... ... ... ... ... ... ... 
... ... ... ... ... ... ... ... ... ... ... ... ... ... ... ... 
._. ._. ._. ._. ._. ._. ._. ._. ._. ._. ._. ._. ._. ._. ._. ._. 
... ... ... ... ... ... ... ... ... ... ... ... ... ... ... ... 
... ... ... ... ... ... ... ... ... ... ... ... ... ... ... ... 
._. ._. ._. ._. ._. ._. ._. ._. ._. ._. ._. ._. ._. ._. ._. ._. 
... ... ... ... ... ... ... ... ... ... ... ... ... ... ... ... 
... ... ... ... ... ... ... ... ... ... ... ... ... ... ... ... 
._. ._. ._. ._. ._. ._. ._. ._. ._. ._. ._. ._. ._. ._. ._. ._. 
... ... ... ... ... ... ... ... ... ... ... ... ... ... ... ... 
... ... ... ... ... ... ... ... ... ... ... ... ... ... ... ... 
._. ._. ._. ._. ._. ._. ._. ._. ._. ._. ._. ._. ._. ._. ._. ._. 
... ... ... ... ... ... ... ... ... ... ... ... ... ... ... ... 
... ... ... ... ... ... ... ... ... ... ... ... ... ... ... ... 
._. ._. ._. ._. ._. ._. ._. ._. ._. ._. ._. ._. ._. ._. ._. ._. 
... ... ... ... ... ... ... ... ... ... ... ... ... ... ... ... 
... ... ... ... ... ... ... ... ... ... ... ... ... ... ... ... 
._. ._. ._. ._. ._. ._. ._. ._. ._. ._. ._. ._. ._. ._. ._. ._. 
... ... ... ... ... ... ... ... ... ... ... ... ... ... ... ... 
... ... ... ... ... ... ... ... ... ... ... ... ... ... ... ... 
._. ._. ._. ._. ._. ._. ._. ._. ._. ._. ._. ._. ._. ._. ._. ._. 
... ... ... ... ... ... ... ... ... ... ... ... ... ... ... ... 
... ... ... ... ... ... ... ... ... ... ... ... ... ... ... ... 
._. ._. ._. ._. ._. ._. ._. ._. ._. ._. ._. ._. ._. ._. ._. ._. 
... ... ... ... ... ... ... ... ... ... ... ... ... ... ... ...
";

    readonly ILogger logger = SLog.ForContext<SingleLevelPathFinderTest>();

    readonly Dictionary<string, (string sourceText, string resultText)> testCases = new Dictionary<string, (string sourceText, string resultText)>()
    {
        { nameof(EmptyRoom), (EmptyRoom, EmptyRoomResult) },
        { nameof(SimpleRoom), (SimpleRoom, SimpleRoomResult) },
        { nameof(DonutRoom), (DonutRoom, DonutRoomResult) },
    };

    static DynamicDataView2D<float> ParseMap(string text, out Rectangle parsedBounds, Func<(float walkingCost, float swimmingCost), float> trx)
    {
        var tokenParser = new TokenParser();
        tokenParser.AddToken("", (1f, 1f));
        tokenParser.AddToken(".", (1f, 1f));
        tokenParser.AddToken("_", (0f, 1f));
        tokenParser.AddToken("'", (1f, 0f));
        tokenParser.AddToken("###", (0f, 0f));
        tokenParser.AddToken("##", (0f, 0f));
        tokenParser.AddToken("#", (0f, 0f));

        return TestHelpers.Parse(text, tokenParser, out parsedBounds, trx);
    }

    MovementSourceData ParseMovementData<TMovementType>(TMovementType m,
                                                        string sourceText,
                                                        Func<(float, float), float> trx,
                                                        out Rectangle parsedBounds)
        where TMovementType : IMovementMode
    {
        var resistanceMap = ParseMap(sourceText, out parsedBounds, trx);

        var outboundDirectionalityMapSystem = new OutboundMovementDirectionalitySystem<WalkingMovement>(resistanceMap.As3DMap(0));
        outboundDirectionalityMapSystem.MarkGloballyDirty();
        outboundDirectionalityMapSystem.Process();
        outboundDirectionalityMapSystem.ResultView.TryGetView(0, out var outboundDirectionalityMap).Should().BeTrue();

        var inboundDirectionalityMapSystem = new InboundMovementDirectionalitySystem<WalkingMovement>(resistanceMap.As3DMap(0));
        inboundDirectionalityMapSystem.MarkGloballyDirty();
        inboundDirectionalityMapSystem.Process();
        inboundDirectionalityMapSystem.ResultView.TryGetView(0, out var inboundDirectionalityMap).Should().BeTrue();

        Assert.NotNull(inboundDirectionalityMap);
        Assert.NotNull(outboundDirectionalityMap);

        logger.Debug("Movement mode: {Movement}\n{OutBound}", m,
                     TestHelpers.PrintMap(resistanceMap, parsedBounds));
        logger.Debug("Inbound\n{InBoundDirections}", inboundDirectionalityMap.PrintEdges(parsedBounds));
        logger.Debug("In: {In} Out: {Out}", inboundDirectionalityMap.At(3, 1), outboundDirectionalityMap.At(3, 1));

        return new MovementSourceData(m, resistanceMap.As3DMap(0), inboundDirectionalityMap.As3DMap(0), outboundDirectionalityMap.As3DMap(0));
    }


    [Test]
    [TestCase(nameof(EmptyRoom))]
    [TestCase(nameof(SimpleRoom))]
    [TestCase(nameof(DonutRoom))]
    public void ValidateZonePainting(string id)
    {
        var (sourceText, resultText) = testCases[id];

        logger.Debug("Using room layout \n{Map}", sourceText);


        var ms = new MovementDataCollector();
        ms.RegisterMovementSource(ParseMovementData(WalkingMovement.Instance, sourceText, x => x.Item1, out var parsedBounds));
        ms.RegisterMovementSource(ParseMovementData(SwimmingMovement.Instance, sourceText, x => x.Item2, out _));

        var mr = new MovementModeRegistry();
        mr.Register(WalkingMovement.Instance);
        mr.Register(SwimmingMovement.Instance);

        var mapConfig = new DynamicDataViewConfiguration(0, 0, 16, 16);

        var sys = new HierarchicalPathfindingSystem<WalkingMovement>(mapConfig, ms, DistanceCalculation.Euclid);
        sys.Initialize();
        sys.CollectDirtyRegions();
        sys.ProcessDirtyRegionTasks();

        var result = sys.ZoneInformation;
        result.TryGetView(0, out var zone2D).Should().BeTrue();
        zone2D!.TryGetData(0, 0, out var maybeZone).Should().BeTrue();
        var zoneView = maybeZone as PathfinderRegionDataView;

        Assert.NotNull(zoneView);

        var producedResultText = zoneView.PrintEdges();
        Console.WriteLine("Edges:\n" + producedResultText);
        producedResultText.NormalizeMultilineText().Should().Be(resultText.NormalizeMultilineText());
    }

    [Test]
    [TestCase(nameof(EmptyRoom))]
    [TestCase(nameof(SimpleRoom))]
    [TestCase(nameof(DonutRoom))]
    public void ValidateEdgeDetection(string id)
    {
        var (sourceText, resultText) = testCases[id];

        logger.Debug("Using room layout \n{Map}", sourceText);


        var ms = new MovementDataCollector();
        ms.RegisterMovementSource(ParseMovementData(WalkingMovement.Instance, sourceText, x => x.Item1, out var parsedBounds));
        ms.RegisterMovementSource(ParseMovementData(SwimmingMovement.Instance, sourceText, x => x.Item2, out _));

        var mr = new MovementModeRegistry();
        mr.Register(WalkingMovement.Instance);
        mr.Register(SwimmingMovement.Instance);

        var mapConfig = new DynamicDataViewConfiguration(0, 0, 16, 16);

        var sys = new HierarchicalPathfindingSystem<WalkingMovement>(mapConfig, ms, DistanceCalculation.Euclid);
        sys.Initialize();
        sys.CollectDirtyRegions();
        sys.PaintDirtyPathfinderRegionsPrimitive();

        var result = sys.ZoneInformation;
        result.TryGetView(0, out var zone2D).Should().BeTrue();
        zone2D!.TryGetData(0, 0, out var maybeZone).Should().BeTrue();
        var zoneView = maybeZone as PathfinderRegionDataView;

        Assert.NotNull(zoneView);

        Console.WriteLine("Collected Edges");
        var contents = zoneView.Contents();
        foreach (var c in contents)
        {
            Console.WriteLine(c.zone + " -> ");
            foreach (var x in c.edges)
            {
                Console.WriteLine("   " + x);
            }
        }

        var producedResultText = zoneView.PrintEdges();
        Console.WriteLine("Edges:\n" + producedResultText);
        producedResultText.NormalizeMultilineText().Should().Be(resultText.NormalizeMultilineText());
    }
}