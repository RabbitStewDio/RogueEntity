namespace RogueEntity.Core.Sensing.Common
{
    public static class SenseDirectionExtensions
    {
        public static SenseDataFlags WithObstructed(this SenseDataFlags f, SenseDataFlags other)
        {
            return f | (other & SenseDataFlags.Obstructed);
        }

        /// <summary>
        ///   Marks all edges that are shared by both directions.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static SenseDirection Intersect(this SenseDirection a, SenseDirection b)
        {
            return a & b;
        }
    }
}