using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FishyFlip;
using FishyFlip.Lexicon.App.Bsky.Feed;
using SkyDrop.Collections;

namespace SkyDrop.ViewModels;

/// <summary>
/// ViewModel for selecting a feed from popular Bluesky feeds.
/// </summary>
public partial class FeedSelectorViewModel : ViewModelBase
{
    private readonly ATProtocol _protocol;

    public FeedSelectorViewModel(ATProtocol protocol)
    {
        _protocol = protocol;
        Generators = new PopularFeedGeneratorCollection(_protocol);
    }

    /// <summary>
    /// Gets the collection of feed generators.
    /// </summary>
    public PopularFeedGeneratorCollection Generators { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SelectedFeedDisplayName))]
    private GeneratorView? _selectedGenerator;

    /// <summary>
    /// Gets the display name of the selected feed, or a default message if none is selected.
    /// </summary>
    public string SelectedFeedDisplayName =>
        SelectedGenerator?.DisplayName ?? "No feed selected";

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _searchQuery = string.Empty;

    [ObservableProperty]
    private bool _isSearchBarHighlighted;

    [ObservableProperty]
    private bool _isLoadMoreHighlighted;

    [ObservableProperty]
    private int _highlightedIndex = -1;

    [ObservableProperty]
    private bool _hasMoreItems = true;

    /// <summary>
    /// Event raised when a feed is selected.
    /// </summary>
    public event Action<string>? FeedSelected;

    partial void OnSelectedGeneratorChanged(GeneratorView? value)
    {
        if (value?.Uri != null)
        {
            FeedSelected?.Invoke(value.Uri.ToString());
        }
    }

    [RelayCommand]
    private async Task LoadFeedsAsync()
    {
        if (IsLoading) return;

        try
        {
            IsLoading = true;
            Generators.Query = SearchQuery;
            await Generators.RefreshAsync(20).ConfigureAwait(false);
            HasMoreItems = !string.IsNullOrEmpty(Generators.Cursor);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task LoadMoreFeedsAsync()
    {
        if (IsLoading || !HasMoreItems) return;

        try
        {
            IsLoading = true;
            await Generators.GetMoreItemsAsync(20).ConfigureAwait(false);
            HasMoreItems = !string.IsNullOrEmpty(Generators.Cursor);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task SearchFeedsAsync()
    {
        if (IsLoading) return;

        try
        {
            IsLoading = true;
            Generators.Query = SearchQuery;
            await Generators.RefreshAsync(20).ConfigureAwait(false);
            HasMoreItems = !string.IsNullOrEmpty(Generators.Cursor);
            HighlightedIndex = Generators.Count > 0 ? 0 : -1;
            IsSearchBarHighlighted = false;
            IsLoadMoreHighlighted = false;
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void SelectFeed(GeneratorView? generator)
    {
        if (generator?.Uri != null)
        {
            SelectedGenerator = generator;
        }
    }

    [RelayCommand]
    private void SelectHighlighted()
    {
        if (HighlightedIndex >= 0 && HighlightedIndex < Generators.Count)
        {
            SelectFeed(Generators[HighlightedIndex]);
        }
    }

    [RelayCommand]
    private void MoveHighlightUp()
    {
        if (IsLoadMoreHighlighted)
        {
            IsLoadMoreHighlighted = false;
            HighlightedIndex = Generators.Count > 0 ? Generators.Count - 1 : -1;
            if (HighlightedIndex == -1)
            {
                IsSearchBarHighlighted = true;
            }
        }
        else if (HighlightedIndex > 0)
        {
            HighlightedIndex--;
        }
        else if (HighlightedIndex == 0)
        {
            HighlightedIndex = -1;
            IsSearchBarHighlighted = true;
        }
    }

    [RelayCommand]
    private void MoveHighlightDown()
    {
        if (IsSearchBarHighlighted)
        {
            IsSearchBarHighlighted = false;
            HighlightedIndex = Generators.Count > 0 ? 0 : -1;
            if (HighlightedIndex == -1 && HasMoreItems)
            {
                IsLoadMoreHighlighted = true;
            }
        }
        else if (HighlightedIndex >= 0 && HighlightedIndex < Generators.Count - 1)
        {
            HighlightedIndex++;
        }
        else if (HighlightedIndex == Generators.Count - 1)
        {
            if (HasMoreItems)
            {
                HighlightedIndex = -1;
                IsLoadMoreHighlighted = true;
            }
        }
    }

    /// <summary>
    /// Gets whether the highlight is at the bottom of the feed selector.
    /// </summary>
    public bool IsAtBottom =>
        IsLoadMoreHighlighted ||
        (!HasMoreItems && HighlightedIndex == Generators.Count - 1) ||
        (!HasMoreItems && Generators.Count == 0 && !IsSearchBarHighlighted);

    /// <summary>
    /// Resets the highlight state to the search bar.
    /// </summary>
    public void ResetHighlight()
    {
        IsSearchBarHighlighted = true;
        IsLoadMoreHighlighted = false;
        HighlightedIndex = -1;
    }

    /// <summary>
    /// Clears all highlight state.
    /// </summary>
    public void ClearHighlight()
    {
        IsSearchBarHighlighted = false;
        IsLoadMoreHighlighted = false;
        HighlightedIndex = -1;
    }
}
