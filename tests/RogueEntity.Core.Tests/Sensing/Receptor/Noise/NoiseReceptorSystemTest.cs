using System;
using EnTTSharp.Entities.Systems;
using FluentAssertions;
using NUnit.Framework;
using RogueEntity.Core.Infrastructure.Time;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Sensing;
using RogueEntity.Core.Sensing.Cache;
using RogueEntity.Core.Sensing.Common.FloodFill;
using RogueEntity.Core.Sensing.Common.Physics;
using RogueEntity.Core.Sensing.Receptors;
using RogueEntity.Core.Sensing.Receptors.Noise;
using RogueEntity.Core.Sensing.Resistance;
using RogueEntity.Core.Sensing.Resistance.Maps;
using RogueEntity.Core.Sensing.Sources.Noise;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.Algorithms;

namespace RogueEntity.Core.Tests.Sensing.Receptor.Noise
{
    public class NoiseReceptorSystemTest : SenseReceptorSystemBase<NoiseSense, NoiseSense, NoiseSourceDefinition, NoiseSystem>
    {
        const string EmptyRoom = @"
// 11x11; an empty room
1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1
1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 
1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 
1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 
1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 
1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 
1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 
1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 
1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 
1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 
1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 
1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 
1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 
1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 
1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 
1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 
1, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1 
1, 0, 0, 0, 0, 0, 0, 0, 1 
1, 0, 0, 0, 0, 0, 0, 0, 1 
1, 0, 0, 0, 0, 0, 0, 0, 1 
1, 0, 0, 0, 0, 0, 0, 0, 1 
1, 1, 1, 1, 1, 1, 1, 1, 1 
";

        const string EmptyRoomPerceptionStrength = @"
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   , 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   , 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   , 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   , 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   , 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   , 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   , 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   , 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   , 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   , 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
";

        const string EmptyRoomSenseMapResult = @"
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,  7.000,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,  8.000,   .   ,   .   ,   .    
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,  9.000,   .   ,   .   ,   .    
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   , 10.000,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
";

        const string EmptyRoomSenseMapDirections = @"
  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ 
  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ 
  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ 
  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ 
  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ , ┬  ,  ~ ,  ~ ,  ~ 
  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ , ┬  ,  ~ ,  ~ ,  ~ 
  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ , ┬  ,  ~ ,  ~ ,  ~ 
  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ , ┼ *,  ~ ,  ~ ,  ~ 
  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ 
  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ 
  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ 
  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ 
  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ 
  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ 
  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ 
  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ 
  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ 
  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ 
  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ 
  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ 
  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ 
  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~
";
        
        /// <summary>
        ///  A room with a pillar blocking the direct line of sight to the noise source.
        ///  Anyone staying in a cheap hotel knows that noise travels through any opening
        ///  and does not need a line of sight to hit you. 
        /// </summary>
        const string PillarRoom = @"
// 11x11; an empty room
1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1
1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 
1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 
1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 
1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 
1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 
1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 0, 1, 1, 0, 1 
1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, ., 0, 0, 1 
1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 
1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 
1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 
1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 
1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 
1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 
1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 
1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 
1, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1 
1, 0, 0, 0, 0, 0, 0, 0, 1 
1, 0, 0, 0, 0, 0, 0, 0, 1 
1, 0, 0, 0, 0, 0, 0, 0, 1 
1, 0, 0, 0, 0, 0, 0, 0, 1 
1, 1, 1, 1, 1, 1, 1, 1, 1 
";


        const string PillarRoomPerceptionStrength = @"
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   , 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   , 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   , 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   , 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   , 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   , 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000,   .   ,   .   , 10.000,   .   ,   .   , 10.000,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   , 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   , 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   , 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   , 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
";

        const string PillarRoomSenseMapResult = @"
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,  7.000,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,  8.000,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,  9.000,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   , 10.000,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
";

        const string PillarRoomDirectionResult = @"
  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ 
  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ 
  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ 
  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ 
  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ , ┐  ,  ~ ,  ~ ,  ~ 
  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ , ┬  ,  ~ ,  ~ ,  ~ ,  ~ 
  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ , ┌  ,  ~ ,  ~ ,  ~ ,  ~ 
  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ , ┼ *,  ~ ,  ~ ,  ~ 
  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ 
  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ 
  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ 
  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ 
  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ 
  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ 
  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ 
  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ 
  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ 
  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ 
  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ 
  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ 
  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ 
  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~
";
        
