using SkiaSharp;

namespace SkyDrop.Services;

/// <summary>
/// Generates stats images for sharing game results.
/// </summary>
public static class StatsImageGenerator
{
    private const int ImageWidth = 800;
    private const int ImageHeight = 420;

    // Colors matching the app theme
    private static readonly SKColor BackgroundColor = new(30, 30, 46);      // Dark background
    private static readonly SKColor PanelColor = new(45, 45, 65);           // Panel background
    private static readonly SKColor AccentCyan = new(0, 188, 212);          // Cyan accent
    private static readonly SKColor AccentGold = new(255, 193, 7);          // Gold accent
    private static readonly SKColor AccentGreen = new(76, 175, 80);         // Green accent
    private static readonly SKColor AccentRed = new(244, 67, 54);           // Red accent
    private static readonly SKColor TextPrimary = new(255, 255, 255);       // White text
    private static readonly SKColor TextSecondary = new(176, 176, 192);     // Gray text

    // Cached custom typeface
    private static SKTypeface? _customTypeface;
    private static SKTypeface CustomTypeface => _customTypeface ??= LoadCustomTypeface();

    private static SKTypeface LoadCustomTypeface()
    {
        try
        {
            var uri = new Uri("avares://SkyDrop/Assets/NuKinakoMochi-Reg.otf");
            using var stream = Avalonia.Platform.AssetLoader.Open(uri);
            return SKTypeface.FromStream(stream) ?? SKTypeface.Default;
        }
        catch
        {
            // Fallback to default if font can't be loaded
            return SKTypeface.Default;
        }
    }

    /// <summary>
    /// Generates a stats image with the game results.
    /// </summary>
    /// <param name="score">Final score</param>
    /// <param name="level">Final level</param>
    /// <param name="lines">Lines cleared</param>
    /// <param name="postsCount">Number of posts created</param>
    /// <returns>PNG image as byte array</returns>
    public static byte[] GenerateStatsImage(int score, int level, int lines, int postsCount)
    {
        using var surface = SKSurface.Create(new SKImageInfo(ImageWidth, ImageHeight));
        var canvas = surface.Canvas;

        // Draw background
        canvas.Clear(BackgroundColor);

        // Draw main panel
        using var panelPaint = new SKPaint
        {
            Color = PanelColor,
            IsAntialias = true
        };
        canvas.DrawRoundRect(new SKRoundRect(new SKRect(20, 20, ImageWidth - 20, ImageHeight - 20), 16, 16), panelPaint);

        // Create fonts using custom typeface
        var typeface = CustomTypeface;
        using var titleFont = new SKFont(typeface, 48);
        using var subtitleFont = new SKFont(typeface, 20);
        using var footerFont = new SKFont(typeface, 14);
        using var labelFont = new SKFont(typeface, 16);
        using var valueFont = new SKFont(typeface, 36);

        // Draw "SKYDROP" title
        using var titlePaint = new SKPaint { Color = AccentCyan, IsAntialias = true };
        canvas.DrawText("SKYDROP", ImageWidth / 2, 80, SKTextAlign.Center, titleFont, titlePaint);

        // Draw "GAME STATS" subtitle
        using var subtitlePaint = new SKPaint { Color = TextSecondary, IsAntialias = true };
        canvas.DrawText("GAME STATS", ImageWidth / 2, 110, SKTextAlign.Center, subtitleFont, subtitlePaint);

        // Draw separator line
        using var linePaint = new SKPaint
        {
            Color = AccentCyan,
            StrokeWidth = 2,
            IsAntialias = true
        };
        canvas.DrawLine(100, 130, ImageWidth - 100, 130, linePaint);

        // Stats layout - 2x2 grid
        DrawStatBox(canvas, 60, 160, "SCORE", score.ToString("N0"), AccentGold, labelFont, valueFont);
        DrawStatBox(canvas, ImageWidth / 2 + 20, 160, "LEVEL", level.ToString(), AccentCyan, labelFont, valueFont);
        DrawStatBox(canvas, 60, 280, "LINES", lines.ToString(), AccentGreen, labelFont, valueFont);
        DrawStatBox(canvas, ImageWidth / 2 + 20, 280, "POSTS", postsCount.ToString(), AccentRed, labelFont, valueFont);

        // Draw footer
        using var footerPaint = new SKPaint { Color = TextSecondary, IsAntialias = true };
        canvas.DrawText("Posted with SkyDrop", ImageWidth / 2, ImageHeight - 35, SKTextAlign.Center, footerFont, footerPaint);

        // Encode to PNG
        using var image = surface.Snapshot();
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        return data.ToArray();
    }

    private static void DrawStatBox(SKCanvas canvas, float x, float y, string label, string value, SKColor accentColor, SKFont labelFont, SKFont valueFont)
    {
        const float boxWidth = 340;
        const float boxHeight = 90;

        // Draw box background
        using var boxPaint = new SKPaint
        {
            Color = BackgroundColor,
            IsAntialias = true
        };
        canvas.DrawRoundRect(new SKRoundRect(new SKRect(x, y, x + boxWidth, y + boxHeight), 8, 8), boxPaint);

        // Draw accent border on left
        using var accentPaint = new SKPaint
        {
            Color = accentColor,
            IsAntialias = true
        };
        canvas.DrawRoundRect(new SKRoundRect(new SKRect(x, y, x + 6, y + boxHeight), 3, 3), accentPaint);

        // Draw label
        using var labelPaint = new SKPaint { Color = TextSecondary, IsAntialias = true };
        canvas.DrawText(label, x + 20, y + 30, SKTextAlign.Left, labelFont, labelPaint);

        // Draw value
        using var valuePaint = new SKPaint { Color = accentColor, IsAntialias = true };
        canvas.DrawText(value, x + 20, y + 70, SKTextAlign.Left, valueFont, valuePaint);
    }
}
