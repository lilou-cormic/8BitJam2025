using Godot;

public partial class Credits : Node2D
{
    public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed("Start-button") || Input.IsActionJustPressed("A-button") || Input.IsActionJustPressed("B-button"))
        {
            GetTree().ChangeSceneToFile(@"res://Scenes/Menu.tscn");
        }
    }
}
