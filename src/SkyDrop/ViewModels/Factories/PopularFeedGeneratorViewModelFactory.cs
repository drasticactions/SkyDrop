using FishyFlip;
using SkyDrop.Services;

namespace SkyDrop.ViewModels.Factories;

public class PopularFeedGeneratorViewModelFactory : IPopularFeedGeneratorViewModelFactory
{
    private readonly ATProtocol protocol;

    public PopularFeedGeneratorViewModelFactory(ATProtocol protocol)
    {
        this.protocol = protocol ?? throw new ArgumentNullException(nameof(protocol));
    }

    public PopularFeedGeneratorViewModel Create()
    {
        return new PopularFeedGeneratorViewModel(protocol);
    }

    public PopularFeedGeneratorViewModel Create(string query)
    {
        return new PopularFeedGeneratorViewModel(query, protocol);
    }
}