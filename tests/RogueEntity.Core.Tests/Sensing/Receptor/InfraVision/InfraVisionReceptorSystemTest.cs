using System;
using EnTTSharp.Entities.Systems;
using NUnit.Framework;
using RogueEntity.Core.Infrastructure.Time;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Meta.ItemTraits;
using RogueEntity.Core.Sensing;
using RogueEntity.Core.Sensing.Cache;
using RogueEntity.Core.Sensing.Common;
using RogueEntity.Core.Sensing.Common.Physics;
using RogueEntity.Core.Sensing.Receptors;
using RogueEntity.Core.Sensing.Receptors.InfraVision;
using RogueEntity.Core.Sensing.Resistance;
using RogueEntity.Core.Sensing.Sources;
using RogueEntity.Core.Sensing.Sources.Heat;
using RogueEntity.Core.Utils.Algorithms;
using RogueEntity.Core.Utils.DataViews;

namespace RogueEntity.Core.Tests.Sensing.Receptor.InfraVision
{
    public class InfraVisionReceptorSystemTest : SenseReceptorSystemBase<VisionSense, TemperatureSense, HeatSourceDefinition>
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
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   , 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   , 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   , 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   , 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   , 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   , 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   , 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   , 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   , 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   , 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   , 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   , 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000
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
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,  1.000,  2.000,  3.000,  3.000,  3.000,  3.000,  3.000,  3.000,  3.000,  3.000,  3.000,  3.000,  3.000
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,  1.000,  2.000,  3.000,  4.000,  4.000,  4.000,  4.000,  4.000,  4.000,  4.000,  4.000,  4.000,  4.000
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,  1.000,  2.000,  3.000,  4.000,  5.000,  5.000,  5.000,  5.000,  5.000,  5.000,  5.000,  5.000,  5.000
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,  1.000,  2.000,  3.000,  4.000,  5.000,  6.000,  6.000,  6.000,  6.000,  6.000,  6.000,  6.000,  6.000
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,  1.000,  2.000,  3.000,  4.000,  5.000,  6.000,  7.000,  7.000,  7.000,  7.000,  7.000,  7.000,  7.000
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,  1.000,  2.000,  3.000,  4.000,  5.000,  6.000,  7.000,  8.000,  8.000,  8.000,  8.000,  8.000,  7.000
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,  1.000,  2.000,  3.000,  4.000,  5.000,  6.000,  7.000,  8.000,  9.000,  9.000,  9.000,  8.000,  7.000
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,  1.000,  2.000,  3.000,  4.000,  5.000,  6.000,  7.000,  8.000,  9.000, 10.000,  9.000,  8.000,  7.000
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,  1.000,  2.000,  3.000,  4.000,  5.000,  6.000,  7.000,  8.000,  9.000,  9.000,  9.000,  8.000,  7.000
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,  1.000,  2.000,  3.000,  4.000,  5.000,  6.000,  7.000,  8.000,  8.000,  8.000,  8.000,  8.000,  7.000
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,  1.000,  2.000,  3.000,  4.000,  5.000,  6.000,  7.000,  7.000,  7.000,  7.000,  7.000,  7.000,  7.000
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,  1.000,  2.000,  3.000,  4.000,  5.000,  6.000,  6.000,  6.000,  6.000,  6.000,  6.000,  6.000,  6.000
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

        const string EmptyRoomDirectionMapResult = @"
  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ , ┌# , ┌# , ┌# , ┌# , ┌# , ┌# , ┌# , ┌# , ┌# , ┬# , ┐# , ┐# , ┐# 
  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ , ┌  , ┌  , ┌  , ┌  , ┌  , ┌  , ┌  , ┌  , ┌  , ┬  , ┐  , ┐  , ┐# 
  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ , ┌  , ┌  , ┌  , ┌  , ┌  , ┌  , ┌  , ┌  , ┌  , ┬  , ┐  , ┐  , ┐# 
  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ , ┌  , ┌  , ┌  , ┌  , ┌  , ┌  , ┌  , ┌  , ┌  , ┬  , ┐  , ┐  , ┐# 
  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ , ┌  , ┌  , ┌  , ┌  , ┌  , ┌  , ┌  , ┌  , ┌  , ┬  , ┐  , ┐  , ┐# 
  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ , ┌  , ┌  , ┌  , ┌  , ┌  , ┌  , ┌  , ┌  , ┌  , ┬  , ┐  , ┐  , ┐# 
  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ , ┌  , ┌  , ┌  , ┌  , ┌  , ┌  , ┌  , ┌  , ┌  , ┬  , ┐  , ┐  , ┐# 
  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ , ├  , ├  , ├  , ├  , ├  , ├  , ├  , ├  , ├  , ┼ *, ┤  , ┤  , ┤# 
  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ , └  , └  , └  , └  , └  , └  , └  , └  , └  , ┴  , ┘  , ┘  , ┘# 
  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ , └  , └  , └  , └  , └  , └  , └  , └  , └  , ┴  , ┘  , ┘  , ┘# 
  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ , └  , └  , └  , └  , └  , └  , └  , └  , └  , ┴  , ┘  , ┘  , ┘# 
  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ , └# , └# , └# , └# , └# , └# , └# , └# , └# , ┴# , ┘# , ┘# , ┘# 
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
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   , 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   , 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   , 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   , 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   , 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   , 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   , 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000, 10.000
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   , 10.000, 10.000, 10.000, 10.000, 10.000,   .   ,   .   ,   .   , 10.000,   .   ,   .   ,   .   ,   .   , 10.000
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   , 10.000, 10.000, 10.000,   .   ,   .   ,   .   ,   .   , 10.000,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   , 10.000,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
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

