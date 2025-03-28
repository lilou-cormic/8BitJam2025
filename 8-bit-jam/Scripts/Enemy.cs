﻿using Godot;

public partial class Enemy : MazeExplorer
{
    [Export] int Strength = 1;
    [Export] int Points = 100;
    [Export] int TurnsToMove = 2;
    [Export] PackedScene PointsPrefab;
    [Export] PackedScene DoublePtsPrefab;

    private bool _isDead = false;

    private int _turnsCount = 0;

    public bool CouldMove => _turnsCount >= TurnsToMove - 1;

    private Direction _direction = Direction.Up;

    public override void _EnterTree()
    {
        Player.PlayerMoved += Player_PlayerMoved;
    }

    public override void _ExitTree()
    {
        Player.PlayerMoved -= Player_PlayerMoved;
    }

    public void Attack()
    {
        GameManager.Player.Damage(Strength);
    }

    public async void Damage(bool doublePoints = false)
    {
        if (_isDead)
            return;

        GetNode<AudioStreamPlayer2D>(doublePoints ? "BigHurtSoundPlayer" : "HurtSoundPlayer").Play();

        ScoreManager.Add(Points * (doublePoints ? 2 : 1));

        _isDead = true;

        Points points = (doublePoints ? DoublePtsPrefab : PointsPrefab).Instantiate<Points>();
        points.GlobalPosition = GlobalPosition;
        GetParent().AddChild(points);

        GameManager.Enemies.Remove(this);

        SelfModulate = ColorPalette.Red;
        await ToSignal(GetTree().CreateTimer(0.2f), SceneTreeTimer.SignalName.Timeout);

        QueueFree();
    }

    protected override void OnMoved(Direction direction)
    {
        _direction = direction;
        _turnsCount = 0;
    }

    private bool IsPlayerThere(Direction direction)
    {
        return GameManager.Player.Location == Location.GetAdjacent(direction);
    }

    protected override bool CanMove(Direction direction)
    {
        if (GameManager.IsEnemyThere(Location.GetAdjacent(direction)))
            return false;

        return base.CanMove(direction);
    }

    protected override bool TryMove(Direction direction)
    {
        if (_turnsCount >= TurnsToMove)
        {
            if (IsPlayerThere(direction))
            {
                Attack();
                OnMoved(direction);
                return true;
            }

            return base.TryMove(direction);
        }

        return false;
    }

    private void Player_PlayerMoved()
    {
        if (_isDead)
            return;

        _turnsCount++;

        Direction direction1 = (Direction)(((int)_direction + 1) % 4);

        Direction direction2 = _direction;

        Direction direction3 = (Direction)(((int)_direction + 3) % 4);

        Direction direction4 = (Direction)(((int)_direction + 2) % 4);

        if (IsPlayerThere(direction1))
        {
            TryMove(direction1);
            return;
        }

        if (IsPlayerThere(direction2))
        {
            TryMove(direction2);
            return;
        }

        if (IsPlayerThere(direction3))
        {
            TryMove(direction3);
            return;
        }

        if (CanMove(direction1) && !HasExplored(Location.GetAdjacent(direction1)))
        {
            TryMove(direction1);
            return;
        }

        if (CanMove(direction2) && !HasExplored(Location.GetAdjacent(direction2)))
        {
            TryMove(direction2);
            return;
        }

        if (CanMove(direction3) && !HasExplored(Location.GetAdjacent(direction3)))
        {
            TryMove(direction3);
            return;
        }

        if (CanMove(direction1) && CanMove(direction2) && CanMove(direction3))
        {
            var dirs = new Direction[] { direction1, direction1, direction1, direction2, direction3 };

            TryMove(dirs[GD.RandRange(0, dirs.Length - 1)]);
            return;
        }

        if (CanMove(direction1) && CanMove(direction2))
        {
            var dirs = new Direction[] { direction1, direction1, direction2 };

            TryMove(dirs[GD.RandRange(0, dirs.Length - 1)]);
            return;
        }

        if (CanMove(direction1) && CanMove(direction3))
        {
            var dirs = new Direction[] { direction1, direction1, direction3 };

            TryMove(dirs[GD.RandRange(0, dirs.Length - 1)]);
            return;
        }

        if (CanMove(direction2) && CanMove(direction3))
        {
            var dirs = new Direction[] { direction2, direction3, direction3 };

            TryMove(dirs[GD.RandRange(0, dirs.Length - 1)]);
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
