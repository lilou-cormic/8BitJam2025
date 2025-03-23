using Godot;

public partial class Points : Node2D
{
	private void OnTimeout()
    {
        QueueFree();
    }
}
