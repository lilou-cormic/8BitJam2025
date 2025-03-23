using Godot;

public partial class BerserkCountText : Label
{
    public override void _EnterTree()
    {
        Player.BerserkCountChanged += Player_BerserkCountChanged;
    }

    public override void _Ready()
    {
        SetText();
    }

    public override void _ExitTree()
    {
        Player.BerserkCountChanged -= Player_BerserkCountChanged;
    }

    private void SetText()
    {
        Text = "x" + (GameManager.Player?.BerserkCount.ToString() ?? "1");
    }

    private void Player_BerserkCountChanged()
    {
        SetText();
    }
}
