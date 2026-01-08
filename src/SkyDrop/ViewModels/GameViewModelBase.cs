using CommunityToolkit.Mvvm.ComponentModel;
using SkyDrop.Models;
using SkyDrop.Services;

namespace SkyDrop.ViewModels;

/// <summary>
/// Base class for all game mode view models.
/// </summary>
public abstract partial class GameViewModelBase : ViewModelBase, IGameInputHandler
{
    protected readonly GameEngine _engine;

    public GameEngine Engine => _engine;

    [ObservableProperty]
    private int _score;

    [ObservableProperty]
    private int _level;

    [ObservableProperty]
    private int _lines;

    [ObservableProperty]
    private Tetromino? _nextPiece;

    [ObservableProperty]
    private bool _isGameOver;

    [ObservableProperty]
    private bool _isPaused;

    [ObservableProperty]
    private bool _showInstructions = true;

    /// <summary>
    /// Event raised when the user requests to return to the title screen.
    /// </summary>
    public event Action? RequestReturnToTitle;

    protected GameViewModelBase()
    {
        _engine = new GameEngine();
        _engine.OnGameStateChanged += UpdateStats;
        _engine.OnGameOver += OnGameOver;
    }

    /// <summary>
    /// Handles key down events.
    /// </summary>
    public void HandleKeyDown(GameInput input)
    {
        _engine.OnKeyDown(input);
    }

    /// <summary>
    /// Handles key up events.
    /// </summary>
    public void HandleKeyUp(GameInput input)
    {
        _engine.OnKeyUp(input);
    }

    /// <summary>
    /// Handles game input from the input service.
    /// </summary>
    /// <param name="args">The game input event args.</param>
    public void HandleGameInput(GameInputEventArgs args)
    {
        if (args.IsPressed)
        {
            _engine.OnKeyDown(args.Input);
        }
        else
        {
            _engine.OnKeyUp(args.Input);
        }
    }

    /// <summary>
    /// Updates displayed stats from the engine.
    /// </summary>
    protected virtual void UpdateStats()
    {
        Score = _engine.Score;
        Level = _engine.Level;
        Lines = _engine.Lines;
        NextPiece = _engine.NextPiece;
        IsPaused = _engine.IsPaused;
        IsGameOver = _engine.IsGameOver;
    }

    /// <summary>
    /// Called when the game ends.
    /// </summary>
    protected virtual void OnGameOver()
    {
        IsGameOver = true;
    }

    /// <summary>
    /// Returns to the title screen.
    /// </summary>
    public virtual void ReturnToTitle()
    {
        _engine.StopGame();
        IsPaused = false;
        IsGameOver = false;
        ShowInstructions = true;
        RequestReturnToTitle?.Invoke();
    }

    /// <summary>
    /// Dismisses the instructions overlay and starts the game.
    /// Subclasses should override to implement mode-specific start logic.
    /// </summary>
    public abstract void DismissInstructionsAndStart();

    /// <summary>
    /// Starts the game engine with the specified start level.
    /// </summary>
    protected void StartEngine(int startLevel)
    {
        _engine.StartGame(startLevel);
        UpdateStats();
    }
}
