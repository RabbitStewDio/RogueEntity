using System;
using FluentAssertions;
using NUnit.Framework;
using RogueEntity.Core.GridProcessing.Directionality;
using RogueEntity.Core.Positioning.Algorithms;
using RogueEntity.Core.Sensing;
using RogueEntity.Core.Sensing.Common;
using RogueEntity.Core.Sensing.Common.FloodFill;
using RogueEntity.Core.Sensing.Common.Physics;
using RogueEntity.Core.Sensing.Resistance.Directions;
using RogueEntity.Core.Tests.Fixtures;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.DataViews;
using static RogueEntity.Core.Tests.Sensing.SenseTestHelpers;

namespace RogueEntity.Core.Tests.Sensing.Common
{
    public class FloodFillCastTest
    {
        const string EmptyRoom = @"
// 11x11; an empty room
  1.000,  1.000,  1.000,  1.000,  1.000,  1.000,  1.000,  1.000,  1.000
  1.000,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,  1.000
  1.000,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,  1.000
  1.000,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,  1.000
  1.000,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,  1.000
  1.000,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,  1.000
  1.000,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,  1.000
  1.000,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,  1.000
  1.000,  1.000,  1.000,  1.000,  1.000,  1.000,  1.000,  1.000,  1.000
";

        const string EmptyRoomResult = @"
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,  0.757,  1.172,  1.586,  2.000,  1.586,  1.172,  0.757,   .   
   .   ,  1.172,  2.172,  2.586,  3.000,  2.586,  2.172,  1.172,   .   
   .   ,  1.586,  2.586,  3.586,  4.000,  3.586,  2.586,  1.586,   .   
   .   ,  2.000,  3.000,  4.000,  5.000,  4.000,  3.000,  2.000,   .   
   .   ,  1.586,  2.586,  3.586,  4.000,  3.586,  2.586,  1.586,   .   
   .   ,  1.172,  2.172,  2.586,  3.000,  2.586,  2.172,  1.172,   .   
   .   ,  0.757,  1.172,  1.586,  2.000,  1.586,  1.172,  0.757,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
";

        const string EmptyRoomResultCheb = @"
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,  2.000,  2.000,  2.000,  2.000,  2.000,  2.000,  2.000,   .   
   .   ,  2.000,  3.000,  3.000,  3.000,  3.000,  3.000,  2.000,   .   
   .   ,  2.000,  3.000,  4.000,  4.000,  4.000,  3.000,  2.000,   .   
   .   ,  2.000,  3.000,  4.000,  5.000,  4.000,  3.000,  2.000,   .   
   .   ,  2.000,  3.000,  4.000,  4.000,  4.000,  3.000,  2.000,   .   
   .   ,  2.000,  3.000,  3.000,  3.000,  3.000,  3.000,  2.000,   .   
   .   ,  2.000,  2.000,  2.000,  2.000,  2.000,  2.000,  2.000,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
";

        const string EmptyRoomDirections = @"
  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ 
  ~ , ┌  , ┌  , ┌  , ┬  , ┐  , ┐  , ┐  ,  ~ 
  ~ , ┌  , ┌  , ┌  , ┬  , ┐  , ┐  , ┐  ,  ~ 
  ~ , ┌  , ┌  , ┌  , ┬  , ┐  , ┐  , ┐  ,  ~ 
  ~ , ├  , ├  , ├  , ┼ *, ┤  , ┤  , ┤  ,  ~ 
  ~ , └  , └  , └  , ┴  , ┘  , ┘  , ┘  ,  ~ 
  ~ , └  , └  , └  , ┴  , ┘  , ┘  , ┘  ,  ~ 
  ~ , └  , └  , └  , ┴  , ┘  , ┘  , ┘  ,  ~ 
  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~
";

