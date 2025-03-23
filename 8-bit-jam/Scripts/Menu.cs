using Godot;
using System;

public partial class Menu : Node2D
{
    private bool _isCreditsSelected = false;

    public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed("Select-button") || Input.IsActionJustPressed("D-pad-down") || Input.IsActionJustPressed("D-pad-up"))
        {
            _isCreditsSelected = !_isCreditsSelected;

            GetNode<Sprite2D>("%StartSelector").Visible = !_isCreditsSelected;
            GetNode<Sprite2D>("%CreditsSelector").Visible = _isCreditsSelected;
        }
        else if (Input.IsActionJustPressed("Start-button") || Input.IsActionJustPressed("A-button"))
        {
            if (_isCreditsSelected)
                GetTree().ChangeSceneToFile(@"res://Scenes/Credits.tscn");
            else
                GetTree().ChangeSceneToFile(@"res://Scenes/Main.tscn");
        }
    }
}
