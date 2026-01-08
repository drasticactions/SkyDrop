namespace SkyDrop.Models;

/// <summary>
/// Base interface for game mode options.
/// </summary>
public interface IGameModeOptions
{
    /// <summary>
    /// Gets the game mode these options are for.
    /// </summary>
    GameMode Mode { get; }
}
