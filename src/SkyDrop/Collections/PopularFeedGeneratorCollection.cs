// <copyright file="PopularFeedGeneratorCollection.cs" company="Drastic Actions">
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

public class PopularFeedGeneratorCollection : ATObjectCollectionBase<GeneratorView>, IAsyncEnumerable<GeneratorView>, IList<GeneratorView>, IList
{
    private ATProtocol atp;
    private string query = string.Empty;

    public PopularFeedGeneratorCollection(string query, ATProtocol atp)
        : base(atp)
    {
        this.atp = atp;
        this.query = query;
    }

    public PopularFeedGeneratorCollection(ATProtocol atp)
        : base(atp)
    {
        this.atp = atp;
        this.query = string.Empty;
    }

    public string Query
    {
        get => this.query;
        set
        {
            this.Clear();
            if (this.query != value)
            {
                this.query = value;
            }
        }
    }

    public ATProtocol ATProtocol => this.atp;

    public override async Task<(IList<GeneratorView> Posts, string Cursor)> GetRecordsAsync(int? limit = null, CancellationToken? cancellationToken = null)
    {
        cancellationToken?.ThrowIfCancellationRequested();
        var (result, error) = await this.ATProtocol.Unspecced.GetPopularFeedGeneratorsAsync(limit, this.Cursor, this.Query, cancellationToken ?? System.Threading.CancellationToken.None);
        this.HandleATError(error);
        if (result == null || result.Feeds == null)
        {
            throw new InvalidOperationException("The result or its properties cannot be null.");
        }

        return (result.Feeds, result.Cursor ?? string.Empty);
    }
    
    /// <inheritdoc/>
    public override Task RefreshAsync(int? limit = null, CancellationToken? cancellationToken = null)
    {
        cancellationToken?.ThrowIfCancellationRequested();
        this.Clear();
        return this.GetMoreItemsAsync(limit, cancellationToken);
    }

    #region IList<GeneratorView> Implementation (delegating to base)

    GeneratorView IList<GeneratorView>.this[int index]
    {
        get => base[index];
        set => throw new NotSupportedException();
    }

    int ICollection<GeneratorView>.Count => base.Count;

    bool ICollection<GeneratorView>.IsReadOnly => true;

    void ICollection<GeneratorView>.Add(GeneratorView item) => throw new NotSupportedException();

    bool ICollection<GeneratorView>.Contains(GeneratorView item)
    {
        for (int i = 0; i < base.Count; i++)
        {
            if (EqualityComparer<GeneratorView>.Default.Equals(base[i], item))
                return true;
        }
        return false;
    }

    void ICollection<GeneratorView>.CopyTo(GeneratorView[] array, int arrayIndex)
    {
        for (int i = 0; i < base.Count; i++)
        {
            array[arrayIndex + i] = base[i];
        }
    }

    int IList<GeneratorView>.IndexOf(GeneratorView item)
    {
        for (int i = 0; i < base.Count; i++)
        {
            if (EqualityComparer<GeneratorView>.Default.Equals(base[i], item))
                return i;
        }
        return -1;
    }

    void IList<GeneratorView>.Insert(int index, GeneratorView item) => throw new NotSupportedException();

    bool ICollection<GeneratorView>.Remove(GeneratorView item) => throw new NotSupportedException();

    void IList<GeneratorView>.RemoveAt(int index) => throw new NotSupportedException();

    IEnumerator<GeneratorView> IEnumerable<GeneratorView>.GetEnumerator()
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
        if (value is GeneratorView item)
            return ((ICollection<GeneratorView>)this).Contains(item);
        return false;
    }

    int IList.IndexOf(object? value)
    {
        if (value is GeneratorView item)
            return ((IList<GeneratorView>)this).IndexOf(item);
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