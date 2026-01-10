using SkyDrop.ViewModels;

namespace SkyDrop.Models;

/// <summary>
/// Options for the Create Post game mode.
/// </summary>
/// <param name="StartLevel">The starting level (0-19).</param>
/// <param name="StartInputMode">The starting text input mode (T9 or ABC). Used in Standard variant.</param>
/// <param name="Variant">The CreatePost variant (Standard or Queued).</param>
/// <param name="QueuedPosts">Pre-written posts for Queued variant. Each string is a separate post in the thread.</param>
public record CreatePostModeOptions(
    int StartLevel,
    TextInputMode StartInputMode = TextInputMode.T9,
    CreatePostVariant Variant = CreatePostVariant.Standard,
    IReadOnlyList<string>? QueuedPosts = null) : IGameModeOptions
{
    /// <inheritdoc/>
    public GameMode Mode => GameMode.CreatePost;
}
