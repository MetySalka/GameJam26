using System;
using System.Linq;

public enum EnemyKind { Pike, Swordfish, Bubble, Pufferfish, Crab }

public sealed record EnemySpawnRule(
    int FromLevel, EnemyKind Enemy, int MaxCount,
    float MinSeconds, float MaxSeconds,
    int MinBatch = 1, int MaxBatch = 1, float InitialDelay = 0f);

public static class EnemySpawnTable
{
    // Levels are zero-based. The latest matching FromLevel wins for each enemy.
    // Intervals and initial delays are seconds. MaxCount = 0 disables new spawns.
    // MaxCount includes fish outside the viewport; batch size counts individual fish.
    // Add a row to override an enemy at a new level. Unlisted higher levels reuse the last row.
    // Before an enemy's first configured level, it is disabled automatically.
    private static readonly EnemySpawnRule[] Rows =
    {
        // Level  Enemy                 Max   Min sec Max sec  Batch min/max  First delay
        new(0,    EnemyKind.Pike,        10,   3f,     8f,      1, 1,          7f),
        new(1,    EnemyKind.Pike,         0,   3f,     8f),

        new(1,    EnemyKind.Swordfish,   10,   0.2f,   2f,      2, 4),
        new(2,    EnemyKind.Swordfish,   0,   0.2f,   2f),

        new(0,    EnemyKind.Bubble,       10,   0.5f,     7f),
        new(1,    EnemyKind.Bubble,       0,   0.5f,     4f),
        new(2,    EnemyKind.Bubble,      0,   0.3f,     3f),

        new(0,    EnemyKind.Pufferfish,   0,   6f,    10f,      1, 1,          5f),
        new(1,    EnemyKind.Pufferfish,   7,   2f,    4f,      1, 1,          5f),
        new(2,    EnemyKind.Pufferfish,   0,   2f,    5f, 1,1,          5f),

        // Crab only appears once the player reaches land (level 2, the beach).
        new(2,    EnemyKind.Crab,         4,   3f,     7f,      1, 1,          2f),

    };

    static EnemySpawnTable()
    {
        foreach (var row in Rows)
            if (row.FromLevel < 0 || row.MaxCount < 0 || !float.IsFinite(row.MinSeconds)
                || !float.IsFinite(row.MaxSeconds) || row.MinSeconds <= 0 || row.MaxSeconds < row.MinSeconds
                || row.MinBatch < 1 || row.MaxBatch < row.MinBatch
                || !float.IsFinite(row.InitialDelay) || row.InitialDelay < 0)
                throw new InvalidOperationException($"Invalid spawn settings for {row.Enemy} at level {row.FromLevel}.");
        if (Rows.GroupBy(row => (row.FromLevel, row.Enemy)).Any(group => group.Count() > 1))
            throw new InvalidOperationException("Duplicate enemy spawn rows for the same level.");
    }

    public static EnemySpawnRule Select(EnemyKind enemy, int playerLevel)
    {
        return Rows.Where(row => row.Enemy == enemy && row.FromLevel <= Math.Max(0, playerLevel))
            .OrderByDescending(row => row.FromLevel).FirstOrDefault()
            ?? new EnemySpawnRule(0, enemy, 0, 1f, 1f);
    }
}
