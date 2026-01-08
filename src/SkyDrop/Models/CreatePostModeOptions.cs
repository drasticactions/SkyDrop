using SkyDrop.ViewModels;

namespace SkyDrop.Models;

/// <summary>
/// Options for the Create Post game mode.
/// </summary>
/// <param name="StartLevel">The starting level (0-19).</param>
/// <param name="StartInputMode">The starting text input mode (T9 or ABC).</param>
public record CreatePostModeOptions(int StartLevel, TextInputMode StartInputMode = TextInputMode.T9) : IGameModeOptions
{
    /// <inheritdoc/>
    public GameMode Mode => GameMode.CreatePost;
}
