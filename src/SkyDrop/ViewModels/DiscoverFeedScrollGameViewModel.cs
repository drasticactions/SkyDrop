using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using FishyFlip;
using FishyFlip.Lexicon.App.Bsky.Feed;
using SkyDrop.Collections;
using SkyDrop.Models;
using SkyDrop.Resources;

namespace SkyDrop.ViewModels;

/// <summary>
/// View model for the Discover Feed Scroll game mode.
/// </summary>
public partial class DiscoverFeedScrollGameViewModel : GameViewModelBase
{
    private readonly ATProtocol _protocol;
    private DiscoverFeedScrollModeOptions? _currentOptions;
    private FeedViewCollection? _feedCollection;

    /// <summary>
    /// Gets the collection of posts loaded from the feed.
    /// </summary>
    public ObservableCollection<FeedViewPost> FeedPosts { get; } = new();

    [ObservableProperty]
    private bool _isLoadingFeed;

    [ObservableProperty]
    private string? _feedError;

    [ObservableProperty]
    private double _scrollOffset;

    [ObservableProperty]
    private int _totalRotations;

    [ObservableProperty]
    private string _feedName = Strings.FeedNameScroll;

    public event Action<bool>? RequestScroll;

    public DiscoverFeedScrollGameViewModel(ATProtocol protocol)
    {
        _protocol = protocol;
        _engine.OnRotation += OnRotation;
    }

    /// <summary>
    /// Prepares the game and shows instructions.
    /// </summary>
    public async Task PrepareGameAsync(DiscoverFeedScrollModeOptions options)
    {
        _currentOptions = options;
        ShowInstructions = true;
        IsGameOver = false;
        IsPaused = false;
        ScrollOffset = 0;
        TotalRotations = 0;
        FeedPosts.Clear();
        FeedError = null;
        FeedName = options.FeedName;

        await LoadFeedAsync(options.FeedUri);
    }

    /// <summary>
    /// Dismisses instructions and starts the game.
    /// </summary>
    public override void DismissInstructionsAndStart()
    {
        ShowInstructions = false;

        if (_currentOptions == null)
            return;

        StartEngine(_currentOptions.StartLevel);
    }

    /// <summary>
    /// Starts a new game with the specified options (shows instructions first).
    /// </summary>
    public async Task StartGameAsync(DiscoverFeedScrollModeOptions options)
    {
        await PrepareGameAsync(options);
    }

    /// <summary>
    /// Loads posts from the specified feed URI.
    /// </summary>
    private async Task LoadFeedAsync(string feedUri)
    {
        try
        {
            IsLoadingFeed = true;
            FeedError = null;

            var uri = FishyFlip.Models.ATUri.Create(feedUri);
            if (uri == null)
            {
                FeedError = Strings.ErrorInvalidFeedUri;
                return;
            }

            _feedCollection = new FeedViewCollection(_protocol, uri);
            await _feedCollection.GetMoreItemsAsync(50);

            foreach (var post in _feedCollection)
            {
                FeedPosts.Add(post);
            }
        }
        catch (Exception ex)
        {
            FeedError = $"{Strings.ErrorFailedToLoadFeed}: {ex.Message}";
        }
        finally
        {
            IsLoadingFeed = false;
        }
    }

    /// <summary>
    /// Called when a rotation is performed.
    /// Scrolls the feed based on rotation direction.
    /// </summary>
    private void OnRotation(bool clockwise)
    {
        TotalRotations++;

        RequestScroll?.Invoke(clockwise);
    }

    /// <summary>
    /// Loads additional posts from the feed when approaching the bottom.
    /// </summary>
    private async Task TryLoadMorePostsAsync()
    {
        if (_feedCollection == null || !_feedCollection.HasMoreItems || IsLoadingFeed)
            return;

        try
        {
            IsLoadingFeed = true;
            var previousCount = FeedPosts.Count;
            await _feedCollection.GetMoreItemsAsync(25);

            var items = _feedCollection.ToList();
            for (int i = previousCount; i < items.Count; i++)
            {
                FeedPosts.Add(items[i]);
            }
        }
        catch (Exception ex)
        {
            FeedError = $"{Strings.ErrorFailedToLoadMorePosts}: {ex.Message}";
        }
        finally
        {
            IsLoadingFeed = false;
        }
    }

    /// <summary>
    /// Called by the view when scroll position changes to track when to load more.
    /// </summary>
    public void OnScrollPositionChanged(double offset, double viewportHeight, double extentHeight)
    {
        ScrollOffset = offset;

        if (extentHeight - offset - viewportHeight < viewportHeight * 2)
        {
            if (_feedCollection?.HasMoreItems == true && !IsLoadingFeed)
            {
                _ = TryLoadMorePostsAsync();
            }
        }
    }

    /// <summary>
    /// Restarts the game with the same options (skips instructions).
    /// </summary>
    public async Task RestartGameAsync()
    {
        if (_currentOptions == null)
            return;

        ShowInstructions = false;
        IsGameOver = false;
        IsPaused = false;
        ScrollOffset = 0;
        TotalRotations = 0;
        FeedPosts.Clear();
        FeedError = null;

        await LoadFeedAsync(_currentOptions.FeedUri);

        StartEngine(_currentOptions.StartLevel);
    }
}
