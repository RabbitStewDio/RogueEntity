namespace RogueEntity.Core.Storage
{
    public interface IStorageLocationService
    {
        public string ContentLocation { get; }
        public string ConfigurationLocation { get; }
    }
}
