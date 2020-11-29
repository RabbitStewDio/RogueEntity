using System;
using System.Collections;
using System.Collections.Generic;
using RogueEntity.Core.Movement;
using RogueEntity.Core.Positioning.Grid;

namespace RogueEntity.Core.Tests.Movement.Pathfinding
{
    public static class ResultPathExtensions
    {
        public static int PathIndexOf(this List<(EntityGridPosition, IMovementMode)> pathItems, EntityGridPosition pos)
        {
            for (var index = 0; index < pathItems.Count; index++)
            {
                var p = pathItems[index];
                if (p.Item1 == pos)
                {
                    return index;
                }
            }

            return -1;
        }

        public static PathEnumerable ToPathEnumerable(this List<(EntityGridPosition, IMovementMode)> pathItems) => new PathEnumerable(pathItems);
        
        public readonly struct PathEnumerable : IEnumerable<(EntityGridPosition, IMovementMode)>
        {
            readonly List<(EntityGridPosition, IMovementMode)> pathItems;

            public PathEnumerable(List<(EntityGridPosition, IMovementMode)> pathItems)
            {
                this.pathItems = pathItems;
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            IEnumerator<(EntityGridPosition, IMovementMode)> IEnumerable<(EntityGridPosition, IMovementMode)>.GetEnumerator()
            {
                return GetEnumerator();
            }
            
            public PathEnumerator GetEnumerator() => new PathEnumerator(pathItems);
        }
        
        public struct PathEnumerator: IEnumerator<(EntityGridPosition, IMovementMode)>
        {
            readonly List<(EntityGridPosition, IMovementMode)> pathItems;
            int index;

            public PathEnumerator(List<(EntityGridPosition, IMovementMode)> pathItems)
            {
                this.pathItems = pathItems;
                this.index = pathItems.Count;
            }

            public bool MoveNext()
            {
                if (index < 0)
                {
                    return false;
                }

                index -= 1;
                return true;
            }

            public void Reset()
            {
                this.index = pathItems.Count;
            }

            public void Dispose()
            {
            }

            object IEnumerator.Current
            {
                get { return Current; }
            }

            public (EntityGridPosition, IMovementMode) Current
            {
                get
                {
                    if (index < 0 || index >= pathItems.Count)
                    {
                        throw new IndexOutOfRangeException("Iterator is out of range");
                    }

                    return pathItems[index];
                }
            }
        } 
    }
}