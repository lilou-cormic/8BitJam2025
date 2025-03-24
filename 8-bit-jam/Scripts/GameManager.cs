using Godot;
using System.Collections.Generic;
using System.Linq;
using static Godot.TextServer;

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

    private Queue<int> _enemyQueue;

    [Export] PackedScene[] EnemyPrefabs;

    private TileMapLayer _tileMapLayer;

    private bool _IsGameOver = false;
    public static bool IsGameOver => _instance._IsGameOver;

    private bool _canRestart = false;

    private int _stepCount = 0;

    private int _enemyCount = 0;

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
        _enemyQueue = new Queue<int>();

        InitEnemyQueue();
    }

    public override void _ExitTree()
    {
        Player.PlayerMoved -= Player_PlayerMoved;
    }

    private void InitEnemyQueue()
    {
        _enemyQueue.Enqueue(0);
        _enemyQueue.Enqueue(0);
        _enemyQueue.Enqueue(0);
        _enemyQueue.Enqueue(0);
        _enemyQueue.Enqueue(0);
        _enemyQueue.Enqueue(0);
        _enemyQueue.Enqueue(0);
        _enemyQueue.Enqueue(0);
        _enemyQueue.Enqueue(0);
        _enemyQueue.Enqueue(1);

        _enemyQueue.Enqueue(0);
        _enemyQueue.Enqueue(0);
        _enemyQueue.Enqueue(0);
        _enemyQueue.Enqueue(0);
        _enemyQueue.Enqueue(1);
        _enemyQueue.Enqueue(0);
        _enemyQueue.Enqueue(0);
        _enemyQueue.Enqueue(0);
        _enemyQueue.Enqueue(0);
        _enemyQueue.Enqueue(1);

        _enemyQueue.Enqueue(0);
        _enemyQueue.Enqueue(0);
        _enemyQueue.Enqueue(1);
        _enemyQueue.Enqueue(1);
        _enemyQueue.Enqueue(0);
        _enemyQueue.Enqueue(0);
        _enemyQueue.Enqueue(1);
        _enemyQueue.Enqueue(1);
        _enemyQueue.Enqueue(0);
        _enemyQueue.Enqueue(2);
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

    private void TrySpawnEnemy()
    {
        int wave = (_enemyCount / 10);

        if (wave >= 10)
        {
            if (!IsEnemyThere(Maze.Entrance))
                SpawnEnemy(2);

            return;
        }

        if (_enemyQueue.Count == 0)
        {
            int[] tiers = GetTiers(wave);

            for (int i = 0; i < 10; i++)
            {
                int tier = tiers[GD.RandRange(0, tiers.Length - 1)];

                _enemyQueue.Enqueue(tier);
            }
        }

        if (_stepCount % (10 - wave) == 0)
        {

            if (_enemyQueue.Count > 0 && !IsEnemyThere(Maze.Entrance))
                SpawnEnemy(_enemyQueue.Dequeue());
        }
    }

    private int[] GetTiers(int wave)
    {
        switch (wave)
        {
            case 0:
                return new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 };

            case 1:
                return new int[] { 0, 0, 0, 0, 1 };

            case 2:
                return new int[] { 0, 0, 0, 0, 0, 1, 1, 1, 1, 2 };

            case 3:
                return new int[] { 0, 0, 0, 0, 1, 1, 1, 2 };

            case 4:
                return new int[] { 0, 0, 0, 1, 1, 1, 2, 2 };

            case 5:
                return new int[] { 0, 1, 2 };

            case 6:
                return new int[] { 0, 1, 1, 2, 2, 2 };

            case 7:
                return new int[] { 0, 1, 1, 1, 2, 2, 2, 2 };

            case 8:
                return new int[] { 0, 1, 1, 2, 2, 2, 2, 2, 2, 2 };

            case 9:
                return new int[] { 1, 2, 2, 2, 2, 2, 2, 2, 2, 2 };

            default:
                return new int[] { 2 };
        }
    }

    private void SpawnEnemy(int tier)
    {
        _enemyCount++;

        Enemy enemy = EnemyPrefabs[tier].Instantiate<Enemy>();
        AddChild(enemy);

        Enemies.Add(enemy);
    }

    public static bool CanMove(MazeLocation currentLocation, Direction direction, bool canDestroyWalls = false)
    {
        if (IsGameOver)
            return false;

        MazeLocation location = currentLocation.GetAdjacent(direction);

        if (location == _instance.Maze.Entrance)
            return false;

        if (canDestroyWalls && !_instance.Maze.GetCellType(location).IsBorder())
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
        _stepCount++;

        TrySpawnEnemy();
    }
}
