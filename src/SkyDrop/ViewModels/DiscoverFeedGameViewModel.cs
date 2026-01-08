using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using FishyFlip;
using FishyFlip.Lexicon.App.Bsky.Feed;
using SkyDrop.Collections;
using SkyDrop.Models;

namespace SkyDrop.ViewModels;

/// <summary>
/// View model for the Discover Feed game mode.
/// </summary>
public partial class DiscoverFeedGameViewModel : GameViewModelBase
{
    private readonly ATProtocol _protocol;
    private DiscoverFeedModeOptions? _currentOptions;
    private FeedViewCollection? _feedCollection;
    private int _feedIndex;

    /// <summary>
    /// Gets the collection of posts revealed during gameplay.
    /// Posts are added as the player clears lines.
    /// </summary>
    public ObservableCollection<FeedViewPost> RevealedPosts { get; } = new();

    /// <summary>
    /// Gets the collection of posts loaded from the feed but not yet revealed.
    /// </summary>
    private List<FeedViewPost> _loadedPosts = new();

    [ObservableProperty]
    private bool _isLoadingFeed;

    [ObservableProperty]
    private string? _feedError;

    [ObservableProperty]
    private int _totalPostsRevealed;

    [ObservableProperty]
    private string _feedName = "DISCOVER FEED";

    public DiscoverFeedGameViewModel(ATProtocol protocol)
    {
        _protocol = protocol;
        _engine.OnLinesCleared += OnLinesCleared;
    }

    /// <summary>
    /// Prepares the game and shows instructions.
    /// </summary>
    public async Task PrepareGameAsync(DiscoverFeedModeOptions options)
    {
        _currentOptions = options;
        ShowInstructions = true;
        IsGameOver = false;
        IsPaused = false;
        _feedIndex = 0;
        TotalPostsRevealed = 0;
        RevealedPosts.Clear();
        _loadedPosts.Clear();
        FeedError = null;
        FeedName = options.FeedName;

        // Pre-load the feed while showing instructions
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
    public async Task StartGameAsync(DiscoverFeedModeOptions options)
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
                FeedError = "Invalid feed URI";
                return;
            }

            _feedCollection = new FeedViewCollection(_protocol, uri);
            await _feedCollection.GetMoreItemsAsync(100);

            _loadedPosts = _feedCollection.ToList();
        }
        catch (Exception ex)
        {
            FeedError = $"Failed to load feed: {ex.Message}";
        }
        finally
        {
            IsLoadingFeed = false;
        }
    }

    /// <summary>
    /// Called when lines are cleared in the game.
    /// Reveals posts based on the number of lines cleared (1-4 posts).
    /// </summary>
    private void OnLinesCleared(int lineCount)
    {
        for (int i = 0; i < lineCount && _feedIndex < _loadedPosts.Count; i++)
        {
            RevealedPosts.Insert(0, _loadedPosts[_feedIndex]);
            _feedIndex++;
            TotalPostsRevealed++;
        }

        // Load more posts if we're running low
        if (_feedIndex >= _loadedPosts.Count - 10 && _feedCollection?.HasMoreItems == true)
        {
            _ = LoadMorePostsAsync();
        }
    }

    /// <summary>
    /// Loads additional posts from the feed.
    /// </summary>
    private async Task LoadMorePostsAsync()
    {
        if (_feedCollection == null || !_feedCollection.HasMoreItems)
            return;

        try
        {
            var previousCount = _loadedPosts.Count;
            await _feedCollection.GetMoreItemsAsync(50);

            var items = _feedCollection.ToList();
            for (int i = previousCount; i < items.Count; i++)
            {
                _loadedPosts.Add(items[i]);
            }
        }
        catch
        {
            // Silently fail - we have enough posts for now
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
        _feedIndex = 0;
        TotalPostsRevealed = 0;
        RevealedPosts.Clear();
        _loadedPosts.Clear();
        FeedError = null;

        await LoadFeedAsync(_currentOptions.FeedUri);

        StartEngine(_currentOptions.StartLevel);
    }
}
