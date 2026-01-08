using FishyFlip.Lexicon.App.Bsky.Feed;
using FishyFlip.Models;

namespace SkyDrop.ViewModels.Factories;

public interface IThreadViewPostViewModelFactory
{
    ThreadViewPostViewModel Create(ThreadViewPost post);
    ThreadViewPostViewModel Create(ATUri uri);
}