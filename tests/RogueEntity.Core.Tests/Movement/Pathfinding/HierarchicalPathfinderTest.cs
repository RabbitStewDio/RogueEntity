﻿using FluentAssertions;
using NUnit.Framework;
using RogueEntity.Api.Utils;
using RogueEntity.Core.GridProcessing.Directionality;
using RogueEntity.Core.Movement;
using RogueEntity.Core.Movement.Cost;
using RogueEntity.Core.Movement.CostModifier.Directions;
using RogueEntity.Core.Movement.MovementModes;
using RogueEntity.Core.Movement.MovementModes.Ethereal;
using RogueEntity.Core.Movement.MovementModes.Swimming;
using RogueEntity.Core.Movement.MovementModes.Walking;
using RogueEntity.Core.MovementPlaning.Pathfinding;
using RogueEntity.Core.MovementPlaning.Pathfinding.Hierarchical;
using RogueEntity.Core.MovementPlaning.Pathfinding.Hierarchical.Data;
using RogueEntity.Core.MovementPlaning.Pathfinding.Hierarchical.Systems;
using RogueEntity.Core.MovementPlaning.Pathfinding.SingleLevel;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.Algorithms;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Tests.Fixtures;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.DataViews;
using Serilog;
using System;
using System.Collections.Generic;
using Assert = NUnit.Framework.Assert;
using static RogueEntity.Core.Tests.Movement.PathfindingTestUtil;

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

    const string EmptyRoomPathFindingResult = @"
 ### , ### , ### , ### , ### , ### , ### , ### , ### 
 ### ,  @  ,  .  ,  .  ,  .  ,  .  ,  .  ,  .  , ### 
 ### ,  .  ,   1 ,  2  ,  .  ,  .  ,  .  ,  .  , ### 
 ### ,  .  ,  .  , ### ,  3  ,  4  ,  .  ,  .  , ### 
 ### ,  .  , ### , ### , ### , ### ,  5  ,  .  , ### 
 ### ,  .  , ### ,  .  ,  .  , ### ,  .  ,  6  , ### 
 ### ,  .  , ### ,  .  ,  .  , ### ,  .  ,  7  , ### 
 ### ,  .  , ### , ### , ### , ### ,  .  ,  8  , ### 
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

    const string DiagonalRoom = @"
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

    const string DiagonalRoomPathFindingResult = @"
 ### , ### , ### , ### , ### , ### , ### , ### , ### 
 ### ,  @  ,  .  ,  .  ,  .  ,  .  ,  .  ,  .  , ### 
 ### ,  .  ,  1  ,  2  ,  3  ,  .  ,  .  ,  .  , ### 
 ### ,  .  ,  .  ,  .  , ### ,  4  ,  .  ,  .  , ### 
 ### ,  .  ,  .  , ### ,  .  ,  .  ,  5  ,  .  , ### 
 ### ,  .  ,  .  ,  .  ,  .  ,  .  ,  .  ,  6  , ### 
 ### ,  .  ,  .  ,  .  ,  .  ,  .  ,  .  ,  7  , ### 
 ### ,  .  ,  .  ,  .  ,  .  ,  .  ,  .  ,  8  , ### 
 ### ,  .  ,  .  ,  .  ,  .  ,  .  ,  .  ,  .  , ### 
 ### , ### , ### , ### , ### , ### , ### , ### , ### 
