using Godot;

public partial class MazeExplorer : Sprite2D
{
    public MazeLocation Location { get; private set; }

    private bool[,] _explored = new bool[MazeGrid.ColumnCount, MazeGrid.RowCount];

    protected virtual bool CanMove(Direction direction)
    {
        return GameManager.CanMove(Location, direction);
    }

    protected bool HasExplored(MazeLocation location)
    {
        return _explored[location.Column, location.Row];
    }

    public void SetLocation(MazeLocation location)
    {
        Location = location;
        Position = GameManager.GetPosition(Location);

        _explored[Location.Column, location.Row] = true;
    }

    protected void Move(Direction direction)
    {
        SetLocation(Location.GetAdjacent(direction));

        OnMoved(direction);
    }

    protected virtual void OnMoved(Direction direction)
    {
    }

    protected virtual bool TryMove(Direction direction)
    {
        if (CanMove(direction))
        {
            Move(direction);
            return true;
        }

        return false;
    }
}
