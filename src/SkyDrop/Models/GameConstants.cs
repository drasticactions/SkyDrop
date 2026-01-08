using Avalonia.Media;

namespace SkyDrop.Models;

/// <summary>
/// Game constants
/// </summary>
public static class GameConstants
{
    public const int BoardWidth = 10;
    public const int BoardHeight = 20;

    public const double FrameRate = 60.0;
    public const double FrameTimeMs = 1000.0 / FrameRate;

    public const int DasInitialDelay = 16;
    public const int DasRepeatRate = 6;

    public const int SoftDropSpeed = 2;

    public const int LockDelayFrames = 30;

    public const int MaxLockDelayResets = 15;

    public const int SpawnX = 3;
    public const int SpawnY = 0;

    /// <summary>
    /// Frames per row drop.
    /// Index is the level (0-29+).
    /// </summary>
    public static readonly int[] GravityTable = new int[]
    {
        48,  // Level 0
        43,  // Level 1
        38,  // Level 2
        33,  // Level 3
        28,  // Level 4
        23,  // Level 5
        18,  // Level 6
        13,  // Level 7
        8,   // Level 8
        6,   // Level 9
        5,   // Level 10
        5,   // Level 11
        5,   // Level 12
        4,   // Level 13
        4,   // Level 14
        4,   // Level 15
        3,   // Level 16
        3,   // Level 17
        3,   // Level 18
        2,   // Level 19
        2,   // Level 20
        2,   // Level 21
        2,   // Level 22
        2,   // Level 23
        2,   // Level 24
        2,   // Level 25
        2,   // Level 26
        2,   // Level 27
        2,   // Level 28
        1    // Level 29+ (kill screen, if you get here while actually playing the game... good job)
    };

    /// <summary>
    /// Gets the gravity (frames per row) for a given level.
    /// </summary>
    public static int GetGravity(int level)
    {
        if (level < 0) return GravityTable[0];
        if (level >= GravityTable.Length) return GravityTable[^1];
        return GravityTable[level];
    }

    /// <summary>
    /// Base scoring values for line clears.
    /// Actual score = base * (level + 1)
    /// </summary>
    public static readonly int[] LineScoreBase = new int[]
    {
        0,     // 0 lines
        40,    // Single
        100,   // Double
        300,   // Triple
        1200   // The BIG one
    };

    /// <summary>
    /// Calculates the score for clearing lines at a given level.
    /// </summary>
    public static int CalculateScore(int linesCleared, int level)
    {
        if (linesCleared < 0 || linesCleared > 4) return 0;
        return LineScoreBase[linesCleared] * (level + 1);
    }

    /// <summary>
    /// Calculates lines needed to reach the next level.
    /// First level-up: (startLevel * 10) + 10
    /// Subsequent: every 10 lines
    /// </summary>
    public static int GetLinesForNextLevel(int startLevel, int currentLevel)
    {
        if (currentLevel == startLevel)
        {
            return (startLevel * 10) + 10;
        }
        return (currentLevel + 1) * 10;
    }

    /// <summary>
    /// Colors for each tetromino type.
    /// </summary>
    public static readonly IReadOnlyDictionary<TetrominoType, Color> PieceColors = new Dictionary<TetrominoType, Color>
    {
        { TetrominoType.I, Color.Parse("#00D4FF") },
        { TetrominoType.O, Color.Parse("#FFD700") },
        { TetrominoType.T, Color.Parse("#9B30FF") },
        { TetrominoType.S, Color.Parse("#32CD32") },
        { TetrominoType.Z, Color.Parse("#FF3030") },
        { TetrominoType.J, Color.Parse("#1E90FF") },
        { TetrominoType.L, Color.Parse("#FF8C00") }
    };

    /// <summary>
    /// Ghost piece opacity for landing preview.
    /// </summary>
    public const double GhostOpacity = 0.3;

    /// <summary>
    /// Grid line color.
    /// </summary>
    public static readonly Color GridColor = Color.Parse("#333333");

    /// <summary>
    /// Background color for the playfield.
    /// </summary>
    public static readonly Color BackgroundColor = Color.Parse("#1A1A1A");

    /// <summary>
    /// Border color for blocks.
    /// </summary>
    public static readonly Color BlockBorderColor = Color.Parse("#000000");
}
