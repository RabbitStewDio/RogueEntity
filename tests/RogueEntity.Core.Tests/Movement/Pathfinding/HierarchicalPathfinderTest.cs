using FluentAssertions;
using NUnit.Framework;
using RogueEntity.Api.Utils;
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
 ### , ### , ### , ### , ### , ### , ### , ### , ### 
 ### ,  @  ,  .  ,  .  ,  .  ,  .  ,  .  ,  .  , ### 
 ### ,  .  ,   6 ,  .  ,  .  ,  .  ,  .  ,  .  , ### 
 ### ,  .  ,  .  ,   5 ,  .  ,  .  ,  .  ,  .  , ### 
 ### ,  .  ,  .  ,  .  ,   4 ,  .  ,  .  ,  .  , ### 
 ### ,  .  ,  .  ,  .  ,  .  ,   3 ,  .  ,  .  , ### 
 ### ,  .  ,  .  ,  .  ,  .  ,  .  ,   2 ,  .  , ### 
 ### ,  .  ,  .  ,  .  ,  .  ,  .  ,  .  ,   1 , ### 
 ### ,  .  ,  .  ,  .  ,  .  ,  .  ,  .  ,  .  , ### 
 ### , ### , ### , ### , ### , ### , ### , ### , ### 
";
        const string DiagonalBlockRoom = @"
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

        const string DiagonalRoomResult = @"
 ### , ### , ### , ### , ### , ### , ### , ### , ### 
 ### ,  .  ,  .  ,  .  ,  .  ,  .  ,  .  ,  .  , ### 
 ### ,  .  ,  .  ,  .  ,  .  ,  .  ,  .  ,  .  , ### 
 ### ,  .  ,  .  ,   . , ### ,  .  ,  .  ,  .  , ### 
 ### ,  .  ,  .  , ### ,  .  ,  .  ,  .  ,  .  , ### 
 ### ,  .  ,  .  ,  .  ,  .  ,  .  ,  .  ,  .  , ### 
 ### ,  .  ,  .  ,  .  ,  .  ,  .  ,  .  ,  .  , ### 
 ### ,  .  ,  .  ,  .  ,  .  ,  .  ,  .  ,  .  , ### 
 ### ,  .  ,  .  ,  .  ,  .  ,  .  ,  .  ,  .  , ### 
 ### , ### , ### , ### , ### , ### , ### , ### , ### 
";

    readonly ILogger logger = SLog.ForContext<SingleLevelPathFinderTest>();

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
        where TMovementType: IMovementMode
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
        logger.Debug("In: {In} Out: {Out}", inboundDirectionalityMap[3, 1], outboundDirectionalityMap[3, 1]); 
        
        return new MovementSourceData(m, resistanceMap.As3DMap(0), inboundDirectionalityMap.As3DMap(0), outboundDirectionalityMap.As3DMap(0));
    }
    

    [Test]
    [TestCase(nameof(EmptyRoom), EmptyRoom, EmptyRoomResult, 1, 1, 7, 7)]
    //[TestCase(nameof(DiagonalBlockRoom), DiagonalBlockRoom, DiagonalRoomResult, 1, 1, 7, 7)]
    public void ValidatePathFinding(string id, string sourceText, string resultText, int sx, int sy, int tx, int ty)
    {
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
        sys.PaintDirtyPathfinderRegions();

        var result = sys.ZoneInformation;
        result.TryGetView(0, out var zone2D).Should().BeTrue();
        zone2D!.TryGetData(0, 0, out var maybeZone).Should().BeTrue(); 
        var zoneView = maybeZone as PathfinderRegionDataView;
        
        Assert.NotNull(zoneView);
        
        Console.WriteLine("Edges:\n" + zoneView.PrintEdges());
    }
}