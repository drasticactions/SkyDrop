// <copyright file="FeedViewCollection.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using FishyFlip;
using FishyFlip.Lexicon.App.Bsky.Feed;
using FishyFlip.Models;

namespace SkyDrop.Collections;

/// <summary>
/// Feed View Collection.
/// </summary>
public class FeedViewCollection : FeedViewPostCollection
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FeedViewCollection"/> class.
    /// </summary>
    /// <param name="atProtocol">The ATProtocol.</param>
    /// <param name="identifier">The ATIdentifier.</param>
    public FeedViewCollection(ATProtocol atp, ATUri feed)
        : base(atp)
    {
        this.FeedUri = feed;
    }

    /// <summary>
    /// Gets the ATUri.
    /// </summary>
    public ATUri FeedUri { get; }

    /// <inheritdoc/>
    public override async Task<(IList<FeedViewPost> Posts, string Cursor)> GetRecordsAsync(int? limit = null, CancellationToken? cancellationToken = null)
    {
        await this.GetMoreItemsAsync(limit, cancellationToken ?? System.Threading.CancellationToken.None);
        return (this.ToList(), this.Cursor ?? string.Empty);
    }

    /// <inheritdoc/>
    internal override async Task<(IList<FeedViewPost> Posts, string Cursor)> GetPostViewItemsAsync(int limit = 50, CancellationToken? token = default)
    {
        var (result, error) = await this.ATProtocol.Feed.GetFeedAsync(this.FeedUri, limit, this.Cursor, token ?? System.Threading.CancellationToken.None);

        this.HandleATError(error);
        if (result == null || result.Feed == null)
        {
            throw new InvalidOperationException("The result or its properties cannot be null.");
        }

        return (result.Feed, result.Cursor ?? string.Empty);
    }
}