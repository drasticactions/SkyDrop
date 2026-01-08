using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using FishyFlip;
using FishyFlip.Lexicon.App.Bsky.Feed;

namespace SkyDrop.ViewModels;

/// <summary>
/// View model for feed game modes where posts are revealed by clearing lines.
/// </summary>
public partial class RevealFeedGameViewModel : FeedGameViewModelBase
{
    private List<FeedViewPost> _loadedPosts = new();
    private int _feedIndex;

    /// <summary>
    /// Gets the collection of posts revealed during gameplay.
    /// Posts are added as the player clears lines.
    /// </summary>
    public ObservableCollection<FeedViewPost> RevealedPosts { get; } = new();

    [ObservableProperty]
    private int _totalPostsRevealed;

    public RevealFeedGameViewModel(ATProtocol protocol) : base(protocol)
    {
        _engine.OnLinesCleared += OnLinesCleared;
    }

    /// <inheritdoc/>
    protected override void ResetModeSpecificState()
    {
        _feedIndex = 0;
        TotalPostsRevealed = 0;
        RevealedPosts.Clear();
        _loadedPosts.Clear();
    }

    /// <inheritdoc/>
    protected override int GetInitialLoadCount() => 100;

    /// <inheritdoc/>
    protected override void OnFeedLoaded()
    {
        _loadedPosts = _feedCollection?.ToList() ?? new List<FeedViewPost>();
    }

    /// <inheritdoc/>
    protected override void OnMorePostsLoaded()
    {
        if (_feedCollection == null)
            return;

        var items = _feedCollection.ToList();
        for (int i = _loadedPosts.Count; i < items.Count; i++)
        {
            _loadedPosts.Add(items[i]);
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

        if (_feedIndex >= _loadedPosts.Count - 10 && _feedCollection?.HasMoreItems == true)
        {
            _ = LoadMorePostsAsync(50);
        }
    }
}
