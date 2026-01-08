using System.ComponentModel;
using SkyDrop.Resources;
using SkyDrop.Services;

namespace SkyDrop.Models;

/// <summary>
/// Displayable information about a game mode.
/// </summary>
public class GameModeInfo : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    public GameModeInfo(GameMode mode, bool isAvailable, bool requiresLogin = false)
    {
        Mode = mode;
        IsAvailable = isAvailable;
        RequiresLogin = requiresLogin;

        LocalizationService.Instance.LanguageChanged += OnLanguageChanged;
    }

    private void OnLanguageChanged()
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Description)));
    }

    /// <summary>The game mode type.</summary>
    public GameMode Mode { get; }

    /// <summary>Whether this mode is implemented and playable.</summary>
    public bool IsAvailable { get; }

    /// <summary>Whether this mode requires the user to be logged in.</summary>
    public bool RequiresLogin { get; }

    /// <summary>Display name for the mode.</summary>
    public string Name => Mode switch
    {
        GameMode.Normal => Strings.ModeNormal,
        GameMode.DiscoverFeed => Strings.ModeDiscoverFeed,
        GameMode.AuthorFeed => Strings.ModeAuthorFeed,
        GameMode.Timeline => Strings.ModeTimeline,
        GameMode.CreatePost => Strings.ModeCreatePost,
        GameMode.T9Generator => Strings.ModeT9Generator,
        _ => Mode.ToString()
    };

    /// <summary>Brief description of the mode.</summary>
    public string Description => Mode switch
    {
        GameMode.Normal => Strings.ModeNormalDesc,
        GameMode.DiscoverFeed => Strings.ModeDiscoverFeedDesc,
        GameMode.AuthorFeed => Strings.ModeAuthorFeedDesc,
        GameMode.Timeline => Strings.ModeTimelineDesc,
        GameMode.CreatePost => Strings.ModeCreatePostDesc,
        GameMode.T9Generator => Strings.ModeT9GeneratorDesc,
        _ => string.Empty
    };
}
