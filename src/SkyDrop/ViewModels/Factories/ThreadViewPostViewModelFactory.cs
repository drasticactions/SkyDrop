using FishyFlip;
using FishyFlip.Lexicon.App.Bsky.Feed;
using FishyFlip.Models;
using SkyDrop.Services;

namespace SkyDrop.ViewModels.Factories;

public class ThreadViewPostViewModelFactory : IThreadViewPostViewModelFactory
{
    private readonly ATProtocol protocol;

    public ThreadViewPostViewModelFactory(ATProtocol protocol)
    {
        this.protocol = protocol ?? throw new ArgumentNullException(nameof(protocol));
    }

    public ThreadViewPostViewModel Create(ThreadViewPost post)
    {
        return new ThreadViewPostViewModel(post, protocol);
    }

    public ThreadViewPostViewModel Create(ATUri uri)
    {
        return new ThreadViewPostViewModel(uri, protocol);
    }
}