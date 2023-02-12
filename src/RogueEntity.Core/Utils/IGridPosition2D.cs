namespace RogueEntity.Core.Utils;

public interface IGridPosition2D<TPosition> 
    where TPosition: IGridPosition2D<TPosition>
{
    public int X { get; }
    public int Y { get; }
        
    public void Deconstruct(out int x, out int y);
    public TPosition With(int x, int y);
    public TPosition Add(GridPosition2D d);
}