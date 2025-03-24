using Godot;
using System;
using System.Linq;

public partial class Player : MazeExplorer
{
    private Sprite2D Right;
    private Sprite2D Down;
    private Sprite2D Left;
    private Sprite2D Up;

    public static event Action PlayerMoved;
    public static event Action PlayerHPChanged;
    public static event Action BerserkCountChanged;

    public int MaxHP = 6;

    public int HP = 6;

    private bool _isHurting = false;

    private bool CanGoBerserk => BerserkCount > 0;
    private bool _isBerserk = false;

    private int _ptsForBerserk = 0;

    public int BerserkCount { get; private set; }

    public override void _EnterTree()
    {
        base._EnterTree();

        ScoreManager.ScoreChanged += ScoreManager_ScoreChanged;
    }

    public override void _Ready()
    {
        base._Ready();

        HP = MaxHP;
        BerserkCount = 1;

        Right = GetNode<Sprite2D>("Right");
        Down = GetNode<Sprite2D>("Down");
        Left = GetNode<Sprite2D>("Left");
        Up = GetNode<Sprite2D>("Up");

        SelfModulate = ColorPalette.Brown;
    }

    public override void _Process(double delta)
    {
        SetCellSelector(Right, Direction.Right);
        SetCellSelector(Down, Direction.Down);
        SetCellSelector(Left, Direction.Left);
        SetCellSelector(Up, Direction.Up);

        if (GameManager.IsGameOver)
            return;

        if (Input.IsActionJustPressed("A-button"))
        {
            if (CanGoBerserk)
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

    private void SetCellSelector(Sprite2D sprite, Direction direction)
    {
        sprite.Visible = CanMove(direction);

        sprite.SelfModulate = GetCellSelectColor(Location.GetAdjacent(direction));
    }

    private void SetBerserkIcon()
    {
        GetNode<Sprite2D>("%BerserkFrame").SelfModulate = _isBerserk ? ColorPalette.Brown : ColorPalette.White;
        GetNode<Sprite2D>("%BerserkAvailable").Visible = CanGoBerserk;
    }

    private Color GetCellSelectColor(MazeLocation location)
    {
        if (GameManager.IsEnemyThere(location))
            return _isBerserk ? ColorPalette.Blue : ColorPalette.Brown;

        if (GameManager.IsDangerNear(location))
            return _isBerserk ? ColorPalette.Blue : ColorPalette.Red;

        if (GameManager.IsWallOrPillarThere(location))
            return ColorPalette.Blue;

        return ColorPalette.White;
    }

    public void Attack(Enemy enemy)
    {
        enemy.Damage(_isBerserk);

        if (_isBerserk)
            UseBerserkEnergy();

        OnMoved(Direction.None);
    }

    protected override bool CanMove(Direction direction)
    {
        return GameManager.CanMove(Location, direction, _isBerserk);
    }

    public async void Damage(int damage)
    {
        if (_isBerserk)
        {
            UseBerserkEnergy();

            GetNode<AudioStreamPlayer2D>("NoHurtSoundPlayer").Play();
        }
        else
        {
            HP -= damage;

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
            UseBerserkEnergy();
        }

        bool moved = base.TryMove(direction);

        if (moved)
            GetNode<AudioStreamPlayer2D>("WalkSoundPlayer").Play();

        return moved;
    }

    private void UseBerserkEnergy()
    {
        _isBerserk = false;

        if (BerserkCount > 0)
            BerserkCount--;

        BerserkCountChanged?.Invoke();
    }

    protected override void OnMoved(Direction direction)
    {
        PlayerMoved?.Invoke();
    }

    private void ScoreManager_ScoreChanged(int points)
    {
        _ptsForBerserk += points;

        if (_ptsForBerserk >= 500)
        {
            for (; _ptsForBerserk >= 500; _ptsForBerserk -= 500)
            {
                if (BerserkCount < 9)
                    BerserkCount++;
            }

            BerserkCountChanged?.Invoke();

            GetNode<AudioStreamPlayer2D>("BerserkGetSoundPlayer").Play();
        }
    }
}
