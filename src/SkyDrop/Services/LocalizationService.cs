using System.Globalization;
using SkyDrop.Resources;

namespace SkyDrop.Services;

/// <summary>
/// Service for managing application language/localization.
/// Supports switching between English and Japanese.
/// </summary>
public class LocalizationService
{
    private static LocalizationService? _instance;
    public static LocalizationService Instance => _instance ??= new LocalizationService();

    private static readonly CultureInfo[] SupportedCultures =
    [
        new CultureInfo("en"), // English (neutral)
        new CultureInfo("ja")  // Japanese
    ];

    private int _currentCultureIndex;

    /// <summary>
    /// Fired when the language changes. Subscribe to this to refresh UI.
    /// </summary>
    public event Action? LanguageChanged;

    /// <summary>
    /// Gets the current culture.
    /// </summary>
    public CultureInfo CurrentCulture => SupportedCultures[_currentCultureIndex];

    /// <summary>
    /// Gets the current language code (e.g., "EN" or "JA").
    /// </summary>
    public string CurrentLanguageCode => CurrentCulture.TwoLetterISOLanguageName.ToUpperInvariant();

    /// <summary>
    /// Gets a value indicating whether the current language is Japanese.
    /// </summary>
    public bool IsJapanese => CurrentCulture.TwoLetterISOLanguageName == "ja";

    private LocalizationService()
    {
        // Start with system culture if supported, otherwise English
        var systemCulture = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
        _currentCultureIndex = systemCulture == "ja" ? 1 : 0;
        ApplyCulture();
    }

    /// <summary>
    /// Toggles to the next supported language.
    /// </summary>
    public void ToggleLanguage()
    {
        _currentCultureIndex = (_currentCultureIndex + 1) % SupportedCultures.Length;
        ApplyCulture();
        LanguageChanged?.Invoke();
    }

    /// <summary>
    /// Sets a specific language by culture name.
    /// </summary>
    public void SetLanguage(string cultureName)
    {
        for (int i = 0; i < SupportedCultures.Length; i++)
        {
            if (SupportedCultures[i].TwoLetterISOLanguageName.Equals(cultureName, StringComparison.OrdinalIgnoreCase))
            {
                if (_currentCultureIndex != i)
                {
                    _currentCultureIndex = i;
                    ApplyCulture();
                    LanguageChanged?.Invoke();
                }
                return;
            }
        }
    }

    private void ApplyCulture()
    {
        var culture = SupportedCultures[_currentCultureIndex];
        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;
        Strings.Culture = culture;
    }

    /// <summary>
    /// Gets a localized string by key.
    /// </summary>
    public string GetString(string key)
    {
        return Strings.ResourceManager.GetString(key, CurrentCulture) ?? key;
    }
}
