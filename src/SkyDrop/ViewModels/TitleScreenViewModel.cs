using Avalonia.Data.Converters;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SkyDrop.Events;
using SkyDrop.Models;
using SkyDrop.Resources;
using SkyDrop.Services;

namespace SkyDrop.ViewModels;

/// <summary>
/// ViewModel for the title screen with game mode selection.
/// </summary>
public partial class TitleScreenViewModel : ViewModelBase
{
    public static readonly IMultiValueConverter IsLockedConverter =
        new FuncMultiValueConverter<object, bool>(values =>
        {
            var list = values.ToList();
            if (list.Count >= 2 && list[0] is bool requiresLogin && list[1] is bool isAuthenticated)
            {
                return requiresLogin && !isAuthenticated;
            }
            return false;
        });

    public static readonly IMultiValueConverter CanPlayModeConverter =
        new FuncMultiValueConverter<object, bool>(values =>
        {
            var list = values.ToList();
            if (list.Count >= 3 && list[0] is bool isAvailable && list[1] is bool requiresLogin && list[2] is bool isAuthenticated)
            {
                var isLocked = requiresLogin && !isAuthenticated;
                return isAvailable && !isLocked;
            }
            return false;
        });

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ThemeDisplayText))]
    private bool _isDarkMode;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ThemeDisplayText))]
    private bool _isAutoTheme = true;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FontDisplayText))]
    private bool _useCustomFont = true;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ContentFilterDisplayText))]
    private bool _showAdultContent;

    [ObservableProperty]
    private int _selectedModeIndex = 0;

    [ObservableProperty]
    private TitleScreenSection _currentSection = TitleScreenSection.ModeList;

    [ObservableProperty]
    private TopBarButton _selectedTopBarButton = TopBarButton.Login;

    [ObservableProperty]
    private bool _isAuthenticated;

    public string ThemeDisplayText => IsAutoTheme ? Strings.ThemeAuto : (IsDarkMode ? Strings.ThemeDark : Strings.ThemeLight);

    public string FontDisplayText => UseCustomFont ? Strings.FontCustom : Strings.FontDefault;

    public string ContentFilterDisplayText => ShowAdultContent ? Strings.ContentFilterOn : Strings.ContentFilterOff;

    [ObservableProperty]
    private string _languageDisplayText = LocalizationService.Instance.CurrentLanguageCode;

    /// <summary>
    /// Available game modes.
    /// </summary>
    public List<GameModeInfo> AvailableModes { get; } =
    [
        new GameModeInfo(GameMode.Normal, isAvailable: true),
        new GameModeInfo(GameMode.DiscoverFeed, isAvailable: true),
        new GameModeInfo(GameMode.AuthorFeed, isAvailable: true),
        new GameModeInfo(GameMode.Timeline, isAvailable: true, requiresLogin: true),
        new GameModeInfo(GameMode.CreatePost, isAvailable: true, requiresLogin: true),
        new GameModeInfo(GameMode.T9Generator, isAvailable: true)
    ];

    /// <summary>
    /// Fired when a mode is selected.
    /// </summary>
    public event Action<GameModeInfo>? ModeSelected;

    /// <summary>
    /// Fired when login is requested.
    /// </summary>
    public event Action? LoginRequested;

    /// <summary>
    /// Fired when credits screen is requested.
    /// </summary>
    public event Action? CreditsRequested;

    public TitleScreenViewModel()
    {
        UpdateFromThemeService();
        UpdateFromFontService();
        UpdateFromContentFilterService();
        ThemeService.Instance.ThemeChanged += OnThemeChanged;
        FontService.Instance.FontChanged += OnFontChanged;
        ContentFilterService.Instance.SettingChanged += OnContentFilterChanged;
        LocalizationService.Instance.LanguageChanged += OnLanguageChanged;

        StrongReferenceMessenger.Default.Register<OnLoginUserEventArgs>(this, OnLoginUserChanged);
    }

    private void OnThemeChanged()
    {
        UpdateFromThemeService();
    }

    private void UpdateFromThemeService()
    {
        IsDarkMode = ThemeService.Instance.IsDarkMode;
        IsAutoTheme = !ThemeService.Instance.UserHasOverridden;
    }

    private void OnFontChanged()
    {
        UpdateFromFontService();
    }

    private void UpdateFromFontService()
    {
        UseCustomFont = FontService.Instance.UseCustomFont;
    }

    private void OnContentFilterChanged()
    {
        UpdateFromContentFilterService();
    }

    private void UpdateFromContentFilterService()
    {
        ShowAdultContent = ContentFilterService.Instance.ShowAdultContent;
    }

    private void OnLoginUserChanged(object recipient, OnLoginUserEventArgs args)
    {
        IsAuthenticated = args.LoginUser is not null;
    }

    private void OnLanguageChanged()
    {
        LanguageDisplayText = LocalizationService.Instance.CurrentLanguageCode;
        OnPropertyChanged(nameof(ThemeDisplayText));
        OnPropertyChanged(nameof(FontDisplayText));
        OnPropertyChanged(nameof(ContentFilterDisplayText));
    }

    /// <summary>
    /// Checks if a mode is locked (requires login but user is not authenticated).
    /// </summary>
    public bool IsModeLockedForLogin(GameModeInfo mode) => mode.RequiresLogin && !IsAuthenticated;

    [RelayCommand]
    private void SelectMode(GameModeInfo mode)
    {
        if (mode.IsAvailable && !IsModeLockedForLogin(mode))
        {
            ModeSelected?.Invoke(mode);
        }
    }

    [RelayCommand]
    private void ToggleTheme()
    {
        ThemeService.Instance.ToggleTheme();
    }

    [RelayCommand]
    private void ToggleFont()
    {
        FontService.Instance.ToggleCustomFont();
    }

    [RelayCommand]
    private void ToggleContentFilter()
    {
        ContentFilterService.Instance.ToggleAdultContent();
    }

    [RelayCommand]
    private void ResetToAutoTheme()
    {
        ThemeService.Instance.ResetToSystemTheme();
    }

    [RelayCommand]
    private void ToggleLanguage()
    {
        LocalizationService.Instance.ToggleLanguage();
    }

    [RelayCommand]
    private void OpenLogin()
    {
        LoginRequested?.Invoke();
    }

    [RelayCommand]
    private void OpenCredits()
    {
        CreditsRequested?.Invoke();
    }

    [RelayCommand]
    private void MoveUp()
    {
        if (CurrentSection == TitleScreenSection.ModeList)
        {
            if (SelectedModeIndex > 0)
            {
                SelectedModeIndex--;
            }
            else
            {
                CurrentSection = TitleScreenSection.TopBar;
            }
        }
    }

    [RelayCommand]
    private void MoveDown()
    {
        if (CurrentSection == TitleScreenSection.TopBar)
        {
            CurrentSection = TitleScreenSection.ModeList;
        }
        else if (SelectedModeIndex < AvailableModes.Count - 1)
        {
            SelectedModeIndex++;
        }
    }

    [RelayCommand]
    private void MoveLeft()
    {
        if (CurrentSection == TitleScreenSection.TopBar)
        {
            SelectedTopBarButton = SelectedTopBarButton switch
            {
                TopBarButton.Theme => TopBarButton.ContentFilter,
                TopBarButton.ContentFilter => TopBarButton.Font,
                TopBarButton.Font => TopBarButton.Language,
                TopBarButton.Language => TopBarButton.Credits,
                TopBarButton.Credits => TopBarButton.Login,
                _ => SelectedTopBarButton
            };
        }
    }

    [RelayCommand]
    private void MoveRight()
    {
        if (CurrentSection == TitleScreenSection.TopBar)
        {
            SelectedTopBarButton = SelectedTopBarButton switch
            {
                TopBarButton.Login => TopBarButton.Credits,
                TopBarButton.Credits => TopBarButton.Language,
                TopBarButton.Language => TopBarButton.Font,
                TopBarButton.Font => TopBarButton.ContentFilter,
                TopBarButton.ContentFilter => TopBarButton.Theme,
                _ => SelectedTopBarButton
            };
        }
    }

    [RelayCommand]
    private void ActivateSelection()
    {
        if (CurrentSection == TitleScreenSection.TopBar)
        {
            switch (SelectedTopBarButton)
            {
                case TopBarButton.Login:
                    LoginRequested?.Invoke();
                    break;
                case TopBarButton.Credits:
                    CreditsRequested?.Invoke();
                    break;
                case TopBarButton.Language:
                    LocalizationService.Instance.ToggleLanguage();
                    break;
                case TopBarButton.Font:
                    FontService.Instance.ToggleCustomFont();
                    break;
                case TopBarButton.ContentFilter:
                    ContentFilterService.Instance.ToggleAdultContent();
                    break;
                case TopBarButton.Theme:
                    ThemeService.Instance.ToggleTheme();
                    break;
            }
        }
        else
        {
            var mode = AvailableModes[SelectedModeIndex];
            if (mode.IsAvailable && !IsModeLockedForLogin(mode))
            {
                ModeSelected?.Invoke(mode);
            }
        }
    }
}
