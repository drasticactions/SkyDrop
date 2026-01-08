using System;
using Avalonia.Threading;
using SkyDrop.Models;

namespace SkyDrop.Services;

/// <summary>
/// Core game engine handling the game loop, gravity, input with DAS, and game state.
/// </summary>
public class GameEngine
{
    private readonly DispatcherTimer _gameTimer;
    private readonly Random _random = new();

    // Game state
    private readonly GameBoard _board = new();
    private Tetromino? _currentPiece;
    private Tetromino? _nextPiece;
    private int _score;
    private int _level;
    private int _startLevel;
    private int _totalLines;
    private bool _isGameOver;
    private bool _isPaused;

    // Timing
    private int _gravityCounter;
    private int _softDropCounter;
    private bool _isSoftDropping;

    // DAS (Delayed Auto Shift)
    private int _dasCounter;
    private int _dasDirection; // -1 = left, 0 = none, 1 = right
    private bool _dasCharged;

    // Lock delay (Infinity/Extended Placement Lock Down)
    private int _lockDelayCounter;
    private int _lockDelayResets;
    private bool _isOnGround;

    // Events for UI updates
    public event Action? OnGameStateChanged;
    public event Action? OnGameOver;
    public event Action? OnBoardChanged;
    public event Action<int>? OnLinesCleared;
    public event Action<bool>? OnRotation; // true = clockwise, false = counter-clockwise
    public event Action? OnPieceLocked;

    // Properties for binding
    public GameBoard Board => _board;
    public Tetromino? CurrentPiece => _currentPiece;
    public Tetromino? NextPiece => _nextPiece;
    public int Score => _score;
    public int Level => _level;
    public int Lines => _totalLines;
    public bool IsGameOver => _isGameOver;
    public bool IsPaused => _isPaused;
    public bool IsRunning => _gameTimer.IsEnabled;

