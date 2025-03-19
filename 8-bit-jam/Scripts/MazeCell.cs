public class MazeCell
{
    public CellType CellType { get; }

    public bool IsWalkable { get { return CellType == CellType.Space; } }

    public MazeCell(CellType cellType)
    {
        CellType = cellType;
    }
}