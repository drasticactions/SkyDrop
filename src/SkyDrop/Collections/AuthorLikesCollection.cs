using FishyFlip;
using FishyFlip.Lexicon.App.Bsky.Feed;
using FishyFlip.Models;

namespace SkyDrop.Collections;

/// <summary>
/// Author Likes Collection.
/// </summary>
public class AuthorLikesCollection : FeedViewPostCollection
{
    private ATIdentifier atIdentifier;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthorLikesCollection"/> class.
    /// </summary>
    /// <param name="atProtocol">The ATProtocol.</param>
    public AuthorLikesCollection(ATProtocol atp)
        : base(atp)
    {
        this.atIdentifier = atp.Session?.Did ?? throw new ArgumentNullException("Must be authenticated with a session to use this collection");
    }

    /// <inheritdoc/>
    public override async Task<(IList<FeedViewPost> Posts, string Cursor)> GetRecordsAsync(int? limit = null, CancellationToken? cancellationToken = null)
    {
        await this.GetMoreItemsAsync(limit, cancellationToken ?? System.Threading.CancellationToken.None);
        return (this.ToList(), this.Cursor ?? string.Empty);
    }

    /// <inheritdoc/>
    internal override async Task<(IList<FeedViewPost> Posts, string Cursor)> GetPostViewItemsAsync(int limit = 50, CancellationToken? token = default)
    {
        var (result, error) = await this.ATProtocol.Feed.GetActorLikesAsync(this.atIdentifier, limit, this.Cursor, token ?? System.Threading.CancellationToken.None);

        this.HandleATError(error);
        if (result == null || result.Feed == null)
        {
            throw new InvalidOperationException("The result or its properties cannot be null.");
        }

        return (result.Feed, result.Cursor ?? string.Empty);
    }
}
