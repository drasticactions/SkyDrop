using AsyncImageLoader;
using Avalonia;
using Avalonia.Browser;
using Avalonia.Media;
using SkyDrop;
using SkyDrop.Browser;
using System.Runtime.InteropServices.JavaScript;
using System.Runtime.Versioning;
using System.Threading.Tasks;

[assembly: SupportedOSPlatform("browser")]
internal sealed partial class Program
{
    private static async Task Main(string[] args)
    {
        await WebAssemblyCultureProviderInterop.LoadSatelliteAssemblies(["ja"]);

        // Register custom image loader that handles CORS via JS interop
        ImageLoader.AsyncImageLoader = new BrowserImageLoader();

        await BuildAvaloniaApp()
            .WithInterFont()
            .With(
                  new FontManagerOptions
                  {
                      DefaultFamilyName = "avares://SkyDrop.Browser/Assets#Noto Sans CJK JP",
                      FontFallbacks = new[]
                        {
                                new FontFallback
                                {
                                        FontFamily = new FontFamily("avares://SkyDrop.Browser/Assets#Noto Sans CJK JP"),
                                },
                                new FontFallback
                                {
                                        FontFamily = new FontFamily("avares://SkyDrop.Browser/Assets#Noto Sans"),
                                },
                                new FontFallback
                                {
                                        FontFamily = new FontFamily("avares://SkyDrop.Browser/Assets#Noto Mono"),
                                },
                                new FontFallback
                                {
                                        FontFamily = new FontFamily("avares://SkyDrop.Browser/Assets#OpenMoji"),
                                        UnicodeRange = UnicodeRange.Parse("U+23??, U+26??, U+2700-27BF, U+2B??, U+1F1E6-1F1FF, U+1F300-1F5FF, U+1F600-1F64F, U+1F680-1F6FF, U+1F9??")
                                }
                        },
                  })
            .StartBrowserAppAsync("out");
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>();
}

internal partial class WebAssemblyCultureProviderInterop
{
    [JSImport("INTERNAL.loadSatelliteAssemblies")]
    public static partial Task LoadSatelliteAssemblies(string[] culturesToLoad);
}