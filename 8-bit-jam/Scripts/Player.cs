using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public partial class Player : Node2D
{
    public MazeLocation Location { get; private set; }

    private Node2D Right;
    private Node2D Down;
    private Node2D Left;
    private Node2D Up;

    public override void _Ready()
    {
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

    public void SetLocation(MazeLocation location)
    {
        Location = location;
        Position = GameManager.GetPosition(Location);
    }

    private void TryMove(Direction direction)
    {
        if (GameManager.Maze.CanMove(Location, direction))
        {
            switch (direction)
            {
                case Direction.Right:
                    SetLocation(new MazeLocation(Location.Column + 1, Location.Row));
                    return;

                case Direction.Down:
                    SetLocation(new MazeLocation(Location.Column, Location.Row + 1));
                    return;

                case Direction.Left:
                    SetLocation(new MazeLocation(Location.Column - 1, Location.Row));
                    return;

                case Direction.Up:
                    SetLocation(new MazeLocation(Location.Column, Location.Row - 1));
                    return;
            }
        }
    }
}
