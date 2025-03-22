using Godot;
using System;

public partial class Player : MazeExplorer
{
    private Node2D Right;
    private Node2D Down;
    private Node2D Left;
    private Node2D Up;

    public static event Action PlayerMoved;

    public override void _Ready()
    {
        base._Ready();

        Right = GetNode<Node2D>("Right");
        Down = GetNode<Node2D>("Down");
        Left = GetNode<Node2D>("Left");
        Up = GetNode<Node2D>("Up");
    }

    public override void _Process(double delta)
    {
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

        Right.Visible = GameManager.Maze.CanMove(Location, Direction.Right);
        Down.Visible = GameManager.Maze.CanMove(Location, Direction.Down);
        Left.Visible = GameManager.Maze.CanMove(Location, Direction.Left);
        Up.Visible = GameManager.Maze.CanMove(Location, Direction.Up);
    }

    protected override void OnMoved(Direction direction)
    {
        PlayerMoved?.Invoke();
    }
}
