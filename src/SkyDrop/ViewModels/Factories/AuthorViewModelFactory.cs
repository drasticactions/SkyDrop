using FishyFlip;
using FishyFlip.Models;

namespace SkyDrop.ViewModels.Factories;

public class AuthorViewModelFactory : IAuthorViewModelFactory
{
    private readonly ATProtocol protocol;

    public AuthorViewModelFactory(ATProtocol protocol)
    {
        this.protocol = protocol ?? throw new ArgumentNullException(nameof(protocol));
    }

    public AuthorViewModel Create(ATIdentifier identifier)
    {
        return new AuthorViewModel(identifier, protocol);
    }
}