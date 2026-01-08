using Avalonia;
using Avalonia.Media;

namespace SkyDrop.Services;

/// <summary>
/// Service for managing custom font settings.
/// Allows toggling between custom game font and system default font.
/// </summary>
public class FontService
{
    private static FontService? _instance;
    public static FontService Instance => _instance ??= new FontService();

    private bool _useCustomFont = true;

    public bool UseCustomFont => _useCustomFont;

    public event Action? FontChanged;

    private static readonly FontFamily CustomFontFamily = new("avares://SkyDrop/Assets/NuKinakoMochi-Reg.otf#Nu Kinako Mochi");
    private static readonly FontFamily DefaultFontFamily = FontFamily.Default;

    private FontService()
    {
    }

    /// <summary>
    /// Call this after Application is initialized to set up font resources.
    /// </summary>
    public void Initialize()
    {
        UpdateFontResource();
    }

    /// <summary>
    /// Toggles between custom font and system default font.
    /// </summary>
    public void ToggleCustomFont()
    {
        _useCustomFont = !_useCustomFont;
        UpdateFontResource();
        FontChanged?.Invoke();
    }

    /// <summary>
    /// Sets whether to use the custom font.
    /// </summary>
    public void SetCustomFont(bool useCustom)
    {
        if (_useCustomFont == useCustom) return;

        _useCustomFont = useCustom;
        UpdateFontResource();
        FontChanged?.Invoke();
    }

    private void UpdateFontResource()
    {
        if (Application.Current == null) return;

        var font = _useCustomFont ? CustomFontFamily : DefaultFontFamily;
        Application.Current.Resources["AppFontFamily"] = font;
    }
}
