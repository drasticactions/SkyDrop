namespace SkyDrop.Models;

/// <summary>
/// Options for Normal (Block) game mode.
/// </summary>
/// <param name="StartLevel">Starting level (0-19).</param>
public record NormalModeOptions(int StartLevel) : IGameModeOptions
{
    /// <inheritdoc/>
    public GameMode Mode => GameMode.Normal;
}
