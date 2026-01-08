using System.ComponentModel;
using SkyDrop.Resources;

namespace SkyDrop.Services;

/// <summary>
/// Provides bindable access to localized strings with INotifyPropertyChanged support.
/// Bind to Instance.PropertyName to get strings that update when language changes.
/// </summary>
public class BindableStrings : INotifyPropertyChanged
{
    private static BindableStrings? _instance;
    public static BindableStrings Instance => _instance ??= new BindableStrings();

    public event PropertyChangedEventHandler? PropertyChanged;

    private BindableStrings()
    {
        LocalizationService.Instance.LanguageChanged += OnLanguageChanged;
    }

    private void OnLanguageChanged()
    {
        // Notify that all properties have changed (null/empty string means all)
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
    }

    public string SelectMode => Strings.SelectMode;
    public string Login => Strings.Login;
    public string Logout => Strings.Logout;
    public string Credits => Strings.Credits;
    public string ComingSoon => Strings.ComingSoon;
    public string LoginRequiredToPlay => Strings.LoginRequiredToPlay;
    public string NavTitleScreen => Strings.NavTitleScreen;
    public string ModeNormal => Strings.ModeNormal;
    public string ModeNormalDesc => Strings.ModeNormalDesc;
    public string ModeDiscoverFeed => Strings.ModeDiscoverFeed;
    public string ModeDiscoverFeedDesc => Strings.ModeDiscoverFeedDesc;
    public string ModeAuthorFeed => Strings.ModeAuthorFeed;
    public string ModeAuthorFeedDesc => Strings.ModeAuthorFeedDesc;
    public string ModeTimeline => Strings.ModeTimeline;
    public string ModeTimelineDesc => Strings.ModeTimelineDesc;
    public string ModeCreatePost => Strings.ModeCreatePost;
    public string ModeCreatePostDesc => Strings.ModeCreatePostDesc;
    public string ModeT9Generator => Strings.ModeT9Generator;
    public string ModeT9GeneratorDesc => Strings.ModeT9GeneratorDesc;
    public string RequiresLogin => Strings.RequiresLogin;
}
