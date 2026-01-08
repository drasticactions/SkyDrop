using System.Text.Json;
using System.Text.RegularExpressions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FishyFlip;
using FishyFlip.Models;
using SkyDrop.Events;
using SkyDrop.Models;
using SkyDrop.Services;

namespace SkyDrop.ViewModels;

public partial class LoginViewModel : BlueskyViewModel
{
    private static readonly Regex EmailRegex = new(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", RegexOptions.Compiled);

    public LoginViewModel(ATProtocol protocol)
        : base(protocol)
    {
    }

    /// <summary>
    /// Event raised when the user requests to go back.
    /// </summary>
    public event Action? BackRequested;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LoginWithPasswordCommand))]
    [NotifyPropertyChangedFor(nameof(CanLoginWithPassword))]
    [NotifyPropertyChangedFor(nameof(CanLoginWithOauth))]
    private string _identifier = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LoginWithPasswordCommand))]
    [NotifyPropertyChangedFor(nameof(CanLoginWithPassword))]
    private string _password = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LoginWithPasswordCommand))]
    [NotifyPropertyChangedFor(nameof(CanLoginWithPassword))]
    [NotifyPropertyChangedFor(nameof(CanLoginWithOauth))]
    private bool _isLoggingInWithPassword;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LoginWithOAuthCommand))]
    [NotifyPropertyChangedFor(nameof(CanLoginWithPassword))]
    [NotifyPropertyChangedFor(nameof(CanLoginWithOauth))]
    private bool _isLoggingInWithOAuth;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    /// <summary>
    /// Gets a value indicating whether any login operation is in progress.
    /// </summary>
    public bool IsLoggingIn => IsLoggingInWithPassword || IsLoggingInWithOAuth;

    /// <summary>
    /// Gets a value indicating whether the user can log in with password.
    /// </summary>
    public bool CanLoginWithPassword => IsValidIdentifier() && !string.IsNullOrWhiteSpace(this.Password) && !this.IsLoggingIn;

    /// <summary>
    /// Gets a value indicating whether the user can log in with OAuth.
    /// </summary>
    public bool CanLoginWithOauth => IsValidOAuthIdentifier() && !this.IsLoggingIn;

    [RelayCommand(CanExecute = nameof(CanLoginWithPassword))]
    public async Task<bool> LoginWithPasswordAsync()
    {
        try
        {
            this.IsLoggingInWithPassword = true;
            this.ErrorMessage = string.Empty;
            var (result, error) = await this.Protocol.AuthenticateWithPasswordResultAsync(this.Identifier, this.Password);
            if (error != null)
            {
                this.ErrorMessage = error.Detail?.Message ?? "An unknown error occurred.";
                return false;
            }

            if (result != null)
            {
                var loginUser = new LoginUser
                {
                    Handle = result.Handle.Handle,
                    Email = result.Email ?? string.Empty,
                    Did = result.Did.ToString(),
                    SessionData = JsonSerializer.Serialize<Session>(result, SourceGenerationContext.Default.Session),
                    LoginType = LoginType.Password
                };

                StrongReferenceMessenger.Default.Send(new OnLoginUserEventArgs(loginUser));
            }

            return true;
        }
        catch (Exception ex)
        {
            this.ErrorMessage = ex.Message;
            return false;
        }
        finally
        {
            this.IsLoggingInWithPassword = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanLoginWithOauth))]
    public async Task<bool> LoginWithOAuthAsync()
    {
        try
        {
            this.IsLoggingInWithOAuth = true;
            this.ErrorMessage = string.Empty;

            // TODO: Implement OAuth login flow
            await Task.Delay(5000);

            return false;
        }
        catch (Exception ex)
        {
            this.ErrorMessage = ex.Message;
            return false;
        }
        finally
        {
            this.IsLoggingInWithOAuth = false;
        }
    }

    private bool IsValidIdentifier()
    {
        if (string.IsNullOrWhiteSpace(this.Identifier))
        {
            return false;
        }

        if (EmailRegex.IsMatch(this.Identifier))
        {
            return true;
        }

        return ATIdentifier.TryCreate(this.Identifier, out _);
    }

    private bool IsValidOAuthIdentifier()
    {
        if (string.IsNullOrWhiteSpace(this.Identifier))
        {
            return false;
        }

        if (EmailRegex.IsMatch(this.Identifier))
        {
            return false;
        }

        return ATIdentifier.TryCreate(this.Identifier, out _);
    }

    protected override void OnLoginUser(object recipient, OnLoginUserEventArgs args)
    {
        base.OnLoginUser(recipient, args);

        this.Password = string.Empty;
        this.IsLoggingInWithPassword = false;
        this.IsLoggingInWithOAuth = false;
        this.ErrorMessage = string.Empty;
        this.Identifier = string.Empty;
    }

    [RelayCommand]
    private void Back()
    {
        BackRequested?.Invoke();
    }

    [RelayCommand]
    private void Logout()
    {
        this.CurrentUser = null;

        StrongReferenceMessenger.Default.Send(new OnLoginUserEventArgs(null));

        this.Password = string.Empty;
        this.ErrorMessage = string.Empty;
        this.Identifier = string.Empty;
    }
}