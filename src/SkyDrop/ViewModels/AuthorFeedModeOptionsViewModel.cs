using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FishyFlip;
using FishyFlip.Lexicon.App.Bsky.Actor;
using FishyFlip.Models;
using SkyDrop.Models;

namespace SkyDrop.ViewModels;

/// <summary>
/// Filter option for author feeds.
/// </summary>
public record AuthorFilterOption(string Value, string DisplayName, string Description);

public partial class AuthorFeedModeOptionsViewModel : GameModeOptionsViewModelBase
{
    private readonly ATProtocol _protocol;
    private CancellationTokenSource? _lookupCts;

    public AuthorFeedModeOptionsViewModel(ATProtocol protocol)
    {
        _protocol = protocol;
    }

    [ObservableProperty]
    private int _selectedLevel;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanPlay))]
    private string _authorHandle = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanPlay))]
    [NotifyPropertyChangedFor(nameof(HasResolvedProfile))]
    private ProfileViewDetailed? _resolvedProfile;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanPlay))]
    private bool _isResolvingProfile;

    [ObservableProperty]
    private string? _profileError;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Mode))]
    [NotifyPropertyChangedFor(nameof(GameTypeDescription))]
    [NotifyPropertyChangedFor(nameof(IsRevealMode))]
    [NotifyPropertyChangedFor(nameof(IsScrollMode))]
    private FeedGameType _selectedGameType = FeedGameType.Reveal;

    [ObservableProperty]
    private int _selectedFilterIndex;

    /// <summary>
    /// Available filter options for author feeds.
    /// </summary>
    public AuthorFilterOption[] AvailableFilters { get; } = new[]
    {
        new AuthorFilterOption(AuthorFilterConstants.PostsAndAuthorThreads, "All Posts", "Posts and threads by the author"),
        new AuthorFilterOption(AuthorFilterConstants.PostsWithReplies, "With Replies", "Posts including replies"),
        new AuthorFilterOption(AuthorFilterConstants.PostsWithMedia, "Media Only", "Posts with images or media"),
        new AuthorFilterOption(AuthorFilterConstants.PostsWithVideo, "Video Only", "Posts with videos"),
    };

    /// <summary>
    /// Gets the currently selected filter option.
    /// </summary>
    public AuthorFilterOption SelectedFilter => AvailableFilters[SelectedFilterIndex];

    /// <summary>
    /// Gets a description of the currently selected game type.
    /// </summary>
    public string GameTypeDescription => SelectedGameType switch
    {
        FeedGameType.Reveal => "Posts are revealed as you clear lines",
        FeedGameType.Scroll => "Rotate pieces to scroll through the feed",
        _ => ""
    };

    /// <summary>
    /// Gets whether Reveal mode is selected.
    /// </summary>
    public bool IsRevealMode => SelectedGameType == FeedGameType.Reveal;

    /// <summary>
    /// Gets whether Scroll mode is selected.
    /// </summary>
    public bool IsScrollMode => SelectedGameType == FeedGameType.Scroll;

    /// <summary>
    /// Gets whether a profile has been successfully resolved.
    /// </summary>
    public bool HasResolvedProfile => ResolvedProfile != null;

    /// <summary>
    /// Gets whether the Play button should be enabled.
    /// </summary>
    public bool CanPlay => ResolvedProfile != null && !IsResolvingProfile;

    /// <summary>
    /// Gets the display name to show for the resolved profile.
    /// </summary>
    public string ResolvedDisplayName => ResolvedProfile?.DisplayName ?? ResolvedProfile?.Handle?.Handle ?? "Unknown";

    /// <summary>
    /// Available levels (0-19).
    /// </summary>
    public int[] AvailableLevels { get; } = Enumerable.Range(0, 20).ToArray();

    /// <inheritdoc/>
    public override GameMode Mode => SelectedGameType == FeedGameType.Scroll
        ? GameMode.AuthorFeedScroll
        : GameMode.AuthorFeed;

    /// <inheritdoc/>
    public override IGameModeOptions GetOptions()
    {
        var authorId = ResolvedProfile?.Did?.ToString() ?? AuthorHandle;
        var feedName = ResolvedProfile?.DisplayName ?? ResolvedProfile?.Handle?.Handle ?? AuthorHandle;

        return new FeedModeOptions(
            SelectedLevel,
            FeedSourceType.AuthorFeed,
            SelectedGameType,
            FeedUri: null,
            AuthorIdentifier: authorId,
            AuthorFilter: SelectedFilter.Value,
            FeedName: $"@{feedName}"
        );
    }

    partial void OnSelectedLevelChanging(int value)
    {
        if (value < 0) _selectedLevel = 0;
        else if (value > 19) _selectedLevel = 19;
    }

    partial void OnAuthorHandleChanged(string value)
    {
        // Clear previous profile when handle changes
        ResolvedProfile = null;
        ProfileError = null;
    }

    partial void OnResolvedProfileChanged(ProfileViewDetailed? value)
    {
        OnPropertyChanged(nameof(ResolvedDisplayName));
    }

    [RelayCommand]
    private async Task LookupProfileAsync()
    {
        if (string.IsNullOrWhiteSpace(AuthorHandle))
        {
            ProfileError = "Please enter a handle";
            return;
        }

        // Cancel any previous lookup
        _lookupCts?.Cancel();
        _lookupCts = new CancellationTokenSource();

        try
        {
            IsResolvingProfile = true;
            ProfileError = null;
            ResolvedProfile = null;

            // Clean up the handle (remove @ if present)
            var cleanHandle = AuthorHandle.TrimStart('@').Trim();

            // Try to create an ATIdentifier from the handle
            var identifier = ATIdentifier.Create(cleanHandle);
            if (identifier == null)
            {
                ProfileError = "Invalid handle format";
                return;
            }

            var (profile, error) = await _protocol.Actor.GetProfileAsync(identifier, _lookupCts.Token);

            if (error != null)
            {
                ProfileError = error.Detail?.Message ?? "Profile not found";
                return;
            }

            if (profile == null)
            {
                ProfileError = "Profile not found";
                return;
            }

            ResolvedProfile = profile;
        }
        catch (OperationCanceledException)
        {
            // Lookup was cancelled, ignore
        }
        catch (Exception ex)
        {
            ProfileError = $"Error: {ex.Message}";
        }
        finally
        {
            IsResolvingProfile = false;
        }
    }

    [RelayCommand]
    private void Play()
    {
        if (CanPlay)
        {
            RequestStartGame();
        }
    }

    [RelayCommand]
    private void Back()
    {
        RequestBack();
    }

    [RelayCommand]
    private void IncreaseLevel()
    {
        if (SelectedLevel < 19)
            SelectedLevel++;
    }

    [RelayCommand]
    private void DecreaseLevel()
    {
        if (SelectedLevel > 0)
            SelectedLevel--;
    }

    [RelayCommand]
    private void SelectRevealMode()
    {
        SelectedGameType = FeedGameType.Reveal;
    }

    [RelayCommand]
    private void SelectScrollMode()
    {
        SelectedGameType = FeedGameType.Scroll;
    }

    [RelayCommand]
    private void ToggleGameType()
    {
        SelectedGameType = SelectedGameType == FeedGameType.Reveal
            ? FeedGameType.Scroll
            : FeedGameType.Reveal;
    }

    [RelayCommand]
    private void NextFilter()
    {
        SelectedFilterIndex = (SelectedFilterIndex + 1) % AvailableFilters.Length;
        OnPropertyChanged(nameof(SelectedFilter));
    }

    [RelayCommand]
    private void PreviousFilter()
    {
        SelectedFilterIndex = (SelectedFilterIndex - 1 + AvailableFilters.Length) % AvailableFilters.Length;
        OnPropertyChanged(nameof(SelectedFilter));
    }

    [RelayCommand]
    private void ClearProfile()
    {
        AuthorHandle = string.Empty;
        ResolvedProfile = null;
        ProfileError = null;
    }
}
