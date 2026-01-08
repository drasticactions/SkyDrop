namespace SkyDrop.Models;

/// <summary>
/// Represents a tetromino piece with its shape, position, and rotation state.
/// </summary>
public class Tetromino
{
    public TetrominoType Type { get; }
    public Position Position { get; private set; }
    public int RotationState { get; private set; }

    private static readonly Dictionary<TetrominoType, Position[][]> RotationStates = new()
    {
        {
            TetrominoType.I, new[]
            {
                new[] { new Position(0, 1), new Position(1, 1), new Position(2, 1), new Position(3, 1) },
                new[] { new Position(2, 0), new Position(2, 1), new Position(2, 2), new Position(2, 3) }
            }
        },
        {
            TetrominoType.O, new[]
            {
                new[] { new Position(1, 0), new Position(2, 0), new Position(1, 1), new Position(2, 1) }
            }
        },
        {
            TetrominoType.T, new[]
            {
                new[] { new Position(1, 0), new Position(0, 1), new Position(1, 1), new Position(2, 1) },
                new[] { new Position(1, 0), new Position(1, 1), new Position(2, 1), new Position(1, 2) },
                new[] { new Position(0, 1), new Position(1, 1), new Position(2, 1), new Position(1, 2) },
                new[] { new Position(1, 0), new Position(0, 1), new Position(1, 1), new Position(1, 2) }
            }
        },
        {
            TetrominoType.S, new[]
            {
                new[] { new Position(1, 0), new Position(2, 0), new Position(0, 1), new Position(1, 1) },
                new[] { new Position(1, 0), new Position(1, 1), new Position(2, 1), new Position(2, 2) }
            }
        },
        {
            TetrominoType.Z, new[]
            {
                new[] { new Position(0, 0), new Position(1, 0), new Position(1, 1), new Position(2, 1) },
                new[] { new Position(2, 0), new Position(1, 1), new Position(2, 1), new Position(1, 2) }
            }
        },
        {
            TetrominoType.J, new[]
            {
                new[] { new Position(0, 0), new Position(0, 1), new Position(1, 1), new Position(2, 1) },
                new[] { new Position(1, 0), new Position(2, 0), new Position(1, 1), new Position(1, 2) },
                new[] { new Position(0, 1), new Position(1, 1), new Position(2, 1), new Position(2, 2) },
                new[] { new Position(1, 0), new Position(1, 1), new Position(0, 2), new Position(1, 2) }
            }
        },
        {
            TetrominoType.L, new[]
            {
                new[] { new Position(2, 0), new Position(0, 1), new Position(1, 1), new Position(2, 1) },
                new[] { new Position(1, 0), new Position(1, 1), new Position(1, 2), new Position(2, 2) },
                new[] { new Position(0, 1), new Position(1, 1), new Position(2, 1), new Position(0, 2) },
                new[] { new Position(0, 0), new Position(1, 0), new Position(1, 1), new Position(1, 2) }
            }
        }
    };

    public Tetromino(TetrominoType type)
    {
        Type = type;
        Position = new Position(GameConstants.SpawnX, GameConstants.SpawnY);
        RotationState = 0;
    }

    /// <summary>
    /// Gets the block positions in absolute board coordinates.
    /// </summary>
    public IEnumerable<Position> GetBlockPositions()
    {
        var shape = GetCurrentShape();
        foreach (var block in shape)
        {
            yield return Position + block;
        }
    }

    /// <summary>
    /// Gets the block positions relative to the piece origin.
    /// </summary>
    public Position[] GetCurrentShape()
    {
        return RotationStates[Type][RotationState];
    }

    /// <summary>
    /// Gets the number of rotation states for this piece type.
    /// </summary>
    public int RotationCount => RotationStates[Type].Length;

    /// <summary>
    /// Moves the piece by the specified offset.
    /// </summary>
    public void Move(int dx, int dy)
    {
        Position = Position.Offset(dx, dy);
    }

    /// <summary>
    /// Rotates the piece clockwise.
    /// NRS has no wall kicks - rotation simply fails if blocked.
    /// </summary>
    public void RotateClockwise()
    {
        RotationState = (RotationState + 1) % RotationCount;
    }

    /// <summary>
    /// Rotates the piece counter-clockwise.
    /// </summary>
    public void RotateCounterClockwise()
    {
        RotationState = (RotationState - 1 + RotationCount) % RotationCount;
    }

    /// <summary>
    /// Creates a copy of this tetromino for testing moves.
    /// </summary>
    public Tetromino Clone()
    {
        var clone = new Tetromino(Type)
        {
            Position = Position,
            RotationState = RotationState
        };
        return clone;
    }

    /// <summary>
    /// Gets the blocks that would result from a clockwise rotation.
    /// </summary>
    public Position[] GetRotatedShape(bool clockwise = true)
    {
        int nextState = clockwise
            ? (RotationState + 1) % RotationCount
            : (RotationState - 1 + RotationCount) % RotationCount;
        return RotationStates[Type][nextState];
    }

    /// <summary>
    /// Gets the width of the current shape.
    /// </summary>
    public int Width
    {
        get
        {
            var shape = GetCurrentShape();
            int minX = shape.Min(p => p.X);
            int maxX = shape.Max(p => p.X);
            return maxX - minX + 1;
        }
    }

    /// <summary>
    /// Gets the height of the current shape.
    /// </summary>
    public int Height
    {
        get
        {
            var shape = GetCurrentShape();
            int minY = shape.Min(p => p.Y);
            int maxY = shape.Max(p => p.Y);
            return maxY - minY + 1;
        }
    }

    private Tetromino() { Type = TetrominoType.I; }
}
