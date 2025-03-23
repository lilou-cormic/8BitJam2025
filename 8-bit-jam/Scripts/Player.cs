using Godot;
using System;
using System.Linq;
using static Godot.TextServer;

public partial class Player : MazeExplorer
{
    private Sprite2D Right;
    private Sprite2D Down;
    private Sprite2D Left;
    private Sprite2D Up;

    public static event Action PlayerMoved;
    public static event Action PlayerHPChanged;

    public int MaxHP = 3;

    public int HP = 3;

    private bool _isHurting = false;

    private bool _canGoBerserk = true;
    private bool _isBerserk = false;

    public override void _EnterTree()
    {
        base._EnterTree();

        ScoreManager.ScoreChanged += ScoreManager_ScoreChanged;
    }

    public override void _Ready()
    {
        base._Ready();

        HP = MaxHP;

        Right = GetNode<Sprite2D>("Right");
        Down = GetNode<Sprite2D>("Down");
        Left = GetNode<Sprite2D>("Left");
        Up = GetNode<Sprite2D>("Up");

        SelfModulate = ColorPalette.Brown;
    }

    public override void _Process(double delta)
    {
        SetDirection(Right, Direction.Right);
        SetDirection(Down, Direction.Down);
        SetDirection(Left, Direction.Left);
        SetDirection(Up, Direction.Up);

        if (GameManager.IsGameOver)
            return;

        if (Input.IsActionJustPressed("A-button"))
        {
            if (_canGoBerserk)
                _isBerserk = true;
        }
        else if (Input.IsActionJustPressed("B-button"))
        {
            _isBerserk = false;
        }

        if (Input.IsActionJustPressed("D-pad-right"))
        {
            TryMove(Direction.Right);
        }
        else if (Input.IsActionJustPressed("D-pad-down"))
        {
            TryMove(Direction.Down);
        }
        else if (Input.IsActionJustPressed("D-pad-left"))
        {
            TryMove(Direction.Left);
        }
        else if (Input.IsActionJustPressed("D-pad-up"))
        {
            TryMove(Direction.Up);
        }

        SetColor();
        SetBerserkIcon();
    }

    public override void _ExitTree()
    {
        ScoreManager.ScoreChanged -= ScoreManager_ScoreChanged;
    }

    private void SetColor()
    {
        if (_isBerserk)
            SelfModulate = ColorPalette.Blue;
        else if (_isHurting)
            SelfModulate = ColorPalette.Red;
        else
            SelfModulate = ColorPalette.Brown;
    }

    private void SetDirection(Sprite2D sprite, Direction direction)
    {
        sprite.Visible = CanMove(direction);

        sprite.SelfModulate = GetCellSelectColor(Location.GetAdjacent(direction));
    }

    private void SetBerserkIcon()
    {
        GetNode<Sprite2D>("%BerserkFrame").SelfModulate = _isBerserk ? ColorPalette.Brown : ColorPalette.White;
        GetNode<Sprite2D>("%BerserkAvailable").Visible = _canGoBerserk;
    }

    private Color GetCellSelectColor(MazeLocation location)
    {
        if (GameManager.IsEnemyThere(location))
            return ColorPalette.Red;

        if (GameManager.IsWallOrPillarThere(location))
            return ColorPalette.Blue;

        return ColorPalette.White;
    }

    public void Attack(Enemy enemy)
    {
        if (_isBerserk)
        {
            _isBerserk = false;
            _canGoBerserk = false;
        }

        enemy.Damage();

        OnMoved(Direction.None);
    }

    protected override bool CanMove(Direction direction)
    {
        if (Location.GetAdjacent(direction) == GameManager.Entrance)
            return false;

        return GameManager.CanMove(Location, direction, _isBerserk);
    }

    public async void Damage()
    {
        if (_isBerserk)
        {
            _isBerserk = false;
            _canGoBerserk = false;

            GetNode<AudioStreamPlayer2D>("NoHurtSoundPlayer").Play();
        }
        else
        {
            HP--;

            PlayerHPChanged?.Invoke();

            if (HP <= 0)
                GameManager.GameOver();

            GetNode<AudioStreamPlayer2D>("HurtSoundPlayer").Play();

            _isHurting = true;
            SelfModulate = ColorPalette.Red;
            await ToSignal(GetTree().CreateTimer(0.2f), SceneTreeTimer.SignalName.Timeout);
            _isHurting = false;
        }
    }

    protected override bool TryMove(Direction direction)
    {
        MazeLocation location = Location.GetAdjacent(direction);

        if (GameManager.IsEnemyThere(location))
        {
            Attack(GameManager.Enemies.First(x => x.Location == location));
            return true;
        }

        if (_isBerserk && GameManager.IsWallOrPillarThere(location))
        {
            GameManager.DestroyWall(location);
            _isBerserk = false;
            _canGoBerserk = false;
        }

        return base.TryMove(direction);
    }

    protected override void OnMoved(Direction direction)
    {
        PlayerMoved?.Invoke();
    }

    private void ScoreManager_ScoreChanged()
    {
        if (ScoreManager.Score % 500 == 0)
            _canGoBerserk = true;
    }
}
