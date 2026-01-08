namespace SkyDrop.Models;

/// <summary>
/// Available game modes.
/// </summary>
public enum GameMode
{
    /// <summary>
    /// The block game we all know and love. No social media integration.
    /// </summary>
    Normal,

    /// <summary>
    /// Discover feed - posts revealed by clearing lines.
    /// </summary>
    DiscoverFeed,

    /// <summary>
    /// Discover feed - scroll by rotating.
    /// </summary>
    DiscoverFeedScroll,

    /// <summary>
    /// Author feed - posts revealed by clearing lines.
    /// </summary>
    AuthorFeed,

    /// <summary>
    /// Author feed - scroll by rotating.
    /// </summary>
    AuthorFeedScroll,

    /// <summary>
    /// Create post..
    /// </summary>
    CreatePost,

    /// <summary>
    /// Posts revealed by clearing lines.
    /// Requires login.
    /// </summary>
    Timeline,

    /// <summary>
    /// Scroll by rotating.
    /// Requires login.
    /// </summary>
    TimelineScroll,

    /// <summary>
    /// T9 text generator utility - convert between text and T9 sequences.
    /// </summary>
    T9Generator
}
