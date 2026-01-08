namespace SkyDrop.Models;

/// <summary>
/// The source type for feed-based game modes.
/// </summary>
public enum FeedSourceType
{
    /// <summary>
    /// Posts from a Bluesky feed generator (Discover feeds).
    /// </summary>
    DiscoverFeed,

    /// <summary>
    /// Posts from a specific Bluesky author.
    /// </summary>
    AuthorFeed,

    /// <summary>
    /// Posts from the authenticated user's home timeline.
    /// Requires login.
    /// </summary>
    Timeline
}
