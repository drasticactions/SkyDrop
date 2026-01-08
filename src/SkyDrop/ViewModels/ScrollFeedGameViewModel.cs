using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using FishyFlip;
using FishyFlip.Lexicon.App.Bsky.Feed;

namespace SkyDrop.ViewModels;

/// <summary>
/// View model for feed game modes where the feed is scrolled by rotating pieces.
/// </summary>
public partial class ScrollFeedGameViewModel : FeedGameViewModelBase
{
    /// <summary>
    /// Gets the collection of posts loaded from the feed.
    /// </summary>
    public ObservableCollection<FeedViewPost> FeedPosts { get; } = new();

    [ObservableProperty]
    private double _scrollOffset;

    [ObservableProperty]
    private int _totalRotations;

    /// <summary>
    /// Event raised when a scroll is requested.
    /// Parameter is true for scroll down (clockwise), false for scroll up (counter-clockwise).
    /// </summary>
    public event Action<bool>? RequestScroll;

    public ScrollFeedGameViewModel(ATProtocol protocol) : base(protocol)
    {
        _engine.OnRotation += OnRotation;
    }

    /// <inheritdoc/>
    protected override void ResetModeSpecificState()
    {
        ScrollOffset = 0;
        TotalRotations = 0;
        FeedPosts.Clear();
    }

    /// <inheritdoc/>
    protected override int GetInitialLoadCount() => 50;

    /// <inheritdoc/>
    protected override void OnFeedLoaded()
    {
        if (_feedCollection == null)
            return;

        foreach (var post in _feedCollection)
        {
            FeedPosts.Add(post);
        }
    }

    /// <inheritdoc/>
    protected override void OnMorePostsLoaded()
    {
        if (_feedCollection == null)
            return;

        var items = _feedCollection.ToList();
        for (int i = FeedPosts.Count; i < items.Count; i++)
        {
            FeedPosts.Add(items[i]);
        }
    }

    /// <summary>
    /// Called when a rotation is performed.
    /// Scrolls the feed based on rotation direction.
    /// </summary>
    private void OnRotation(bool clockwise)
    {
        TotalRotations++;

        // Clockwise = scroll down, Counter-clockwise = scroll up
        RequestScroll?.Invoke(clockwise);
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
                _ = LoadMorePostsAsync(25);
            }
        }
    }
}
