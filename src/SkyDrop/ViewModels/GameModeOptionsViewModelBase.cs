using SkyDrop.Models;

namespace SkyDrop.ViewModels;

/// <summary>
/// Base class for game mode options view models.
/// </summary>
public abstract class GameModeOptionsViewModelBase : ViewModelBase
{
    /// <summary>
    /// Gets the game mode this options screen is for.
    /// </summary>
    public abstract GameMode Mode { get; }

    /// <summary>
    /// Gets the options configured by this view model.
    /// </summary>
    public abstract IGameModeOptions GetOptions();

    /// <summary>
    /// Fired when the user wants to start the game with the configured options.
    /// </summary>
    public event Action<IGameModeOptions>? StartGameRequested;

    /// <summary>
    /// Fired when the user wants to go back to mode selection.
    /// </summary>
    public event Action? BackRequested;

    /// <summary>
    /// Requests to start the game with current options.
    /// </summary>
    protected void RequestStartGame() => StartGameRequested?.Invoke(GetOptions());

    /// <summary>
    /// Requests to go back to mode selection.
    /// </summary>
    protected void RequestBack() => BackRequested?.Invoke();
}
