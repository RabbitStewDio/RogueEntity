namespace RogueEntity.Core.Tests
{
    public static class ForcedConversions
    {
        /// <summary>
        ///   Fuck you FluentAssertions for not giving me an easy way of comparing objects as objects when they
        ///   implement IEnumerable in some way.
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public static object AsObject(this object o) => o;
    }
}