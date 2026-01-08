using FishyFlip;
using FishyFlip.Models;
using SkyDrop.Collections;
using SkyDrop.Events;
using SkyDrop.Services;

namespace SkyDrop.ViewModels;

public partial class AuthorViewModel : BlueskyViewModel
{
    private ATIdentifier atIdentifier;

    public AuthorViewModel(ATIdentifier identifier, ATProtocol protocol)
        : base(protocol)
    {
        this.atIdentifier = identifier;
        this.MainAuthorFeed = new AuthorViewCollection(protocol, identifier, AuthorFilterConstants.PostsAndAuthorThreads, true);
        this.RepliesFeed = new AuthorViewCollection(protocol, identifier, AuthorFilterConstants.PostsWithReplies, false);
        this.VideosFeed = new AuthorViewCollection(protocol, identifier, AuthorFilterConstants.PostsWithVideo, false);
        this.MediaFeed = new AuthorViewCollection(protocol, identifier, AuthorFilterConstants.PostsWithMedia, false);
    }
    
    public AuthorViewCollection MainAuthorFeed { get; }

    public AuthorViewCollection RepliesFeed { get; }

    public AuthorViewCollection VideosFeed { get; }

    public AuthorViewCollection MediaFeed { get; }
}