using RogueEntity.Api.Time;

namespace RogueEntity.Api.GameLoops
{
    public readonly struct WorldStepEventArgs
    {
        public readonly GameTimeState Time;

        public WorldStepEventArgs(GameTimeState time)
        {
            Time = time;
        }
    }
}