        const string PillarRoomSenseMapResult = @"
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,  3.000,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,  4.000,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,  5.000,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,  6.000,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,  7.000,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,  8.000,   .   ,   .   ,   .   ,   .   ,   .   
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,  1.000,  2.000,  3.000,  4.000,  5.000,  6.000,   .   ,  8.000,  9.000,   .   ,   .   ,  8.000,  7.000
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,  1.000,  2.000,  3.000,  4.000,   .   ,   .   ,   .   ,  8.000,   .   ,   .   ,   .   ,   .   ,  7.000
   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,   .   ,  1.000,  2.000,   .   ,   .   ,   .   ,   .   ,  7.000,   .   ,   .   ,   .   ,   .   ,   .   ,   .   
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
  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ , ┌# ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ 
  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ , ┌  ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ 
  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ , ┌  ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ 
  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ , ┌  ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ 
  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ , ┌  ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ 
  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ , ┌  ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ 
  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ , ┌  , ┌  , ┌  , ┌  , ┌  , ┌  ,  ~ , ┌# , ┌  ,  ~ ,  ~ , ┐  , ┐# 
  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ , ├  , ├  , ├  , ├  ,  ~ ,  ~ ,  ~ , ├  ,  ~ ,  ~ ,  ~ ,  ~ , ┤# 
  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ , └  , └  ,  ~ ,  ~ ,  ~ ,  ~ , └  ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ ,  ~ 
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

        readonly HeatPhysicsConfiguration sourcePhysics;
        readonly InfraVisionSenseReceptorPhysicsConfiguration phy;

        public InfraVisionReceptorSystemTest()
        {
            this.sourcePhysics = new HeatPhysicsConfiguration(LinearDecaySensePhysics.For(DistanceCalculation.Chebyshev), Temperature.FromCelsius(0));
            this.phy = new InfraVisionSenseReceptorPhysicsConfiguration(sourcePhysics);
        }

        protected override Action<SenseMappingTestContext> CreateCopyAction()
        {
            var builder = context.ItemEntityRegistry.BuildSystem()
                                 .WithContext<SenseMappingTestContext>();

            var omniSystem = new SenseReceptorBlitterSystem<VisionSense, TemperatureSense>(senseSystem, new DefaultRadiationSenseReceptorBlitter());
            return builder.CreateSystem<SingleLevelSenseDirectionMapData<VisionSense, TemperatureSense>, SensoryReceptorState<VisionSense, TemperatureSense>>(omniSystem.CopySenseSourcesToVisionField);
        }

        protected override (ISensePropagationAlgorithm propagationAlgorithm, ISensePhysics sensePhysics) GetOrCreateSourceSensePhysics()
        {
            return (sourcePhysics.CreateHeatPropagationAlgorithm(), sourcePhysics.HeatPhysics);
        }

        protected override (ISensePropagationAlgorithm, ISensePhysics) GetOrCreateReceptorSensePhysics()
        {
            return (phy.CreateInfraVisionSensorPropagationAlgorithm(), phy.InfraVisionPhysics);
        }

        protected override ReferenceItemDeclaration<SenseMappingTestContext, ItemReference> AttachTrait(ReferenceItemDeclaration<SenseMappingTestContext, ItemReference> decl)
        {
            switch (decl.Id.Id)
            {
                case "SenseReceptor-Active-10":
                    decl.WithTrait(new InfraVisionSenseTrait<SenseMappingTestContext, ItemReference>(phy, 10));
                    return decl;
                case "SenseReceptor-Active-5":
                    decl.WithTrait(new InfraVisionSenseTrait<SenseMappingTestContext, ItemReference>(phy, 5));
                    return decl;
                case "SenseReceptor-Inactive-5":
                    decl.WithTrait(new InfraVisionSenseTrait<SenseMappingTestContext, ItemReference>(phy, 5, false));
                    return decl;
                case "SenseSource-Active-10":
                    decl.WithTrait(new HeatSourceTrait<SenseMappingTestContext, ItemReference>(sourcePhysics, Temperature.FromCelsius(10)));
                    return decl;
                case "SenseSource-Active-5":
                    decl.WithTrait(new HeatSourceTrait<SenseMappingTestContext, ItemReference>(sourcePhysics, Temperature.FromCelsius(5)));
                    return decl;
                case "SenseSource-Inactive-5":
                    decl.WithTrait(new HeatSourceTrait<SenseMappingTestContext, ItemReference>(sourcePhysics));
                    return decl;
                default:
                    throw new ArgumentException();
            }
        }


        protected override SenseSourceSystem<TemperatureSense, HeatSourceDefinition> CreateSourceSystem()
        {
            return new HeatSourceSystem(senseProperties.AsLazy<IReadOnlyDynamicDataView3D<SensoryResistance<TemperatureSense>>>(),
                                        senseCache.AsLazy<IGlobalSenseStateCacheProvider>(),
                                        timeSource.AsLazy<ITimeSource>(),
                                        directionalitySourceSystem,
                                        senseCache,
                                        sourcePhysics.CreateHeatPropagationAlgorithm(),
                                        sourcePhysics);
        }

        [Test]
        [TestCase(nameof(PillarRoom), PillarRoom, PillarRoomPerceptionStrength, PillarRoomSenseMapResult, PillarRoomDirectionResult)]
        [TestCase(nameof(EmptyRoom), EmptyRoom, EmptyRoomPerceptionStrength, EmptyRoomSenseMapResult, EmptyRoomDirectionMapResult)]
        public void Do(string id, string sourceText, string expectedResultText, string expectedSenseMapText, string expectedSenseMapDirections)
        {
            base.PerformTest(id, sourceText, expectedResultText, expectedSenseMapText, expectedSenseMapDirections);
        }
    }
}