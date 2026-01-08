using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using FishyFlip;
using SkyDrop.Events;
using SkyDrop.Models;
using SkyDrop.Services;

namespace SkyDrop.ViewModels;

public abstract partial class BlueskyViewModel : ViewModelBase, IDisposable
{
    public BlueskyViewModel(ATProtocol protocol)
    {
        if (protocol == null)
        {
            throw new ArgumentNullException(nameof(protocol));
        }

        this.Protocol = protocol;
        StrongReferenceMessenger.Default.Register<OnLoginUserEventArgs>(this, this.OnLoginUser);
    }

    /// <summary>
    /// Gets the <see cref="ATProtocol"/> instance.
    /// </summary>
    public ATProtocol Protocol { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsAuthenticated))]
    private LoginUser? _currentUser;

    public bool IsAuthenticated => this.CurrentUser is not null;

    protected virtual void OnLoginUser(object recipient, OnLoginUserEventArgs args)
    {
        this.CurrentUser = args.LoginUser;
    }

    /// <summary>
    /// Dispose the view model.
    /// </summary>
    public virtual void Dispose()
    {
        StrongReferenceMessenger.Default.Unregister<OnLoginUserEventArgs>(this);
    }
}