using FishyFlip;
using FishyFlip.Models;

namespace SkyDrop.ViewModels.Factories;

public interface IAuthorViewModelFactory
{
    AuthorViewModel Create(ATIdentifier identifier);
}