        const string EmptyRoomDirectionsCheb = @"
  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ 
  ~ , ┌  , ┌  , ┌  , ┬  , ┐  , ┐  , ┐  ,  ~ 
  ~ , ┌  , ┌  , ┌  , ┬  , ┐  , ┐  , ┤  ,  ~ 
  ~ , ┌  , ┌  , ┌  , ┬  , ┐  , ┐  , ┐  ,  ~ 
  ~ , ├  , ├  , ├  , ┼ *, ┤  , ┤  , ┤  ,  ~ 
  ~ , └  , └  , └  , ┴  , ┘  , ┘  , ┘  ,  ~ 
  ~ , ├  , └  , └  , ┴  , ┘  , ┘  , ┘  ,  ~ 
  ~ , └  , └  , └  , ┴  , ┘  , ┘  , ┘  ,  ~ 
  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ 
";

        const string RoomWithPillars = @"
//  A room with a long winding corridor. 
//
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,  1.000,  1.000,  1.000,  1.000,  1.000,  1.000,  1.000,  1.000,  1.000,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,  1.000,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,  1.000,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,  1.000,   .   ,  1.000,  1.000,  1.000,  1.000,   .   ,   .   ,  1.000,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,  1.000,   .   ,   .   ,   .   ,   .   ,  1.000,  1.000,   .   ,  1.000,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,  1.000,  1.000,  1.000,  1.000,   .   ,  1.000,   .   ,   .   ,  1.000,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,  1.000,   .   ,   .   ,  1.000,  1.000,  1.000,   .   ,   .   ,  1.000,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,  1.000,   .   ,  1.000,  1.000,   .   ,   .   ,  1.000,  1.000,  1.000,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,  1.000,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,  1.000,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,  1.000,  1.000,  1.000,  1.000,  1.000,  1.000,  1.000,  1.000,  1.000,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
";

        const string RoomWithPillarsResult = @"
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,  3.172,  2.757,  1.757,  0.757,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,  4.172,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,  4.586,  5.586,  6.586,  7.000,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,  8.000,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
";

        const string RoomWithPillarDirections = @"
  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ 
  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ 
  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ 
  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ 
  ~ ,  ~ ,  ~ ,  ~ , ┬  , ┐  , ┤  , ┤  ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ 
  ~ ,  ~ ,  ~ ,  ~ , ┌  ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ 
  ~ ,  ~ ,  ~ ,  ~ , ├  , ├  , ┌  , ┬  ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ 
  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ , ┼ *,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ 
  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ 
  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ 
  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ 
  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ 
  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ 
  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ 
  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~
";

        const string RoomWithPartialBlock = @"
//  A room with a long winding corridor. 
//
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,  1.000,  1.000,  1.000,  1.000,  1.000,  1.000,  1.000,  1.000,  1.000,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,  1.000,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,  1.000,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,  1.000,   .   ,  1.000,  1.000,  1.000,  1.000,   .   ,   .   ,  1.000,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,  1.000,   .   ,   .   ,   .   ,   .   ,  1.000,  1.000,   .   ,  1.000,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,  1.000,  1.000,  1.000,  1.000,   .   ,  1.000,   .   ,   .   ,  1.000,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,  1.000,   .   ,   .   ,  1.000,  0.500,  1.000,   .   ,   .   ,  1.000,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,  1.000,   .   ,  1.000,  1.000,   .   ,   .   ,  1.000,  1.000,  1.000,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,  1.000,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,  1.000,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,  1.000,  1.000,  1.000,  1.000,  1.000,  1.000,  1.000,  1.000,  1.000,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
";

        const string RoomWithPartialBlockResult = @"
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,  3.172,  2.757,  1.757,  0.757,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,  4.172,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,  4.586,  5.586,  6.586,  7.000,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,  8.000,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,  3.500,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,  2.500,  2.086,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,  0.086,  1.086,  1.500,  1.086,  0.672,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
";

        const string RoomWithPartialBlockDirections = @"
  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ 
  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ 
  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ 
  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ 
  ~ ,  ~ ,  ~ ,  ~ , ┬  , ┐  , ┤  , ┤  ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ 
  ~ ,  ~ ,  ~ ,  ~ , ┌  ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ 
  ~ ,  ~ ,  ~ ,  ~ , ├  , ├  , ┌  , ┬  ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ 
  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ , ┼ *,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ 
  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ , ┴  ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ 
  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ , ┴  , ┘  ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ 
  ~ ,  ~ ,  ~ ,  ~ ,  ~ , ├  , └  , ┴  , ┘  , ┘  ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ 
  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ 
  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ 
  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ 
  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~
";