    public GameEngine()
    {
        _gameTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(GameConstants.FrameTimeMs)
        };
        _gameTimer.Tick += GameLoop;
    }

    /// <summary>
    /// Starts a new game at the specified level.
    /// </summary>
    public void StartGame(int startLevel = 0)
    {
        _board.Clear();
        _score = 0;
        _level = startLevel;
        _startLevel = startLevel;
        _totalLines = 0;
        _isGameOver = false;
        _isPaused = false;

        _gravityCounter = 0;
        _softDropCounter = 0;
        _isSoftDropping = false;
        _dasCounter = 0;
        _dasDirection = 0;
        _dasCharged = false;

        _nextPiece = CreateRandomPiece();
        SpawnNextPiece();

        _gameTimer.Start();
        OnGameStateChanged?.Invoke();
        OnBoardChanged?.Invoke();
    }

    /// <summary>
    /// Stops the current game.
    /// </summary>
    public void StopGame()
    {
        _gameTimer.Stop();
    }

    /// <summary>
    /// Toggles pause state.
    /// </summary>
    public void TogglePause()
    {
        if (_isGameOver) return;

        _isPaused = !_isPaused;
        if (_isPaused)
        {
            _gameTimer.Stop();
        }
        else
        {
            _gameTimer.Start();
        }
        OnGameStateChanged?.Invoke();
    }

    /// <summary>
    /// Main game loop - called every frame (~60 FPS).
    /// </summary>
    private void GameLoop(object? sender, EventArgs e)
    {
        if (_isGameOver || _isPaused || _currentPiece == null)
            return;

        // Process DAS
        ProcessDas();

        // Process gravity
        ProcessGravity();
    }

    /// <summary>
    /// Processes DAS (Delayed Auto Shift) for horizontal movement.
    /// </summary>
    private void ProcessDas()
    {
        if (_dasDirection == 0)
        {
            _dasCounter = 0;
            _dasCharged = false;
            return;
        }

        _dasCounter++;

        if (!_dasCharged && _dasCounter >= GameConstants.DasInitialDelay)
        {
            // Initial DAS charge complete
            _dasCharged = true;
            TryMove(_dasDirection, 0);
            _dasCounter = 0;
        }
        else if (_dasCharged && _dasCounter >= GameConstants.DasRepeatRate)
        {
            // Auto-repeat
            TryMove(_dasDirection, 0);
            _dasCounter = 0;
        }
    }

    /// <summary>
    /// Processes gravity to make pieces fall.
    /// </summary>
    private void ProcessGravity()
    {
        if (_currentPiece == null) return;

        int gravity = GameConstants.GetGravity(_level);

        // Check if piece is on the ground (can't move down)
        bool canMoveDown = _board.CanMove(_currentPiece, 0, 1);

        if (!canMoveDown)
        {
            // Piece is on the ground - process lock delay
            _isOnGround = true;
            _lockDelayCounter++;

            // Lock if delay expired OR max resets reached
            if (_lockDelayCounter >= GameConstants.LockDelayFrames ||
                _lockDelayResets >= GameConstants.MaxLockDelayResets)
            {
                LockPiece();
            }
        }
        else
        {
            // Piece is in the air - reset lock delay counter (but not resets)
            _isOnGround = false;
            _lockDelayCounter = 0;

            // Apply gravity
            if (_isSoftDropping)
            {
                _softDropCounter++;
                if (_softDropCounter >= GameConstants.SoftDropSpeed)
                {
                    TryMove(0, 1);
                    _score++; // 1 point per soft drop cell
                    OnGameStateChanged?.Invoke();
                    _softDropCounter = 0;
                }
            }
            else
            {
                _gravityCounter++;
                if (_gravityCounter >= gravity)
                {
                    TryMove(0, 1);
                    _gravityCounter = 0;
                }
            }
        }
    }

    /// <summary>
    /// Handles key press events.
    /// </summary>
    public void OnKeyDown(GameInput input)
    {
        if (_isGameOver || _isPaused || _currentPiece == null)
        {
            if (input == GameInput.Pause && !_isGameOver)
            {
                TogglePause();
            }
            return;
        }

        switch (input)
        {
            case GameInput.Left:
                if (TryMove(-1, 0))
                {
                    _dasDirection = -1;
                    _dasCounter = 0;
                    _dasCharged = false;
                }
                else
                {
                    _dasDirection = -1;
                    _dasCounter = GameConstants.DasInitialDelay;
                    _dasCharged = true;
                }
                break;

            case GameInput.Right:
                if (TryMove(1, 0))
                {
                    _dasDirection = 1;
                    _dasCounter = 0;
                    _dasCharged = false;
                }
                else
                {
                    _dasDirection = 1;
                    _dasCounter = GameConstants.DasInitialDelay;
                    _dasCharged = true;
                }
                break;

            case GameInput.SoftDrop:
                _isSoftDropping = true;
                _softDropCounter = 0;
                break;

            case GameInput.HardDrop:
                int ghostY = _board.GetDropPosition(_currentPiece);
                _currentPiece.Move(0, ghostY - _currentPiece.Position.Y);
                LockPiece();
                break;

            case GameInput.RotateCW:
                TryRotate(clockwise: true);
                break;

            case GameInput.RotateCCW:
                TryRotate(clockwise: false);
                break;

            case GameInput.Pause:
                TogglePause();
                break;
        }
    }

    /// <summary>
    /// Handles key release events.
    /// </summary>
    public void OnKeyUp(GameInput input)
    {
        switch (input)
        {
            case GameInput.Left:
                if (_dasDirection == -1)
                {
                    _dasDirection = 0;
                }
                break;

            case GameInput.Right:
                if (_dasDirection == 1)
                {
                    _dasDirection = 0;
                }
                break;

            case GameInput.SoftDrop:
                _isSoftDropping = false;
                _softDropCounter = 0;
                break;
        }
    }

    /// <summary>
    /// Attempts to move the current piece.
    /// </summary>
    private bool TryMove(int dx, int dy)
    {
        if (_currentPiece == null) return false;

        if (_board.CanMove(_currentPiece, dx, dy))
        {
            _currentPiece.Move(dx, dy);

            if (_isOnGround && dx != 0)
            {
                _lockDelayCounter = 0;
                _lockDelayResets++;
            }

            OnBoardChanged?.Invoke();
            return true;
        }
        return false;
    }

    /// <summary>
    /// Attempts to rotate the current piece.
    /// </summary>
    private bool TryRotate(bool clockwise)
    {
        if (_currentPiece == null) return false;

        if (_board.CanRotate(_currentPiece, clockwise))
        {
            if (clockwise)
                _currentPiece.RotateClockwise();
            else
                _currentPiece.RotateCounterClockwise();

            if (_isOnGround)
            {
                _lockDelayCounter = 0;
                _lockDelayResets++;
            }

            OnBoardChanged?.Invoke();
            OnRotation?.Invoke(clockwise);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Locks the current piece and processes line clears.
    /// </summary>
    private void LockPiece()
    {
        if (_currentPiece == null) return;

        _board.LockPiece(_currentPiece);
        
        OnPieceLocked?.Invoke();

        int lines = _board.ClearLines();
        if (lines > 0)
        {
            _totalLines += lines;
            _score += GameConstants.CalculateScore(lines, _level);

            int linesForNextLevel = GameConstants.GetLinesForNextLevel(_startLevel, _level);
            while (_totalLines >= linesForNextLevel)
            {
                _level++;
                linesForNextLevel = GameConstants.GetLinesForNextLevel(_startLevel, _level);
            }

            OnLinesCleared?.Invoke(lines);
        }

        SpawnNextPiece();

        OnGameStateChanged?.Invoke();
        OnBoardChanged?.Invoke();
    }

    /// <summary>
    /// Spawns the next piece.
    /// </summary>
    private void SpawnNextPiece()
    {
        _currentPiece = _nextPiece;
        _nextPiece = CreateRandomPiece();

        _gravityCounter = 0;
        _softDropCounter = 0;
        _lockDelayCounter = 0;
        _lockDelayResets = 0;
        _isOnGround = false;

        if (_currentPiece != null && _board.IsGameOver(_currentPiece))
        {
            _isGameOver = true;
            _gameTimer.Stop();
            OnGameOver?.Invoke();
        }
    }

    /// <summary>
    /// Creates a random tetromino.
    /// </summary>
    private Tetromino CreateRandomPiece()
    {
        var types = Enum.GetValues<TetrominoType>();
        var type = types[_random.Next(types.Length)];
        return new Tetromino(type);
    }

    /// <summary>
    /// Gets the ghost piece position (where the piece would land).
    /// </summary>
    public int GetGhostY()
    {
        if (_currentPiece == null) return 0;
        return _board.GetDropPosition(_currentPiece);
    }
}
