using System;

public static class ScoreManager
{
    public static int TopScore { get; private set; }

    public static int Score { get; private set; }

    public static event Action<int> ScoreChanged;

    public static void Add(int points)
    {
        Score += points;

        if (Score > TopScore)
            TopScore = Score;

        ScoreChanged?.Invoke(points);
    }

    public static void Reset()
    {
        Score = 0;
    }
}
