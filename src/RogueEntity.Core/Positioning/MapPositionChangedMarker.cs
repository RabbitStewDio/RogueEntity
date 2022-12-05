using EnTTSharp.Entities;

namespace RogueEntity.Core.Positioning
{
    public readonly struct MapPositionChangedMarker
    {
        public readonly Position PreviousPosition;

        MapPositionChangedMarker(Position previousPosition)
        {
            this.PreviousPosition = previousPosition;
        }

        public static MapPositionChangedMarker From<TPosition>(TPosition p) 
            where TPosition : struct, IPosition<TPosition>
        {
            return new MapPositionChangedMarker(Position.From(p));
        }

        public static void Update<TKey>(IEntityViewControl<TKey> v, TKey k, Position previous)
            where TKey : struct, IEntityKey
        {
            if (v.GetComponent(k, out MapPositionChangedMarker marker))
            {
                // if we already have a change marker, check whether this change simply
                // undoes the previous change.
                if (marker.PreviousPosition == previous)
                {
                    v.RemoveComponent<MapPositionChangedMarker>(k);
                    return;
                }
                
                // subsequent changes in the same frame are ignored. We always preserve
                // the position at the start of the frame here.
                return;
            }
            
            v.AssignComponent(k, new MapPositionChangedMarker(previous));
        }

    }
}