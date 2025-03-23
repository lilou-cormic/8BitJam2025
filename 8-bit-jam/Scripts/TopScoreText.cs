using Godot;

public partial class TopScoreText : Label
{
    public override void _EnterTree()
    {
        ScoreManager.ScoreChanged += ScoreManager_ScoreChanged;
    }

    public override void _Ready()
    {
        SetText();
    }

    public override void _ExitTree()
    {
        ScoreManager.ScoreChanged -= ScoreManager_ScoreChanged;
    }

    private void SetText()
    {
        Text = ScoreManager.TopScore.ToString("000000");
    }

    private void ScoreManager_ScoreChanged()
    {
        SetText();
    }
}
