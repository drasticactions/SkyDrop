using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace SkyDrop.ViewModels;

/// <summary>
/// View model for the Credits screen.
/// </summary>
public partial class CreditsViewModel : ViewModelBase
{
    /// <summary>
    /// Event raised when the user requests to go back.
    /// </summary>
    public event Action? BackRequested;

    /// <summary>
    /// Gets the game website URL.
    /// </summary>
    public string WebsiteUrl => "https://github.com/drasticactions/skydrop";

    /// <summary>
    /// Gets the list of third-party libraries used in the game.
    /// </summary>
    public IReadOnlyList<LibraryInfo> Libraries { get; } = new List<LibraryInfo>
    {
        new("Avalonia", "Cross-platform .NET UI framework", "https://avaloniaui.net"),
        new("AsyncImageLoader.Avalonia", "Asynchronous image loading for Avalonia", "https://github.com/AvaloniaUtils/AsyncImageLoader.Avalonia"),
        new("CommunityToolkit.Mvvm", "MVVM Toolkit for .NET", "https://github.com/CommunityToolkit/dotnet"),
        new("FishyFlip", "ATProtocol/Bluesky .NET library", "https://github.com/drasticactions/FishyFlip"),
        new("NuKinakoMochi", "Japanese Console Font", "https://kokagem.sakura.ne.jp/font/mochi/"),
        new("SkiaSharp", "Cross-platform 2D graphics library", "https://github.com/mono/SkiaSharp"),
        new("Microsoft.Extensions.DependencyInjection", "Dependency injection container", "https://github.com/dotnet/runtime"),
    };

    [RelayCommand]
    private void Back()
    {
        BackRequested?.Invoke();
    }

    [RelayCommand]
    private void OpenWebsite()
    {
        OpenUrl(WebsiteUrl);
    }

    [RelayCommand]
    private void OpenLibraryUrl(string url)
    {
        if (!string.IsNullOrEmpty(url))
        {
            OpenUrl(url);
        }
    }

    private static void OpenUrl(string url)
    {
        try
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }
        catch
        {
            // Silently fail if we can't open the URL
        }
    }
}

/// <summary>
/// Represents information about a third-party library.
/// </summary>
public record LibraryInfo(string Name, string Description, string Url);
