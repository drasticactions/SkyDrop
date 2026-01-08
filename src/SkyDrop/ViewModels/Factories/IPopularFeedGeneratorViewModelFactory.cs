using FishyFlip.Lexicon.App.Bsky.Unspecced;

namespace SkyDrop.ViewModels.Factories;

public interface IPopularFeedGeneratorViewModelFactory
{
    PopularFeedGeneratorViewModel Create();
    PopularFeedGeneratorViewModel Create(string query);
}