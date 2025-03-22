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

    public int MaxHP = 1;

    public int HP = 1;

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

        if (Input.IsActionJustPressed("ui_right"))
        {
            TryMove(Direction.Right);
        }
        else if (Input.IsActionJustPressed("ui_down"))
        {
            TryMove(Direction.Down);
        }
        else if (Input.IsActionJustPressed("ui_left"))
        {
            TryMove(Direction.Left);
        }
        else if (Input.IsActionJustPressed("ui_up"))
        {
            TryMove(Direction.Up);
        }
    }

    private void SetDirection(Sprite2D sprite, Direction direction)
    {
        sprite.Visible = GameManager.Maze.CanMove(Location, direction);

        sprite.SelfModulate = GameManager.IsEnemyThere(Location.GetAdjacent(direction)) ? ColorPalette.Red : ColorPalette.White;
    }

    public void Attack(Enemy enemy)
    {
        enemy.Damage();
    }

    protected override bool CanMove(Direction direction)
    {
        if (Location.GetAdjacent(direction) == GameManager.Maze.Entrance)
            return false;

        return base.CanMove(direction);
    }

    public void Damage()
    {
        HP--;

        if (HP <= 0)
            GameManager.GameOver();
    }

    private bool IsEnemyThere(Direction direction)
    {
        return GameManager.IsEnemyThere(Location.GetAdjacent(direction));
    }

    protected override bool TryMove(Direction direction)
    {
        if (IsEnemyThere(direction))
        {
            Attack(GameManager.Enemies.First(x => x.Location == Location.GetAdjacent(direction)));
            return true;
        }

        return base.TryMove(direction);
    }

    protected override void OnMoved(Direction direction)
    {
        PlayerMoved?.Invoke();
    }
}
