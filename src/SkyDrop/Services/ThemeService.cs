using Avalonia;
using Avalonia.Platform;
using Avalonia.Styling;

namespace SkyDrop.Services;

/// <summary>
/// Service for managing application theme (light/dark mode).
/// Follows system theme by default, but remembers user override.
/// </summary>
public class ThemeService
{
    private static ThemeService? _instance;
    public static ThemeService Instance => _instance ??= new ThemeService();

    private bool _userHasOverridden;

    public bool IsDarkMode => GetCurrentThemeIsDark();

    public event Action? ThemeChanged;

    private ThemeService()
    {
        _userHasOverridden = false;
    }

    /// <summary>
    /// Call this after Application is initialized to subscribe to system theme changes.
    /// </summary>
    public void Initialize()
    {
        if (Application.Current != null)
        {
            // Subscribe to theme changes (both system and app-triggered)
            Application.Current.ActualThemeVariantChanged += OnThemeVariantChanged;

            // Start in Default mode (follows system)
            Application.Current.RequestedThemeVariant = ThemeVariant.Default;
        }
    }

    private void OnThemeVariantChanged(object? sender, EventArgs e)
    {
        // Notify listeners whenever the actual theme changes
        ThemeChanged?.Invoke();
    }

    private bool GetCurrentThemeIsDark()
    {
        if (Application.Current == null) return true;
        return Application.Current.ActualThemeVariant == ThemeVariant.Dark;
    }

    private bool GetSystemThemeIsDark()
    {
        if (Application.Current == null) return true;
        var platformTheme = Application.Current.PlatformSettings?.GetColorValues().ThemeVariant;
        return platformTheme != PlatformThemeVariant.Light;
    }

    /// <summary>
    /// Toggles the theme and marks as user-overridden.
    /// </summary>
    public void ToggleTheme()
    {
        _userHasOverridden = true;
        var currentIsDark = GetCurrentThemeIsDark();

        if (Application.Current != null)
        {
            // Set explicit theme (opposite of current)
            Application.Current.RequestedThemeVariant = currentIsDark
                ? ThemeVariant.Light
                : ThemeVariant.Dark;
        }
    }

    /// <summary>
    /// Sets a specific theme and marks as user-overridden.
    /// </summary>
    public void SetTheme(bool isDark)
    {
        _userHasOverridden = true;

        if (Application.Current != null)
        {
            Application.Current.RequestedThemeVariant = isDark
                ? ThemeVariant.Dark
                : ThemeVariant.Light;
        }
    }

    /// <summary>
    /// Resets to follow system theme.
    /// </summary>
    public void ResetToSystemTheme()
    {
        _userHasOverridden = false;

        if (Application.Current != null)
        {
            // ThemeVariant.Default means follow system
            Application.Current.RequestedThemeVariant = ThemeVariant.Default;
        }
    }

    /// <summary>
    /// Whether the user has manually set a theme preference.
    /// </summary>
    public bool UserHasOverridden => _userHasOverridden;
}
