using System.Text.Json;
using System.Text.RegularExpressions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FishyFlip;
using FishyFlip.Models;
using SkyDrop;
using SkyDrop.Collections;
using SkyDrop.Services;

namespace SkyDrop.ViewModels;

public partial class PopularFeedGeneratorViewModel : BlueskyViewModel
{
    public PopularFeedGeneratorViewModel(ATProtocol protocol)
        : base(protocol)
    {
        this.Generators = new PopularFeedGeneratorCollection(protocol);
    }

    public PopularFeedGeneratorViewModel(string query, ATProtocol protocol)
        : base(protocol)
    {
        this.Generators = new PopularFeedGeneratorCollection(query, protocol);
    }

    [ObservableProperty]
    private string query = string.Empty;

    [RelayCommand]
    public async Task RefreshGeneratorsAsync()
    {
        if (this.IsRefreshing)
        {
            return;
        }

        try
        {
            this.IsRefreshing = true;
            await this.Generators.RefreshAsync(20).ConfigureAwait(false);
        }
        finally
        {
            this.IsRefreshing = false;
        }
    }

    [ObservableProperty]
    private bool isRefreshing;

    public PopularFeedGeneratorCollection Generators { get; }
}