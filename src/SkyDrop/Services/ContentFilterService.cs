namespace SkyDrop.Services;

/// <summary>
/// Service for managing content filter settings.
/// Controls visibility of adult/R18+ content.
/// </summary>
public class ContentFilterService
{
    private static ContentFilterService? _instance;
    public static ContentFilterService Instance => _instance ??= new ContentFilterService();

    private bool _showAdultContent;

    /// <summary>
    /// Gets whether adult (R18+) content should be shown.
    /// Default is false (hidden).
    /// </summary>
    public bool ShowAdultContent => _showAdultContent;

    public event Action? SettingChanged;

    private ContentFilterService()
    {
    }

    /// <summary>
    /// Toggles the adult content visibility setting.
    /// </summary>
    public void ToggleAdultContent()
    {
        _showAdultContent = !_showAdultContent;
        SettingChanged?.Invoke();
    }

    /// <summary>
    /// Sets whether to show adult content.
    /// </summary>
    public void SetShowAdultContent(bool show)
    {
        if (_showAdultContent == show) return;

        _showAdultContent = show;
        SettingChanged?.Invoke();
    }
}
