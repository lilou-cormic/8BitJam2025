using Godot;

public partial class Enemy : MazeExplorer
{
    private bool _isDead = false;

    private Direction _direction = Direction.Up;

    public override void _EnterTree()
    {
        Player.PlayerMoved += Player_PlayerMoved;
    }

    public override void _Ready()
    {
        base._Ready();

        SetLocation(GameManager.Maze.Entrance);
    }

    public override void _ExitTree()
    {
        Player.PlayerMoved -= Player_PlayerMoved;
    }

    protected override void OnMoved(Direction direction)
    {
        _direction = direction;
    }

    private void Player_PlayerMoved()
    {
        if (_isDead)
            return;

        Direction direction1 = (Direction)(((int)_direction + 1) % 4);

        Direction direction2 = _direction;

        Direction direction3 = (Direction)(((int)_direction + 3) % 4);

        Direction direction4 = (Direction)(((int)_direction + 2) % 4);

        if (CanMove(direction1) && !HasExplored(Location.GetAdjacent(direction1)))
        {
            Move(direction1);
            return;
        }

        if (CanMove(direction2) && !HasExplored(Location.GetAdjacent(direction2)))
        {
            Move(direction2);
            return;
        }

        if (CanMove(direction3) && !HasExplored(Location.GetAdjacent(direction3)))
        {
            Move(direction3);
            return;
        }

        if (CanMove(direction1) && CanMove(direction2) && CanMove(direction3))
        {
            var dirs = new Direction[] { direction1, direction1, direction1, direction2, direction3 };

            Move(dirs[GD.RandRange(0, dirs.Length - 1)]);
            return;
        }

        if (CanMove(direction1) && CanMove(direction2))
        {
            var dirs = new Direction[] { direction1, direction1, direction2 };

            Move(dirs[GD.RandRange(0, dirs.Length - 1)]);
            return;
        }

        if (CanMove(direction1) && CanMove(direction3))
        {
            var dirs = new Direction[] { direction1, direction1, direction3 };

            Move(dirs[GD.RandRange(0, dirs.Length - 1)]);
            return;
        }

        if (CanMove(direction2) && CanMove(direction3))
        {
            var dirs = new Direction[] { direction2, direction3, direction3 };

            Move(dirs[GD.RandRange(0, dirs.Length - 1)]);
            return;
        }

        if (TryMove(direction1))
        {
            return;
        }

        if (TryMove(direction2))
        {
            return;
        }

        if (TryMove(direction3))
        {
            return;
        }

        if (TryMove(direction4))
        {
            return;
        }
    }
}
