using Microsoft.Xna.Framework;
using RogueEntity.SadCons;
using RogueEntity.SadCons.Controls;
using RogueEntity.Samples.BoxPusher.Core.ItemTraits;
using SadConsole;
using SadConsole.Controls;
using System;
using System.Collections.Generic;

namespace RogueEntity.Samples.BoxPusher.MonoGame
{
    public class BoxPusherProfileItemTheme: FlexibleListBoxItemTheme<PlayerProfileContainer<BoxPusherPlayerProfile>>
    {
        readonly Dictionary<(Guid, ControlStates state), (BoxPusherPlayerProfile profile, ColoredString parsedString)> cachedValues;
        
        public BoxPusherProfileItemTheme(int itemHeight = 1) : base(itemHeight)
        {
            cachedValues = new Dictionary<(Guid, ControlStates state), (BoxPusherPlayerProfile profile, ColoredString parsedString)>();
        }

        protected override ColoredString FormatValue(FlexibleCursor cursor, 
                                                     Rectangle area, 
                                                     PlayerProfileContainer<BoxPusherPlayerProfile> item,
                                                     ControlStates state)
        {
            var key = (item.Id, state);
            if (cachedValues.TryGetValue(key, out var textTuple) &&
                textTuple.profile == item.Profile)
            {
                return textTuple.parsedString;
            }

            System.Console.WriteLine(area);
            
            var formatted = $"{item.Profile.PlayerName}\n";
            var maxLevel = item.Profile.MaxLevelComplete;
            if (maxLevel > 0)
            {
                formatted += $"Level {maxLevel} complete";
            }
            else
            {
                formatted += "No level completed";
            }
            
            var str = cursor.PrepareColoredString(formatted);
            cachedValues[key] = (item.Profile, str);
            return str;
        }
    }
}
