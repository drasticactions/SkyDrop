namespace SkyDrop.Models;

/// <summary>
/// Options for the Discover Feed Scroll game mode.
/// </summary>
public record DiscoverFeedScrollModeOptions(int StartLevel, string FeedUri, string FeedName) : IGameModeOptions
{
    /// <inheritdoc/>
    public GameMode Mode => GameMode.DiscoverFeedScroll;
}
