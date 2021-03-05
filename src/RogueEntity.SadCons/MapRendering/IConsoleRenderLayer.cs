using RogueEntity.Api.Utils;
using RogueEntity.Core.Positioning;

namespace RogueEntity.SadCons.MapRendering
{
    public interface IConsoleRenderLayer
    {
        public Optional<ConsoleRenderData> Get(Position p);
    }
}
