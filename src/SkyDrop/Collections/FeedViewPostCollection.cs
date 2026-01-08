// <copyright file="FeedViewPostCollection.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using CommunityToolkit.Mvvm.ComponentModel;
using FishyFlip;
using FishyFlip.Lexicon;
using FishyFlip.Lexicon.App.Bsky.Feed;
using FishyFlip.Models;
using FishyFlip.Tools;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace SkyDrop.Collections;

/// <summary>
/// Enumerable collection of PostView objects.
/// </summary>
public abstract class FeedViewPostCollection : ATObjectCollectionBase<FeedViewPost>, IAsyncEnumerable<FeedViewPost>, IList<FeedViewPost>, IList
{
    private ATProtocol atp;

    protected FeedViewPostCollection(ATProtocol atp) : base(atp)
    {
        this.atp = atp;
    }

    public ATProtocol ATProtocol => this.atp;

    /// <inheritdoc/>
    public override async Task GetMoreItemsAsync(int? limit = null, CancellationToken? cancellationToken = null)
    {
        cancellationToken?.ThrowIfCancellationRequested();
        var (postViews, cursor) = await this.GetPostViewItemsAsync(limit ?? 50, cancellationToken);
        foreach (var postView in postViews)
        {
            this.AddItem(postView);
        }

        this.HasMoreItems = !string.IsNullOrEmpty(cursor);
        this.Cursor = cursor;
    }

    /// <inheritdoc/>
    public override Task RefreshAsync(int? limit = null, CancellationToken? cancellationToken = null)
    {
        cancellationToken?.ThrowIfCancellationRequested();
        this.Clear();
        return this.GetMoreItemsAsync(limit, cancellationToken);
    }

    /// <summary>
    /// Get Post View Items.
    /// </summary>
    /// <param name="limit">Limit of items to fetch.</param>
    /// <param name="token">Cancellation Token.</param>
    /// <returns>Task.</returns>
    internal abstract Task<(IList<FeedViewPost> Posts, string Cursor)> GetPostViewItemsAsync(int limit = 50, CancellationToken? token = null);

    #region IList<FeedViewPost> Implementation (delegating to base)

    FeedViewPost IList<FeedViewPost>.this[int index]
    {
        get => base[index];
        set => throw new NotSupportedException();
    }

    int ICollection<FeedViewPost>.Count => base.Count;

    bool ICollection<FeedViewPost>.IsReadOnly => true;

    void ICollection<FeedViewPost>.Add(FeedViewPost item) => throw new NotSupportedException();

    bool ICollection<FeedViewPost>.Contains(FeedViewPost item)
    {
        for (int i = 0; i < base.Count; i++)
        {
            if (EqualityComparer<FeedViewPost>.Default.Equals(base[i], item))
                return true;
        }
        return false;
    }

    void ICollection<FeedViewPost>.CopyTo(FeedViewPost[] array, int arrayIndex)
    {
        for (int i = 0; i < base.Count; i++)
        {
            array[arrayIndex + i] = base[i];
        }
    }

    int IList<FeedViewPost>.IndexOf(FeedViewPost item)
    {
        for (int i = 0; i < base.Count; i++)
        {
            if (EqualityComparer<FeedViewPost>.Default.Equals(base[i], item))
                return i;
        }
        return -1;
    }

    void IList<FeedViewPost>.Insert(int index, FeedViewPost item) => throw new NotSupportedException();

    bool ICollection<FeedViewPost>.Remove(FeedViewPost item) => throw new NotSupportedException();

    void IList<FeedViewPost>.RemoveAt(int index) => throw new NotSupportedException();

    IEnumerator<FeedViewPost> IEnumerable<FeedViewPost>.GetEnumerator()
    {
        for (int i = 0; i < base.Count; i++)
        {
            yield return base[i];
        }
    }

    #endregion

    #region IList Implementation (delegating to base)

    object? IList.this[int index]
    {
        get => base[index];
        set => throw new NotSupportedException();
    }

    bool IList.IsFixedSize => false;

    bool IList.IsReadOnly => true;

    bool ICollection.IsSynchronized => false;

    object ICollection.SyncRoot => this;

    int IList.Add(object? value) => throw new NotSupportedException();

    bool IList.Contains(object? value)
    {
        if (value is FeedViewPost item)
            return ((ICollection<FeedViewPost>)this).Contains(item);
        return false;
    }

    int IList.IndexOf(object? value)
    {
        if (value is FeedViewPost item)
            return ((IList<FeedViewPost>)this).IndexOf(item);
        return -1;
    }

    void IList.Insert(int index, object? value) => throw new NotSupportedException();

    void IList.Remove(object? value) => throw new NotSupportedException();

    void IList.RemoveAt(int index) => throw new NotSupportedException();

    void ICollection.CopyTo(Array array, int index)
    {
        for (int i = 0; i < base.Count; i++)
        {
            array.SetValue(base[i], index + i);
        }
    }

    #endregion
}