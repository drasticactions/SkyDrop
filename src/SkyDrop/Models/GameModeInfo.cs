namespace SkyDrop.Models;

/// <summary>
/// Displayable information about a game mode.
/// </summary>
/// <param name="Mode">The game mode type.</param>
/// <param name="Name">Display name for the mode.</param>
/// <param name="Description">Brief description of the mode.</param>
/// <param name="IsAvailable">Whether this mode is implemented and playable.</param>
/// <param name="RequiresLogin">Whether this mode requires the user to be logged in.</param>
public record GameModeInfo(
    GameMode Mode,
    string Name,
    string Description,
    bool IsAvailable,
    bool RequiresLogin = false);
