using GoRogue;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Utils;

namespace RogueEntity.Core.Sensing.Vision
{
    public static class VisibilityFunctions
    {
        /// <summary>
        ///    Returns a percentage value of how detectable a given spot is.
        ///    Return Percentage.Full if the cell is clearly visible, 
        ///    Percentage.Empty if the cell is shrouded in mystery or anything
        ///    in between to signal partial visibility. 
        /// </summary>
        public delegate Percentage CanSenseAt<TGameContext, TActorId>(TActorId v,
                                                                      TGameContext context,
                                                                      in Position actorOrigin,
                                                                      in Position pos);

        /// <summary>
        ///   Smell, hearing and other senses are not affected by the environment
        ///   but are affected by distance. 
        /// </summary>
        /// <param name="actor"></param>
        /// <param name="context"></param>
        /// <param name="radius"></param>
        /// <param name="pos"></param>
        /// <returns></returns>
        public static Percentage SenseAll<TGameContext, TActorId>(TActorId actor,
                                                                  TGameContext context,
                                                                  in Position actorOrigin,
                                                                  in Position pos)
        {
            return Percentage.Full;
        }

        public static CanSenseAt<TGameContext, TActorId> SenseByDistance<TGameContext, TActorId>(float maxRadius)
        {
            Percentage SenseByDistance(TActorId actor,
                                       TGameContext context,
                                       in Position actorOrigin,
                                       in Position pos)
            {
                float dx = (float) (actorOrigin.X - pos.X);
                float dy = (float) (actorOrigin.Y - pos.Y);
                float dz = (float) (actorOrigin.Z - pos.Z);
                var radius = DistanceCalculation.EUCLIDEAN.Calculate(dx, dy, dz) / maxRadius;
                return Percentage.Of(1 - radius);
            }

            return SenseByDistance;
        }

        /// <summary>
        ///   For vision, sense quality depends on how well-lit the target spot is.
        ///   Actors cannot see well into dark corners.
        ///
        ///   Vision is not affected by distance (at least for the ranges we deal
        ///   with) and as such the radius does not come into play.
        /// </summary>
        /// <param name="r"></param>
        /// <param name="context"></param>
        /// <param name="radius"></param>
        /// <param name="pos"></param>
        /// <returns></returns>
        public static Percentage VisionSense<TGameContext, TActorId>(TActorId r, TGameContext context,
                                                                     in Position actorOrigin,
                                                                     in Position pos)
            where TGameContext: ISenseContextProvider
        {
            return context.SenseContext.BrightnessMap[pos.GridX, pos.GridY, pos.GridZ];
        }

        public static Percentage VisionBlock<TGameContext, TActorId>(TActorId r, 
                                                                     TGameContext context,
                                                                     in Position actorOrigin,
                                                                     in Position p)
            where TGameContext : ISenseContextProvider
        {
            return context.SenseContext.SensePropertyMap[p.GridX, p.GridY, p.GridZ].blocksLight;
        }

        public static Percentage HeatBlock<TGameContext, TActorId>(TActorId r, TGameContext context,
                                                                   in Position actorOrigin,
                                                                   in Position p)
            where TGameContext : ISenseContextProvider
        {
            return context.SenseContext.SensePropertyMap[p.GridX, p.GridY, p.GridZ].blocksHeat;
        }

        public static Percentage SoundBlock<TGameContext, TActorId>(TActorId r, TGameContext context,
                                                                    in Position actorOrigin,
                                                                    in Position p)
            where TGameContext : ISenseContextProvider
        {
            return context.SenseContext.SensePropertyMap[p.GridX, p.GridY, p.GridZ].blocksSound;
        }
    }
}