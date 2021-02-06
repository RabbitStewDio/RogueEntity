namespace RogueEntity.Core.Storage
{
    public interface IDataRepositoryFactory
    {
        public IDataRepository<TKey, TData> Create<TKey, TData>(string id);
    }
}
