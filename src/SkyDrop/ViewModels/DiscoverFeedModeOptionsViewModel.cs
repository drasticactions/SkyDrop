using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SkyDrop.Models;

namespace SkyDrop.ViewModels;

public partial class DiscoverFeedModeOptionsViewModel : GameModeOptionsViewModelBase
{
    private const string DefaultFeedUri = "at://did:plc:z72i7hdynmk6r22z27h6tvur/app.bsky.feed.generator/whats-hot";

    public DiscoverFeedModeOptionsViewModel(FeedSelectorViewModel feedSelector)
    {
        FeedSelector = feedSelector;
        FeedSelector.FeedSelected += OnFeedSelected;
    }

    /// <summary>
    /// Gets the feed selector view model.
    /// </summary>
    public FeedSelectorViewModel FeedSelector { get; }

    [ObservableProperty]
    private int _selectedLevel;

    [ObservableProperty]
    private string _feedUri = DefaultFeedUri;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Mode))]
    [NotifyPropertyChangedFor(nameof(GameTypeDescription))]
    [NotifyPropertyChangedFor(nameof(IsRevealMode))]
    [NotifyPropertyChangedFor(nameof(IsScrollMode))]
    private DiscoverFeedGameType _selectedGameType = DiscoverFeedGameType.Reveal;

    /// <summary>
    /// Gets a description of the currently selected game type.
    /// </summary>
    public string GameTypeDescription => SelectedGameType switch
    {
        DiscoverFeedGameType.Reveal => "Posts are revealed as you clear lines",
        DiscoverFeedGameType.Scroll => "Rotate pieces to scroll through the feed",
        _ => ""
    };

    /// <summary>
    /// Gets whether Reveal mode is selected.
    /// </summary>
    public bool IsRevealMode => SelectedGameType == DiscoverFeedGameType.Reveal;

    /// <summary>
    /// Gets whether Scroll mode is selected.
    /// </summary>
    public bool IsScrollMode => SelectedGameType == DiscoverFeedGameType.Scroll;

    private void OnFeedSelected(string feedUri)
    {
        FeedUri = feedUri;
    }

    /// <summary>
    /// Available levels (0-19).
    /// </summary>
    public int[] AvailableLevels { get; } = Enumerable.Range(0, 20).ToArray();

    /// <inheritdoc/>
    public override GameMode Mode => SelectedGameType == DiscoverFeedGameType.Scroll
        ? GameMode.DiscoverFeedScroll
        : GameMode.DiscoverFeed;

    /// <inheritdoc/>
    public override IGameModeOptions GetOptions() => SelectedGameType switch
    {
        DiscoverFeedGameType.Scroll => new DiscoverFeedScrollModeOptions(SelectedLevel, FeedUri, FeedSelector.SelectedFeedDisplayName),
        _ => new DiscoverFeedModeOptions(SelectedLevel, FeedUri, FeedSelector.SelectedFeedDisplayName)
    };

    partial void OnSelectedLevelChanging(int value)
    {
        if (value < 0) _selectedLevel = 0;
        else if (value > 19) _selectedLevel = 19;
    }

    [RelayCommand]
    private void Play()
    {
        RequestStartGame();
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
    private void ResetFeedUri()
    {
        FeedUri = DefaultFeedUri;
    }

    [RelayCommand]
    private void SelectRevealMode()
    {
        SelectedGameType = DiscoverFeedGameType.Reveal;
    }

    [RelayCommand]
    private void SelectScrollMode()
    {
        SelectedGameType = DiscoverFeedGameType.Scroll;
    }

    [RelayCommand]
    private void ToggleGameType()
    {
        SelectedGameType = SelectedGameType == DiscoverFeedGameType.Reveal
            ? DiscoverFeedGameType.Scroll
            : DiscoverFeedGameType.Reveal;
    }
}
