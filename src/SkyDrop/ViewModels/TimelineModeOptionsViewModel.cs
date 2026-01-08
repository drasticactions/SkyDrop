using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SkyDrop.Models;

namespace SkyDrop.ViewModels;

/// <summary>
/// ViewModel for Timeline mode options.
/// </summary>
public partial class TimelineModeOptionsViewModel : GameModeOptionsViewModelBase
{
    [ObservableProperty]
    private int _selectedLevel;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Mode))]
    [NotifyPropertyChangedFor(nameof(GameTypeDescription))]
    [NotifyPropertyChangedFor(nameof(IsRevealMode))]
    [NotifyPropertyChangedFor(nameof(IsScrollMode))]
    private FeedGameType _selectedGameType = FeedGameType.Reveal;

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
    /// Available levels (0-19).
    /// </summary>
    public int[] AvailableLevels { get; } = Enumerable.Range(0, 20).ToArray();

    /// <inheritdoc/>
    public override GameMode Mode => SelectedGameType == FeedGameType.Scroll
        ? GameMode.TimelineScroll
        : GameMode.Timeline;

    /// <inheritdoc/>
    public override IGameModeOptions GetOptions()
    {
        return new FeedModeOptions(
            SelectedLevel,
            FeedSourceType.Timeline,
            SelectedGameType,
            FeedUri: null,
            AuthorIdentifier: null,
            AuthorFilter: null,
            FeedName: "YOUR TIMELINE"
        );
    }

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
}
