// <copyright file="TimelineViewCollection.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using FishyFlip;
using FishyFlip.Lexicon.App.Bsky.Feed;

namespace SkyDrop.Collections;

/// <summary>
/// Timeline View Collection - fetches the authenticated user's home timeline.
/// Requires authentication.
/// </summary>
public class TimelineViewCollection : FeedViewPostCollection
{
    private readonly string? _algorithm;

    /// <summary>
    /// Initializes a new instance of the <see cref="TimelineViewCollection"/> class.
    /// </summary>
    /// <param name="atp">The ATProtocol.</param>
    /// <param name="algorithm">Optional algorithm variant for timeline. Implementation-specific.</param>
    public TimelineViewCollection(ATProtocol atp, string? algorithm = null)
        : base(atp)
    {
        _algorithm = algorithm;
    }

    /// <summary>
    /// Gets the algorithm variant (if any).
    /// </summary>
    public string? Algorithm => _algorithm;

    /// <inheritdoc/>
    public override async Task<(IList<FeedViewPost> Posts, string Cursor)> GetRecordsAsync(int? limit = null, CancellationToken? cancellationToken = null)
    {
        await this.GetMoreItemsAsync(limit, cancellationToken ?? System.Threading.CancellationToken.None);
        return (this.ToList(), this.Cursor ?? string.Empty);
    }

    /// <inheritdoc/>
    internal override async Task<(IList<FeedViewPost> Posts, string Cursor)> GetPostViewItemsAsync(int limit = 50, CancellationToken? token = default)
    {
        var (result, error) = await this.ATProtocol.Feed.GetTimelineAsync(
            _algorithm,
            limit,
            this.Cursor,
            token ?? System.Threading.CancellationToken.None);

        this.HandleATError(error);
        if (result == null || result.Feed == null)
        {
            throw new InvalidOperationException("The result or its properties cannot be null.");
        }

        return (result.Feed, result.Cursor ?? string.Empty);
    }
}
