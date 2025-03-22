using Godot;

public partial class Heart: Node2D
{
    [Export] int Number = 1;

    public override void _EnterTree()
    {
        Player.PlayerHPChanged += Player_PlayerHPChanged;
    }

    public override void _ExitTree()
    {
        Player.PlayerHPChanged -= Player_PlayerHPChanged;
    }

    private void Player_PlayerHPChanged()
    {
        GetNode<Sprite2D>("HeartFull").Visible = GameManager.Player.HP >= Number;
    }
}
