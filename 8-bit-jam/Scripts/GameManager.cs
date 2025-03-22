using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class GameManager : Node
{
    private static GameManager _instance;

    public static MazeGrid Maze { get; private set; }

    private static Vector2I LeftBorder = new(20, 1);
    private static Vector2I TopBorder = new(19, 2);
    private static Vector2I RightBorder = new(18, 1);
    private static Vector2I BottomBorder = new(19, 0);
    private static Vector2I Corner = new(-1, -1);
    private static Vector2I HorizontalWall = new(48, 10);
    private static Vector2I VerticalWall = new(48, 10);
    private static Vector2I Intersection = new(48, 10);
    private static Vector2I Empty = new(-1, -1);

    public static Player Player { get; private set; }

    public static List<Enemy> Enemies { get; private set; }

    [Export] PackedScene EnemyPrefab;

    int count = 0;

    public override void _EnterTree()
    {
        Player.PlayerMoved += Player_PlayerMoved;
    }

    public override void _Ready()
    {
        _instance = this;

        Maze = MazeGrid.CreateMaze();

        Player = GetNode<Player>("%Player");
        Player.SetLocation(Maze.PlayerStartLocation);

        TileMapLayer tileMapLayer = GetNode<TileMapLayer>("%TileMapLayer");

        for (int row = 0; row < MazeGrid.RowCount; row++)
        {
            for (int col = 0; col < MazeGrid.ColumnCount; col++)
            {
                tileMapLayer.SetCell(new Vector2I(col, row), 1, GetAtlasCoords(Maze.GetCell(col, row).CellType));
            }
        }

        Enemies = new List<Enemy>();

        SpawnEnemy();
    }

    public override void _ExitTree()
    {
        Player.PlayerMoved -= Player_PlayerMoved;
    }

    private void SpawnEnemy()
    {
        if (IsEnemyThere(Maze.Entrance))
            return;

        Enemy enemy = EnemyPrefab.Instantiate<Enemy>();
        AddChild(enemy);

        Enemies.Add(enemy);
    }

    public static void GameOver()
    {
        _instance.GetTree().ChangeSceneToFile(@"res://Scenes/Main.tscn");
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

    public static Vector2 GetPosition(MazeLocation location)
    {
        return new Vector2(location.Column * 16 - 8, location.Row * 16 + 24);
    }

    public static bool IsEnemyThere(MazeLocation location)
    {
        return Enemies.Any(x => x.Location == location);
    }

    private void Player_PlayerMoved()
    {
        count++;

        if (count % 10 == 0)
            SpawnEnemy();
    }
}
