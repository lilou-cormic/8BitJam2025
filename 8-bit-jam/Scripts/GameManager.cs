using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class GameManager : Node
{
    private static GameManager _instance;

    private MazeGrid Maze;

    public static MazeLocation Entrance => _instance?.Maze?.Entrance;

    private static Vector2I LeftBorder = new(20, 1);
    private static Vector2I TopBorder = new(19, 2);
    private static Vector2I RightBorder = new(18, 1);
    private static Vector2I BottomBorder = new(19, 0);
    private static Vector2I Corner = new(-1, -1);
    private static Vector2I HorizontalWall = new(48, 10);
    private static Vector2I VerticalWall = new(48, 10);
    private static Vector2I Intersection = new(48, 10);
    private static Vector2I Empty = new(-1, -1);

    private static Vector2I BrokenWall = new(2, 0);

    public static Player Player { get; private set; }

    public static List<Enemy> Enemies { get; private set; }

    [Export] PackedScene EnemyPrefab;

    private TileMapLayer _tileMapLayer;

    private bool _IsGameOver = false;
    public static bool IsGameOver => _instance._IsGameOver;

    private bool _canRestart = false;

    private int count = 0;

    public override void _EnterTree()
    {
        Player.PlayerMoved += Player_PlayerMoved;
    }

    public override void _Ready()
    {
        _instance = this;

        ScoreManager.Reset();

        Maze = MazeGrid.CreateMaze();

        Player = GetNode<Player>("%Player");
        Player.SetLocation(Maze.PlayerStartLocation);

        _tileMapLayer = GetNode<TileMapLayer>("%TileMapLayer");
        _tileMapLayer.SelfModulate = ColorPalette.Gray;

        for (int row = 0; row < MazeGrid.RowCount; row++)
        {
            for (int col = 0; col < MazeGrid.ColumnCount; col++)
            {
                SetCell(col, row);
            }
        }

        Enemies = new List<Enemy>();

        SpawnEnemy();
    }

    public override void _ExitTree()
    {
        Player.PlayerMoved -= Player_PlayerMoved;
    }

    private void SetCell(int col, int row, Vector2I? atlasCoords = null)
    {
        if (atlasCoords == null)
            atlasCoords = GetAtlasCoords(Maze.GetCellType(col, row));

        _tileMapLayer.SetCell(new Vector2I(col, row), 1, atlasCoords);
        _tileMapLayer.SelfModulate = Colors.White;
        _tileMapLayer.SelfModulate = ColorPalette.Gray;
    }

    public static void DestroyWall(MazeLocation location)
    {
        _instance.GetNode<AudioStreamPlayer2D>("%DestroyWallSoundPlayer").Play();

        _instance.Maze.SetCellType(location, CellType.Space);

        _instance.SetCell(location.Column, location.Row, BrokenWall);
    }

    private void SpawnEnemy()
    {
        if (IsEnemyThere(Maze.Entrance))
            return;

        Enemy enemy = EnemyPrefab.Instantiate<Enemy>();
        AddChild(enemy);

        Enemies.Add(enemy);
    }

    public static bool CanMove(MazeLocation currentLocation, Direction direction, bool canDestroyWalls = false)
    {
        if (IsGameOver)
            return false;

        if (canDestroyWalls && !_instance.Maze.GetCellType(currentLocation.GetAdjacent(direction)).IsBorder())
            return true;

        return _instance.Maze.CanMove(currentLocation, direction);
    }

    public static async void GameOver()
    {
        _instance._IsGameOver = true;
        _instance.GetNode<ColorRect>("%GameOverPanel").Visible = true;
        _instance.GetNode<AudioStreamPlayer>("%MusicPlayer").Stop();
        _instance.GetNode<AudioStreamPlayer2D>("%GameOverSoundPlayer").Play();

        await _instance.ToSignal(_instance.GetTree().CreateTimer(1f), SceneTreeTimer.SignalName.Timeout);
        _instance._canRestart = true;
    }

    public override void _Process(double delta)
    {
        if (_IsGameOver && _canRestart && Input.IsAnythingPressed())
            _instance.GetTree().ChangeSceneToFile(@"res://Scenes/Menu.tscn");
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

    public static bool IsWallOrPillarThere(MazeLocation location)
    {
        return _instance.Maze.GetCellType(location).IsWallOrPillar();
    }

    private void Player_PlayerMoved()
    {
        count++;

        if (count % 10 == 0)
            SpawnEnemy();
    }
}