        const string RoomWithPartialBlockResultCard = @"
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,  2.000,  1.000,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,  3.000,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,  4.000,  5.000,  6.000,  7.000,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,  8.000,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,  3.500,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,  2.500,  1.500,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,  0.500,  1.500,  0.500,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
";

        const string RoomWithPartialBlockDirectionsCard = @"
  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ 
  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ 
  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ 
  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ 
  ~ ,  ~ ,  ~ ,  ~ , ┬  , ┤  ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ 
  ~ ,  ~ ,  ~ ,  ~ , ┬  ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ 
  ~ ,  ~ ,  ~ ,  ~ , ├  , ├  , ├  , ┬  ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ 
  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ , ┼ *,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ 
  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ , ┴  ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ 
  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ , ┴  , ┤  ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ 
  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ , ├  , ┴  , ┤  ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ 
  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ 
  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ 
  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ 
  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~
";

        [Test]
        [TestCase(nameof(EmptyRoom), 9, 9, DistanceCalculation.Euclid, AdjacencyRule.EightWay, EmptyRoom, EmptyRoomResult, EmptyRoomDirections)]
        [TestCase(nameof(EmptyRoom), 9, 9, DistanceCalculation.Chebyshev, AdjacencyRule.EightWay, EmptyRoom, EmptyRoomResultCheb, EmptyRoomDirectionsCheb)]
        [TestCase(nameof(RoomWithPillars), 15, 15, DistanceCalculation.Euclid, AdjacencyRule.EightWay, RoomWithPillars, RoomWithPillarsResult, RoomWithPillarDirections)]
        [TestCase(nameof(RoomWithPartialBlock), 15, 15, DistanceCalculation.Euclid, AdjacencyRule.EightWay, RoomWithPartialBlock, RoomWithPartialBlockResult, RoomWithPartialBlockDirections)]
        [TestCase(nameof(RoomWithPartialBlock), 15, 15, DistanceCalculation.Euclid, AdjacencyRule.Cardinals, RoomWithPartialBlock, RoomWithPartialBlockResultCard, RoomWithPartialBlockDirectionsCard)]
        public void ValidateMap(string name, int width, int height, DistanceCalculation dc, AdjacencyRule ar, string sourceText, string intensityResultText, string directionResultText)
        {
            var radius = width / 2;
            var source = new SenseSourceDefinition(dc, ar, radius + 1);
            var pos = new Position2D(width / 2, height / 2);

            var resistanceMap = ParseMap(sourceText);
            Console.WriteLine("Using room layout \n" + TestHelpers.PrintMap(resistanceMap, new Rectangle(0, 0, width, height)));

            var directionalityMapSystem = new SensoryResistanceDirectionalitySystem<VisionSense>(resistanceMap.As3DMap(0));
            directionalityMapSystem.MarkGloballyDirty();
            directionalityMapSystem.Process();
            directionalityMapSystem.ResultView.TryGetView(0, out var directionalityMap).Should().BeTrue();

            var algo = new FloodFillPropagationAlgorithm(LinearDecaySensePhysics.For(dc), new FloodFillWorkingDataSource());
            var calculatedResult = algo.Calculate(source, source.Intensity, pos, resistanceMap, directionalityMap);
            Console.WriteLine(TestHelpers.PrintMap(calculatedResult, new Rectangle(new Position2D(0, 0), radius, radius)));
            Console.WriteLine(TestHelpers.PrintMap(new SenseMapDirectionTestView(calculatedResult), new Rectangle(new Position2D(0, 0), radius, radius)));
            Console.WriteLine(TestHelpers.PrintMap(directionalityMap.Transform(x => $" [{x.ToFormattedString()}]"), new Rectangle(0, 0, width, height)));

            var expectedResult = ParseMap(intensityResultText);
            TestHelpers.AssertEquals(calculatedResult, expectedResult, new Rectangle(0, 0, width, height), pos);

            var expectedDirections = ParseDirections(directionResultText, out _);
            TestHelpers.AssertEquals(calculatedResult, expectedDirections, new Rectangle(0, 0, width, height), pos, PrintSenseDirectionStore);
        }
    }
}