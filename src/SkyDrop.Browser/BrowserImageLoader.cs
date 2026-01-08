using System;
using System.IO;
using System.Net.Http;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using AsyncImageLoader;
using Avalonia.Media.Imaging;

namespace SkyDrop.Browser;

/// <summary>
/// Custom image loader for the browser platform that uses a CORS proxy.
/// </summary>
[SupportedOSPlatform("browser")]
public class BrowserImageLoader : IAsyncImageLoader
{
    private const string CorsProxy = "https://corsproxy.io/?";

    private static readonly HttpClient HttpClient = new();

    public async Task<Bitmap?> ProvideImageAsync(string url)
    {
        try
        {
            var proxyUrl = CorsProxy + Uri.EscapeDataString(url);
            var imageData = await TryFetchImageAsync(proxyUrl);

            if (imageData == null || imageData.Length == 0)
                return null;

            using var stream = new MemoryStream(imageData);
            return new Bitmap(stream);
        }
        catch
        {
            return null;
        }
    }

    private static async Task<byte[]?> TryFetchImageAsync(string url)
    {
        try
        {
            var response = await HttpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsByteArrayAsync();
            }
        }
        catch
        {
            // Ignore - will try proxy
        }
        return null;
    }

    public void Dispose()
    {
        // HttpClient is static and shared, don't dispose
    }
}
