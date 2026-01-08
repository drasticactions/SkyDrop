using CommunityToolkit.Mvvm.ComponentModel;
using FishyFlip;
using FishyFlip.Lexicon.App.Bsky.Feed;
using FishyFlip.Models;
using SkyDrop.Collections;
using SkyDrop.Models;
using SkyDrop.Resources;

namespace SkyDrop.ViewModels;

/// <summary>
/// Base class for feed-based game mode view models.
/// </summary>
public abstract partial class FeedGameViewModelBase : GameViewModelBase
{
    protected readonly ATProtocol _protocol;
    protected FeedModeOptions? _currentOptions;
    protected FeedViewPostCollection? _feedCollection;

    [ObservableProperty]
    private bool _isLoadingFeed;

    [ObservableProperty]
    private string? _feedError;

    [ObservableProperty]
    private string _feedName = Strings.FeedNameDefault;

    protected FeedGameViewModelBase(ATProtocol protocol)
    {
        _protocol = protocol;
    }

    /// <summary>
    /// Prepares the game and shows instructions.
    /// </summary>
    public async Task PrepareGameAsync(FeedModeOptions options)
    {
        _currentOptions = options;
        ShowInstructions = true;
        IsGameOver = false;
        IsPaused = false;
        FeedError = null;
        FeedName = options.FeedName;

        ResetModeSpecificState();

        await LoadFeedAsync();
    }

    /// <summary>
    /// Starts a new game with the specified options (shows instructions first).
    /// </summary>
    public async Task StartGameAsync(FeedModeOptions options)
    {
        await PrepareGameAsync(options);
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
    /// Restarts the game with the same options (skips instructions).
    /// </summary>
    public async Task RestartGameAsync()
    {
        if (_currentOptions == null)
            return;

        ShowInstructions = false;
        IsGameOver = false;
        IsPaused = false;
        FeedError = null;

        ResetModeSpecificState();

        await LoadFeedAsync();

        StartEngine(_currentOptions.StartLevel);
    }

    /// <summary>
    /// Loads posts from the feed source specified in current options.
    /// </summary>
    protected async Task LoadFeedAsync()
    {
        if (_currentOptions == null)
            return;

        try
        {
            IsLoadingFeed = true;
            FeedError = null;

            _feedCollection = CreateFeedCollection();
            if (_feedCollection == null)
            {
                FeedError = Strings.ErrorFailedToCreateFeedCollection;
                return;
            }

            await _feedCollection.GetMoreItemsAsync(GetInitialLoadCount());
            OnFeedLoaded();
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
    /// Creates the appropriate feed collection based on the source type.
    /// </summary>
    protected virtual FeedViewPostCollection? CreateFeedCollection()
    {
        if (_currentOptions == null)
            return null;

        return _currentOptions.SourceType switch
        {
            FeedSourceType.DiscoverFeed => CreateDiscoverFeedCollection(),
            FeedSourceType.AuthorFeed => CreateAuthorFeedCollection(),
            FeedSourceType.Timeline => CreateTimelineCollection(),
            _ => null
        };
    }

    private FeedViewCollection? CreateDiscoverFeedCollection()
    {
        if (string.IsNullOrEmpty(_currentOptions?.FeedUri))
            return null;

        var uri = ATUri.Create(_currentOptions.FeedUri);
        if (uri == null)
        {
            FeedError = Strings.ErrorInvalidFeedUri;
            return null;
        }

        return new FeedViewCollection(_protocol, uri);
    }

    private AuthorViewCollection? CreateAuthorFeedCollection()
    {
        if (string.IsNullOrEmpty(_currentOptions?.AuthorIdentifier))
            return null;

        var identifier = ATIdentifier.Create(_currentOptions.AuthorIdentifier);
        if (identifier == null)
        {
            FeedError = Strings.ErrorInvalidAuthorIdentifier;
            return null;
        }

        return new AuthorViewCollection(
            _protocol,
            identifier,
            _currentOptions.AuthorFilter ?? string.Empty
        );
    }

    private TimelineViewCollection CreateTimelineCollection()
    {
        return new TimelineViewCollection(_protocol);
    }

    /// <summary>
    /// Loads additional posts from the feed collection.
    /// </summary>
    protected async Task LoadMorePostsAsync(int count = 50)
    {
        if (_feedCollection == null || !_feedCollection.HasMoreItems || IsLoadingFeed)
            return;

        try
        {
            IsLoadingFeed = true;
            await _feedCollection.GetMoreItemsAsync(count);
            OnMorePostsLoaded();
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
    /// Gets the number of posts to load initially.
    /// Override in derived classes to customize.
    /// </summary>
    protected virtual int GetInitialLoadCount() => 50;

    /// <summary>
    /// Called after the initial feed load completes successfully.
    /// Override in derived classes to process loaded posts.
    /// </summary>
    protected virtual void OnFeedLoaded() { }

    /// <summary>
    /// Called after loading more posts completes successfully.
    /// Override in derived classes to process newly loaded posts.
    /// </summary>
    protected virtual void OnMorePostsLoaded() { }

    /// <summary>
    /// Resets mode-specific state when preparing or restarting the game.
    /// Override in derived classes to reset mode-specific properties.
    /// </summary>
    protected abstract void ResetModeSpecificState();
}
