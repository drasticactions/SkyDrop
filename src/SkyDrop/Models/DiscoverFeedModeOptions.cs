namespace SkyDrop.Models;

/// <summary>
/// Options for the Discover Feed game mode.
/// </summary>
public record DiscoverFeedModeOptions(int StartLevel, string FeedUri, string FeedName) : IGameModeOptions
{
    /// <inheritdoc/>
    public GameMode Mode => GameMode.DiscoverFeed;
}
