using System.Text.Json;
using System.Text.RegularExpressions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FishyFlip;
using FishyFlip.Lexicon.App.Bsky.Feed;
using FishyFlip.Models;
using SkyDrop.Services;

namespace SkyDrop.ViewModels;

public partial class ThreadViewPostViewModel : BlueskyViewModel
{
    private ATUri _uri;
    public ThreadViewPostViewModel(ThreadViewPost post, ATProtocol protocol)
        : base(protocol)
    {
        this.Post = post;
        this._uri = post.Post.Uri;
    }

    public ThreadViewPostViewModel(ATUri uri, ATProtocol protocol)
        : base(protocol)
    {
        this._uri = uri;
    }

    [ObservableProperty]
    private ThreadViewPost? _post;

    public async Task RefreshAsync(CancellationToken? token = default)
    {
        var (post, error) = await this.Protocol.Feed.GetPostThreadAsync(this._uri, cancellationToken: token ?? CancellationToken.None);
        if (post?.Thread is ThreadViewPost thread)
        {
            this.Post = thread;
        }
    }
}