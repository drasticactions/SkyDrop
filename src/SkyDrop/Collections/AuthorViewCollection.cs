// <copyright file="AuthorViewCollection.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using FishyFlip;
using FishyFlip.Lexicon.App.Bsky.Feed;
using FishyFlip.Models;

namespace SkyDrop.Collections;

/// <summary>
/// Author View Collection.
/// </summary>
public class AuthorViewCollection : FeedViewPostCollection
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AuthorViewCollection"/> class.
    /// </summary>
    /// <param name="atProtocol">The ATProtocol.</param>
    /// <param name="identifier">The ATIdentifier.</param>
    /// <param name="filter">The Filter.</param>
    /// <param name="includePins">Include pins.</param>
    public AuthorViewCollection(ATProtocol atp, ATIdentifier identifier, string filter = "", bool includePins = true)
        : base(atp)
    {
        this.ATIdentifier = identifier;
        this.Filter = filter;
        this.IncludePins = includePins;
    }

    /// <summary>
    /// Gets the ATIdentifier.
    /// </summary>
    public ATIdentifier ATIdentifier { get; }

    /// <summary>
    /// Gets the filter.
    /// </summary>
    public string Filter { get; }

    /// <summary>
    /// Gets a value indicating whether to include pins.
    /// </summary>
    public bool IncludePins { get; }

    /// <inheritdoc/>
    public override async Task<(IList<FeedViewPost> Posts, string Cursor)> GetRecordsAsync(int? limit = null, CancellationToken? cancellationToken = null)
    {
        await this.GetMoreItemsAsync(limit, cancellationToken ?? System.Threading.CancellationToken.None);
        return (this.ToList(), this.Cursor ?? string.Empty);
    }

    /// <inheritdoc/>
    internal override async Task<(IList<FeedViewPost> Posts, string Cursor)> GetPostViewItemsAsync(int limit = 50, CancellationToken? token = default)
    {
        var (result, error) = await this.ATProtocol.Feed.GetAuthorFeedAsync(this.ATIdentifier, limit, this.Cursor, this.Filter, this.IncludePins, token ?? System.Threading.CancellationToken.None);

        this.HandleATError(error);
        if (result == null || result.Feed == null)
        {
            throw new InvalidOperationException("The result or its properties cannot be null.");
        }

        return (result.Feed, result.Cursor ?? string.Empty);
    }
}