        readonly NoisePhysicsConfiguration sourcePhysics;
        readonly NoiseSenseReceptorPhysicsConfiguration physics;
        public NoiseReceptorSystemTest()
        {
            this.sourcePhysics = new NoisePhysicsConfiguration(LinearDecaySensePhysics.For(DistanceCalculation.Chebyshev));
            this.physics = new NoiseSenseReceptorPhysicsConfiguration(sourcePhysics, new FloodFillWorkingDataSource());
        }

        protected override SensoryResistance Convert(float f)
        {
            return new SensoryResistance(Percentage.Empty, Percentage.Of(f), Percentage.Empty, Percentage.Empty);
        }

        protected override Action<SenseMappingTestContext> CreateCopyAction()
        {
            var builder = context.ItemEntityRegistry.BuildSystem()
                                 .WithContext<SenseMappingTestContext>();

            var omniSystem = new SenseReceptorBlitterSystem<NoiseSense, NoiseSense>(senseSystem, new DefaultDirectionalSenseReceptorBlitter());
            return builder.CreateSystem<SingleLevelSenseDirectionMapData<NoiseSense, NoiseSense>, SensoryReceptorState<NoiseSense, NoiseSense>>(omniSystem.CopySenseSourcesToVisionField);
        }

        protected override NoiseSystem CreateSourceSystem()
        {
            return new NoiseSystem(senseProperties.AsLazy<ISensePropertiesSource>(),
                                   senseCache.AsLazy<IGlobalSenseStateCacheProvider>(),
                                   timeSource.AsLazy<ITimeSource>(),
                                   senseCache,
                                   sourcePhysics.CreateNoisePropagationAlgorithm(), 
                                   sourcePhysics.NoisePhysics);
        }

        protected override ReferenceItemDeclaration<SenseMappingTestContext, ItemReference> AttachTrait(ReferenceItemDeclaration<SenseMappingTestContext, ItemReference> decl)
        {
            switch (decl.Id.Id)
            {
                case "SenseReceptor-Active-10":
                    decl.WithTrait(new NoiseDirectionSenseTrait<SenseMappingTestContext, ItemReference>(physics, 10));
                    return decl;
                case "SenseReceptor-Active-5":
                    decl.WithTrait(new NoiseDirectionSenseTrait<SenseMappingTestContext, ItemReference>(physics, 5));
                    return decl;
                case "SenseReceptor-Inactive-5":
                    decl.WithTrait(new NoiseDirectionSenseTrait<SenseMappingTestContext, ItemReference>(physics, 5, false));
                    return decl;
                case "SenseSource-Active-10":
                    decl.WithTrait(new NoiseSourceTrait<SenseMappingTestContext, ItemReference>(sourcePhysics));
                    return decl;
                case "SenseSource-Active-5":
                    decl.WithTrait(new NoiseSourceTrait<SenseMappingTestContext, ItemReference>(sourcePhysics));
                    return decl;
                case "SenseSource-Inactive-5":
                    decl.WithTrait(new NoiseSourceTrait<SenseMappingTestContext, ItemReference>(sourcePhysics));
                    return decl;
                default:
                    throw new ArgumentException();
            }
        }

        protected override SenseReceptorSystemBase<NoiseSense, NoiseSense> CreateSystem()
        {
            return new NoiseReceptorSystem(senseProperties.AsLazy<ISensePropertiesSource>(),
                                           senseCache.AsLazy<ISenseStateCacheProvider>(),
                                           senseCache.AsLazy<IGlobalSenseStateCacheProvider>(),
                                           timeSource.AsLazy<ITimeSource>(),
                                           physics.NoisePhysics,
                                           physics.CreateNoiseSensorPropagationAlgorithm());
        }

        protected override void PrepareSourceItems(ItemReference active10, ItemReference active5, ItemReference inactive)
        {
            context.ItemResolver.TryUpdateData(active10, context, new NoiseClip(10, "Strength10"), out _).Should().BeTrue();
            context.ItemResolver.TryUpdateData(active5, context, new NoiseClip(5, "Strength5"), out _).Should().BeTrue();
            base.PrepareSourceItems(active10, active5, inactive);
        }

        [Test]
        [TestCase(nameof(EmptyRoom), EmptyRoom, EmptyRoomPerceptionStrength, EmptyRoomSenseMapResult, EmptyRoomSenseMapDirections)]
        [TestCase(nameof(PillarRoom), PillarRoom, PillarRoomPerceptionStrength, PillarRoomSenseMapResult, PillarRoomDirectionResult)]
        public void Do(string id, string sourceText, string expectedResultText, string expectedSenseMapText, string expectedDirections)
        {
            base.PerformTest(id, sourceText, expectedResultText, expectedSenseMapText, expectedDirections);
        }
    }
}