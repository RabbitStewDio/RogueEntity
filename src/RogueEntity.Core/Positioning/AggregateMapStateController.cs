using EnTTSharp;
using RogueEntity.Core.Utils;
using System;
using System.Collections.Generic;

namespace RogueEntity.Core.Positioning
{
    public class AggregateMapStateController: IMapStateController
    {
        readonly List<IMapStateController> maps;

        public AggregateMapStateController()
        {
            maps = new List<IMapStateController>();
        }

        public void Add(IMapStateController c)
        {
            if (c == null)
            {
                throw new ArgumentNullException(nameof(c));
            }

            maps.Add(c);
        }
        
        public void ResetState()
        {
            foreach (var m in maps)
            {
                m.ResetState();
            }
        }

        public void ResetLevel(int z)
        {
            foreach (var m in maps)
            {
                m.ResetLevel(z);
            }
        }

        public void MarkDirty<TPosition>(in TPosition position)
            where TPosition : IPosition<TPosition>
        {
            foreach (var m in maps)
            {
                m.MarkDirty(position);
            }
        }

        public void MarkRegionDirty(int zPositionFrom, int zPositionTo, Optional<Rectangle> layerArea = default)
        {
            foreach (var m in maps)
            {
                m.MarkRegionDirty(zPositionFrom, zPositionTo, layerArea);
            }
        }
    }
}
