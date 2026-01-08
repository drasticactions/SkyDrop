using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using FishyFlip;
using Microsoft.Extensions.DependencyInjection;
using SkyDrop.Services;
using SkyDrop.ViewModels;
using SkyDrop.Views;
using System.Linq;

namespace SkyDrop;

public partial class App : Application
{
    /// <summary>
    /// Gets the service provider for dependency injection.
    /// </summary>
    public static IServiceProvider Services { get; private set; } = null!;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var services = new ServiceCollection();
        ConfigureServices(services);
        Services = services.BuildServiceProvider();

        ThemeService.Instance.Initialize();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
            };
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView = new MainView
            {
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IInputService, KeyboardInputService>();

        services.AddSingleton<ATProtocol>(_ => new ATProtocolBuilder().EnableBlueskyModerationService().Build());

        services.AddSingleton<JapaneseDictionaryService>(sp =>
        {
            var service = new JapaneseDictionaryService();
            Task.Run(() => service.LoadFromAvaloniaResource("avares://SkyDrop/Assets/jmdict-eng-common-3.6.1.json"));
            return service;
        });

        services.AddSingleton<MainWindowViewModel>();
        services.AddSingleton<LoginViewModel>();
        services.AddSingleton<DiscoverFeedGameViewModel>();
        services.AddSingleton<DiscoverFeedScrollGameViewModel>();
        services.AddSingleton<RevealFeedGameViewModel>();
        services.AddSingleton<ScrollFeedGameViewModel>();
        services.AddSingleton<CreatePostGameViewModel>();
        services.AddSingleton<DiscoverFeedModeOptionsViewModel>();
        services.AddSingleton<AuthorFeedModeOptionsViewModel>();
        services.AddSingleton<TimelineModeOptionsViewModel>();
        services.AddTransient<FeedSelectorViewModel>();
        services.AddSingleton<T9GeneratorViewModel>();
    }
}