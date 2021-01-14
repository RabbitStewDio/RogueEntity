using System;

namespace RogueEntity.Core.Players
{
    public static class PlayerIds
    {
        public static (PlayerTag, TEntity) GetOrCreate<TEntity>(IPlayerService<TEntity> ps)
        {
            return ps.GetOrCreate(SinglePlayer);
        }
        
        /// <summary>
        ///   A fixed UUID for identifying a single player. This simplifies player instantiation when
        ///   you don't need the hassle of managing multiple player entities in the same game or
        ///   (heaven forbid) have to persist them on a server. 
        /// </summary>
        public static readonly Guid SinglePlayer = new Guid("a6e4cbef-2c21-4668-a29f-dba17658adf0");
    }
}
