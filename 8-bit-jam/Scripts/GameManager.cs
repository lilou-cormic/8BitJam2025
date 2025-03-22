using Godot;

public partial class GameManager : Node
{
    static int sourceId = 1;
    static Vector2I LeftBorder = new(20, 1);
    static Vector2I TopBorder = new(19, 2);
    static Vector2I RightBorder = new(18, 1);
    static Vector2I BottomBorder = new(19, 0);
    static Vector2I Corner = new(-1, -1);
    static Vector2I HorizontalWall = new(48, 10);
    static Vector2I VerticalWall = new(48, 10);
    static Vector2I Intersection = new(48, 10);
    static Vector2I Empty = new(-1, -1);

    public override void _Ready()
    {
        var maze = MazeGrid.CreateMaze();


        TileMapLayer tileMapLayer = GetNode<TileMapLayer>("%TileMapLayer");

        for (int row = 0; row < maze.RowCount; row++)
        {
            for (int col = 0; col < maze.ColumnCount; col++)
            {
                tileMapLayer.SetCell(new Vector2I(col, row), sourceId, GetAtlasCoords(maze.GetCell(col, row).CellType));
            }
        }
    }

    private Vector2I GetAtlasCoords(CellType cellType)
    {
        switch (cellType)
        {
            case CellType.LeftBorder:
                return LeftBorder;
            case CellType.TopBorder:
                return TopBorder;
            case CellType.RightBorder:
                return RightBorder;
            case CellType.BottomBorder:
                return BottomBorder;
            case CellType.CornerBorder:
                return Corner;
            case CellType.HorizontalWall:
                return HorizontalWall;
            case CellType.VerticalWall:
                return VerticalWall;
            case CellType.Intersection:
                return Intersection;
            case CellType.Space:
                return Empty;
            default:
                return Empty;
        }
    }
}
