using System.Collections.Generic;

namespace RogueEntity.Core.Tests
{
    public static class ForcedConversions
    {
        static void X()
        {
            new List<int>().Find(x => x == 0);
        }
        
        /// <summary>
        ///   Fuck you FluentAssertions for not giving me an easy way of comparing objects as objects when they
        ///   implement IEnumerable in some way.
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public static object AsObject(this object o) => o;
    }
}