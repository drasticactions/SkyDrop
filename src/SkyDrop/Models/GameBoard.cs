namespace SkyDrop.Models;

/// <summary>
/// Represents the 10x20 game playfield.
/// </summary>
public class GameBoard
{
    private readonly TetrominoType?[,] _grid;

    public int Width => GameConstants.BoardWidth;
    public int Height => GameConstants.BoardHeight;

    public GameBoard()
    {
        _grid = new TetrominoType?[Width, Height];
    }

    /// <summary>
    /// Gets the block at the specified position, or null if empty.
    /// </summary>
    public TetrominoType? this[int x, int y]
    {
        get
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height)
                return null;
            return _grid[x, y];
        }
    }

    /// <summary>
    /// Checks if a position is within the board bounds.
    /// </summary>
    public bool IsInBounds(int x, int y)
    {
        return x >= 0 && x < Width && y >= 0 && y < Height;
    }

    /// <summary>
    /// Checks if a position is empty (no locked block).
    /// </summary>
    public bool IsEmpty(int x, int y)
    {
        if (x < 0 || x >= Width) return false;
        if (y < 0) return false;
        if (y >= Height) return false;
        return _grid[x, y] == null;
    }

    /// <summary>
    /// Checks if a tetromino can be placed at its current position.
    /// </summary>
    public bool CanPlace(Tetromino piece)
    {
        foreach (var pos in piece.GetBlockPositions())
        {
            if (pos.Y < 0) continue;

            if (pos.X < 0 || pos.X >= Width || pos.Y >= Height)
                return false;

            if (_grid[pos.X, pos.Y] != null)
                return false;
        }
        return true;
    }

    /// <summary>
    /// Checks if a piece can move by the specified offset.
    /// </summary>
    public bool CanMove(Tetromino piece, int dx, int dy)
    {
        var test = piece.Clone();
        test.Move(dx, dy);
        return CanPlace(test);
    }

    /// <summary>
    /// Checks if a piece can rotate clockwise.
    /// </summary>
    public bool CanRotate(Tetromino piece, bool clockwise = true)
    {
        var test = piece.Clone();
        if (clockwise)
            test.RotateClockwise();
        else
            test.RotateCounterClockwise();
        return CanPlace(test);
    }

    /// <summary>
    /// Locks a piece onto the board at its current position.
    /// </summary>
    public void LockPiece(Tetromino piece)
    {
        foreach (var pos in piece.GetBlockPositions())
        {
            if (pos.Y >= 0 && pos.Y < Height && pos.X >= 0 && pos.X < Width)
            {
                _grid[pos.X, pos.Y] = piece.Type;
            }
        }
    }

    /// <summary>
    /// Clears all complete lines and returns the number of lines cleared.
    /// </summary>
    public int ClearLines()
    {
        int linesCleared = 0;
        int writeRow = Height - 1;

        // Process from bottom to top
        for (int readRow = Height - 1; readRow >= 0; readRow--)
        {
            if (!IsLineFull(readRow))
            {
                // Copy this line down (if needed)
                if (writeRow != readRow)
                {
                    CopyRow(readRow, writeRow);
                }
                writeRow--;
            }
            else
            {
                linesCleared++;
            }
        }

        // Clear remaining rows at top
        for (int row = writeRow; row >= 0; row--)
        {
            ClearRow(row);
        }

        return linesCleared;
    }

    /// <summary>
    /// Checks if a line is completely filled.
    /// </summary>
    private bool IsLineFull(int y)
    {
        for (int x = 0; x < Width; x++)
        {
            if (_grid[x, y] == null)
                return false;
        }
        return true;
    }

    /// <summary>
    /// Copies one row to another.
    /// </summary>
    private void CopyRow(int from, int to)
    {
        for (int x = 0; x < Width; x++)
        {
            _grid[x, to] = _grid[x, from];
        }
    }

    /// <summary>
    /// Clears a row.
    /// </summary>
    private void ClearRow(int y)
    {
        for (int x = 0; x < Width; x++)
        {
            _grid[x, y] = null;
        }
    }

    /// <summary>
    /// Gets the Y position where a piece would land (for ghost piece).
    /// </summary>
    public int GetDropPosition(Tetromino piece)
    {
        var test = piece.Clone();
        while (CanMove(test, 0, 1))
        {
            test.Move(0, 1);
        }
        return test.Position.Y;
    }

    /// <summary>
    /// Checks if the game is over (piece would overlap on spawn).
    /// </summary>
    public bool IsGameOver(Tetromino newPiece)
    {
        return !CanPlace(newPiece);
    }

    /// <summary>
    /// Resets the board to empty state.
    /// </summary>
    public void Clear()
    {
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                _grid[x, y] = null;
            }
        }
    }

    /// <summary>
    /// Gets all filled cells for rendering.
    /// </summary>
    public IEnumerable<(Position Position, TetrominoType Type)> GetFilledCells()
    {
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                if (_grid[x, y].HasValue)
                {
                    yield return (new Position(x, y), _grid[x, y]!.Value);
                }
            }
        }
    }
}
