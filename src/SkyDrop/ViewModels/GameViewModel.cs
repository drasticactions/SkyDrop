using SkyDrop.Models;

namespace SkyDrop.ViewModels;

/// <summary>
/// ViewModel for the normal game mode.
/// </summary>
public partial class GameViewModel : GameViewModelBase
{
    private IGameModeOptions? _currentOptions;

    /// <summary>
    /// Prepares a new game with the specified options and shows instructions.
    /// </summary>
    public void PrepareGame(IGameModeOptions options)
    {
        _currentOptions = options;
        ShowInstructions = true;
        IsGameOver = false;
        IsPaused = false;
    }

    /// <summary>
    /// Dismisses the instructions and starts the game.
    /// </summary>
    public override void DismissInstructionsAndStart()
    {
        ShowInstructions = false;

        if (_currentOptions == null)
            return;

        var startLevel = _currentOptions switch
        {
            NormalModeOptions normalOptions => normalOptions.StartLevel,
            _ => 0
        };

        StartEngine(startLevel);
    }

    /// <summary>
    /// Starts a new game with the specified options (shows instructions first).
    /// </summary>
    public void StartGame(IGameModeOptions options)
    {
        PrepareGame(options);
    }

    /// <summary>
    /// Restarts the game with the same options (skips instructions).
    /// </summary>
    public void RestartGame()
    {
        if (_currentOptions == null)
            return;

        ShowInstructions = false;
        IsGameOver = false;
        IsPaused = false;

        var startLevel = _currentOptions switch
        {
            NormalModeOptions normalOptions => normalOptions.StartLevel,
            _ => 0
        };

        StartEngine(startLevel);
    }
}
