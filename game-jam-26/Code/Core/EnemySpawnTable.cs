using System;
using System.Linq;

public enum EnemyKind { Pike, Swordfish, Bubble, Pufferfish }

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
    private static readonly EnemySpawnRule[] Rows =
    {
        // Level  Enemy                 Max   Min sec Max sec  Batch min/max  First delay
        new(0,    EnemyKind.Pike,        10,   3f,     8f,      1, 1,          1f),
        new(1,    EnemyKind.Pike,         0,   3f,     8f),

        new(1,    EnemyKind.Swordfish,   12,   0.2f,   2f,      2, 5),
        new(2,    EnemyKind.Swordfish,   0,   0.2f,   2f),

        new(0,    EnemyKind.Bubble,       8,   0.5f,     5f),
        new(1,    EnemyKind.Bubble,       6,   0.5f,     4f),
        new(2,    EnemyKind.Bubble,      0,   0.3f,     3f),

        new(0,    EnemyKind.Pufferfish,   0,   6f,    10f,      1, 1,          5f),
        new(1,    EnemyKind.Pufferfish,   7,   2f,    4f,      1, 1,          5f),
        new(2,    EnemyKind.Pufferfish,   0,   2f,    5f),

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
            .OrderByDescending(row => row.FromLevel).First();
    }
}