";
    const string DiagonalRoomResult = @"
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

    const string DonutRoomPathFindingResult = @"
 // 9x9; an empty room
 ### , ### , ### , ### , ### , ### , ###  
 ### ,  @  ,  1  ,  2  ,  3  ,  .  , ###
 ### ,  .  , ### , ### , ### ,  4  , ###
 ### ,  .  , ### ,  .  , ### ,  5  , ###
 ### ,  .  , ### , ### , ### ,  6  , ###
 ### ,  .  ,  .  ,  .  ,  .  ,  7  , ###
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

    readonly record struct TestParams(string sourceText, 
                                      string resultText, 
                                      string pathFindingText, 
                                      Position2D sourcePos, 
                                      Position2D targetPos);
    
    readonly ILogger logger = SLog.ForContext<SingleLevelPathFinderTest>();

    readonly Dictionary<string, TestParams> testCases = new Dictionary<string, TestParams>()
    {
        { nameof(EmptyRoom), new TestParams(EmptyRoom, EmptyRoomResult ,EmptyRoomPathFindingResult,  new Position2D(1, 1), new Position2D(7, 7)) },
        { nameof(DiagonalRoom), new TestParams(DiagonalRoom, DiagonalRoomResult , DiagonalRoomPathFindingResult, new Position2D(1, 1), new Position2D(7, 7)) },
        { nameof(DonutRoom), new TestParams(DonutRoom, DonutRoomResult , DonutRoomPathFindingResult, new Position2D(1, 1), new Position2D(5, 5)) },
    };

    static DynamicDataView2D<float> ParseMap(string text, out Rectangle parsedBounds, Func<(float walkingCost, float swimmingCost, float etheralCost), float> trx)
    {
        var tokenParser = new TokenParser();
        tokenParser.AddToken("", (1f, 1f, 1f));
        tokenParser.AddToken(".", (1f, 1f, 1f));
        tokenParser.AddToken("_", (0f, 1f, 1f));
        tokenParser.AddToken("'", (1f, 0f, 1f));
        tokenParser.AddToken("###", (0f, 0f, 0f));
        tokenParser.AddToken("##", (0f, 0f, 0f));
        tokenParser.AddToken("#", (0f, 0f, 0f));

        return TestHelpers.Parse(text, tokenParser, out parsedBounds, trx);
    }

    MovementSourceData ParseMovementData<TMovementType>(TMovementType m,
                                                        string sourceText,
                                                        Func<(float walk, float swimg, float ether), float> trx,
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
        logger.Debug("Outbound\n{OutBoundDirections}", outboundDirectionalityMap.PrintEdges(parsedBounds));
        logger.Debug("In: {In} Out: {Out}", inboundDirectionalityMap.At(3, 1), outboundDirectionalityMap.At(3, 1));

        return new MovementSourceData(m, resistanceMap.As3DMap(0), inboundDirectionalityMap.As3DMap(0), outboundDirectionalityMap.As3DMap(0));
    }


    [Test]
    [TestCase(nameof(EmptyRoom))]
    [TestCase(nameof(DiagonalRoom))]
    [TestCase(nameof(DonutRoom))]
    public void ValidateZonePainting(string id)
    {
        var (sourceText, resultText, _, _, _) = testCases[id];

        logger.Debug("Using room layout \n{Map}", sourceText);


        var ms = new MovementDataCollector();
        ms.RegisterMovementSource(ParseMovementData(WalkingMovement.Instance, sourceText, x => x.Item1, out var parsedBounds));
        ms.RegisterMovementSource(ParseMovementData(SwimmingMovement.Instance, sourceText, x => x.Item2, out _));
        ms.RegisterMovementSource(ParseMovementData(EtherealMovement.Instance, sourceText, x => x.Item3, out _));

        var mr = new MovementModeRegistry();
        mr.Register(WalkingMovement.Instance);
        mr.Register(SwimmingMovement.Instance);
        mr.Register(EtherealMovement.Instance);

        var mapConfig = new DynamicDataViewConfiguration(0, 0, 16, 16);

        var pfs = new SingleLevelPathFinderSource(new SingleLevelPathFinderPolicy(), ms);
        var sys = new HierarchicalPathfindingSystemCollection(mapConfig, ms, pfs);
        sys.RegisterMovementCombination(WalkingMovement.Instance, DistanceCalculation.Euclid);
        sys.RegisterMovementCombination(SwimmingMovement.Instance, DistanceCalculation.Chebyshev);
        sys.Initialize();
        sys.Process();

        var result = sys.ZoneDataView;
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
    [TestCase(nameof(DiagonalRoom))]
    [TestCase(nameof(DonutRoom))]
    public void ValidateEdgeDetection(string id)
    {
        var (sourceText, resultText, _, _, _) = testCases[id];

        logger.Debug("Using room layout \n{Map}", sourceText);


        var ms = new MovementDataCollector();
        ms.RegisterMovementSource(ParseMovementData(WalkingMovement.Instance, sourceText, x => x.Item1, out var parsedBounds));
        ms.RegisterMovementSource(ParseMovementData(SwimmingMovement.Instance, sourceText, x => x.Item2, out _));
        ms.RegisterMovementSource(ParseMovementData(EtherealMovement.Instance, sourceText, x => x.Item3, out _));

        var mr = new MovementModeRegistry();
        mr.Register(WalkingMovement.Instance);
        mr.Register(SwimmingMovement.Instance);
        mr.Register(EtherealMovement.Instance);

        var mapConfig = new DynamicDataViewConfiguration(0, 0, 16, 16);

        var pfs = new SingleLevelPathFinderSource(new SingleLevelPathFinderPolicy(), ms);
        var sys = new HierarchicalPathfindingSystemCollection(mapConfig, ms, pfs);
        sys.RegisterMovementCombination(WalkingMovement.Instance, DistanceCalculation.Euclid);
        sys.RegisterMovementCombination(SwimmingMovement.Instance, DistanceCalculation.Chebyshev);
        sys.Initialize();
        sys.Process();
        sys.Initialize();
        sys.Process();

        var result = sys.ZoneDataView;
        result.TryGetView(0, out var zone2D).Should().BeTrue();
        zone2D!.TryGetData(0, 0, out var maybeZone).Should().BeTrue();
        var zoneView = maybeZone as PathfinderRegionDataView;

        Assert.NotNull(zoneView);

        Console.WriteLine("Collected Edges:");
        var edges3D = sys.EdgeDataView;
        edges3D.TryGetView(0, out var edges2D).Should().BeTrue();
        edges2D!.TryGetView(new Position2D(0, 0), out var edgesZone).Should().BeTrue();
        
        foreach (var c in edgesZone.GetZones())
        {
            Console.WriteLine(c + " -> \n  Outbound:");
            if (!edgesZone.TryGetZone(c, DistanceCalculation.Euclid, WalkingMovement.Instance, out var walkingEdges))
            {
                continue;
            }
            foreach (var outbound in walkingEdges.GetOutboundConnections())
            {
                Console.WriteLine("   " + outbound.pos);
                foreach (var edge in outbound.record.outboundConnections.Keys)
                {
                    Console.WriteLine("   -> " + edge.EdgeSource + " -> " + edge.EdgeTargetDirection);
                }
            }
            Console.WriteLine("  Inbound:");
            foreach (var inbound in walkingEdges.GetInboundConnections())
            {
                Console.WriteLine("   " + inbound.pos);
                foreach (var edge in inbound.record.inboundEdges)
                {
                    Console.WriteLine("   -> " + edge.EdgeSource + " -> " + edge.EdgeTargetDirection);
                }
            }
        }
/*
        sys.DataView.TryGetRegionView2D(0, out var region);
        region.TryGetRegionData(4, 1, out var a, out var zoneAt);
        a.TryGetZone(zoneAt, out var z);
  */      
        
        var producedResultText = zoneView.PrintEdges();
        Console.WriteLine("Edges:\n" + producedResultText);
        producedResultText.NormalizeMultilineText().Should().Be(resultText.NormalizeMultilineText());
    }

    [Test]
    [TestCase(nameof(EmptyRoom))]
    [TestCase(nameof(DiagonalRoom))]
    [TestCase(nameof(DonutRoom))]
    public void ValidatePathfinding(string id)
    {
        var (sourceText, _, resultText, sourcePos, targetPos) = testCases[id];
        var (sx, sy) = sourcePos;
        var (tx, ty) = targetPos;
        logger.Debug("Using room layout \n{Map}", sourceText);


        var ms = new MovementDataCollector();
        ms.RegisterMovementSource(ParseMovementData(WalkingMovement.Instance, sourceText, x => x.Item1, out var bounds));
        ms.RegisterMovementSource(ParseMovementData(SwimmingMovement.Instance, sourceText, x => x.Item2, out _));
        ms.RegisterMovementSource(ParseMovementData(EtherealMovement.Instance, sourceText, x => x.Item3, out _));

        var mr = new MovementModeRegistry();
        mr.Register(WalkingMovement.Instance);
        mr.Register(SwimmingMovement.Instance);
        mr.Register(EtherealMovement.Instance);

        var mapConfig = new DynamicDataViewConfiguration(0, 0, 16, 16);
        var resistanceMap3D = ms.MovementCosts[WalkingMovement.Instance].Costs;
        resistanceMap3D.TryGetView(0, out var resistanceMap).Should().BeTrue();
        Assert.NotNull(resistanceMap);

        var pfs = new SingleLevelPathFinderSource(new SingleLevelPathFinderPolicy(), ms);
        var sys = new HierarchicalPathfindingSystemCollection(mapConfig, ms, pfs);
        sys.RegisterMovementCombination(WalkingMovement.Instance, DistanceCalculation.Euclid);
        sys.RegisterMovementCombination(SwimmingMovement.Instance, DistanceCalculation.Chebyshev);
        sys.Initialize();
        sys.Process();
        sys.Initialize();
        sys.Process();

        var pathFinderPolicy = new HierarchicalPathFinderPolicy(mapConfig, sys);
        var hlp = new HierarchicalPathfinderSource(pfs, pathFinderPolicy, ms);
        
        var startPosition = EntityGridPosition.OfRaw(0, sx, sy);
        var targetPosition = EntityGridPosition.OfRaw(0, tx, ty);
        using var pfbuilder = pfs.GetPathFinder();
        using var pf = pfbuilder.Data.WithTarget(new DefaultPathFinderTargetEvaluator().WithTargetPosition(targetPosition))
                                .Build(new AggregateMovementCostFactors(new MovementCost(WalkingMovement.Instance, DistanceCalculation.Euclid, 1)));

        pf.TryFindPath(startPosition, out var result).Should().BeTrue();
        var (resultHint, resultPath, resultCost) = result;
        resultHint.Should().Be(PathFinderResult.Found);
            
        var expectedResultMap = ParseResultMap(resultText, out _);
        var producedResultMap = CreateResult(resistanceMap, resultPath, Position.From(startPosition), bounds);
        logger.Debug("Expected Result:\n{Expected}\nComputed Result:\n{Computed}",
                     PrintResultMap(expectedResultMap, bounds),
                     PrintResultMap(producedResultMap, bounds));

        TestHelpers.AssertEquals(producedResultMap, expectedResultMap, bounds, default, PrintResultMap);

    }
}