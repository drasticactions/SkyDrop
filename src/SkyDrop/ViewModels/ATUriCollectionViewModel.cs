using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FishyFlip;
using FishyFlip.Lexicon.App.Bsky.Feed;
using FishyFlip.Models;
using SkyDrop.Collections;

namespace SkyDrop.ViewModels;

public partial class ATUriCollectionViewModel : ObservableObject
{
    private readonly ATProtocol protocol;
    private FeedViewCollection? feedCollection;

    public ATUriCollectionViewModel(ATProtocol protocol)
    {
        this.protocol = protocol;
    }

    [ObservableProperty]
    private string feedUri = "at://did:plc:z72i7hdynmk6r22z27h6tvur/app.bsky.feed.generator/whats-hot";

    /// <summary>
    /// Gets the observable collection of feed items for UI binding.
    /// </summary>
    public ObservableCollection<FeedViewPost> FeedItems { get; } = new();

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private string? errorMessage;

    [ObservableProperty]
    private bool hasError;

    [ObservableProperty]
    private bool hasFeedItems;

    [RelayCommand]
    private async Task LoadFeedAsync()
    {
        if (string.IsNullOrWhiteSpace(FeedUri))
        {
            ErrorMessage = "Please enter a valid ATUri";
            HasError = true;
            return;
        }

        try
        {
            IsLoading = true;
            HasError = false;
            ErrorMessage = null;
            FeedItems.Clear();

            var uri = ATUri.Create(FeedUri);
            if (uri == null)
            {
                ErrorMessage = "Invalid ATUri format";
                HasError = true;
                return;
            }

            feedCollection = new FeedViewCollection(protocol, uri);
            await feedCollection.GetMoreItemsAsync(50);

            foreach (var item in feedCollection)
            {
                FeedItems.Add(item);
            }

            HasFeedItems = FeedItems.Count > 0;
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            HasError = true;
            feedCollection = null;
            HasFeedItems = false;
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task LoadMoreAsync()
    {
        if (feedCollection == null || !feedCollection.HasMoreItems || IsLoading)
            return;

        try
        {
            IsLoading = true;
            var previousCount = FeedItems.Count;
            await feedCollection.GetMoreItemsAsync(50);

            // Add only the new items
            var items = feedCollection.ToList();
            for (int i = previousCount; i < items.Count; i++)
            {
                FeedItems.Add(items[i]);
            }

            HasFeedItems = FeedItems.Count > 0;
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            HasError = true;
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        if (feedCollection == null || IsLoading)
            return;

        try
        {
            IsLoading = true;
            HasError = false;
            ErrorMessage = null;
            FeedItems.Clear();

            await feedCollection.RefreshAsync(50);

            foreach (var item in feedCollection)
            {
                FeedItems.Add(item);
            }

            HasFeedItems = FeedItems.Count > 0;
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            HasError = true;
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void ClearFeed()
    {
        FeedItems.Clear();
        feedCollection = null;
        HasError = false;
        ErrorMessage = null;
        HasFeedItems = false;
    }
}
