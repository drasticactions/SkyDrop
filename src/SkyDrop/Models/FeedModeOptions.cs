namespace SkyDrop.Models;

/// <summary>
/// Unified options for all feed-based game modes.
/// </summary>
/// <param name="StartLevel">The starting level (0-19).</param>
/// <param name="SourceType">The source of the feed (DiscoverFeed or AuthorFeed).</param>
/// <param name="GameType">The gameplay type (Reveal or Scroll).</param>
/// <param name="FeedUri">The feed URI (for DiscoverFeed source type).</param>
/// <param name="AuthorIdentifier">The author DID or handle (for AuthorFeed source type).</param>
/// <param name="AuthorFilter">The author feed filter (for AuthorFeed source type).</param>
/// <param name="FeedName">Display name for the feed.</param>
public record FeedModeOptions(
    int StartLevel,
    FeedSourceType SourceType,
    FeedGameType GameType,
    string? FeedUri,
    string? AuthorIdentifier,
    string? AuthorFilter,
    string FeedName
) : IGameModeOptions
{
    /// <inheritdoc/>
    public GameMode Mode => (SourceType, GameType) switch
    {
        (FeedSourceType.DiscoverFeed, FeedGameType.Reveal) => GameMode.DiscoverFeed,
        (FeedSourceType.DiscoverFeed, FeedGameType.Scroll) => GameMode.DiscoverFeedScroll,
        (FeedSourceType.AuthorFeed, FeedGameType.Reveal) => GameMode.AuthorFeed,
        (FeedSourceType.AuthorFeed, FeedGameType.Scroll) => GameMode.AuthorFeedScroll,
        (FeedSourceType.Timeline, FeedGameType.Reveal) => GameMode.Timeline,
        (FeedSourceType.Timeline, FeedGameType.Scroll) => GameMode.TimelineScroll,
        _ => throw new ArgumentOutOfRangeException()
    };
}
