using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using SkyDrop.Services;

namespace SkyDrop.Controls;

/// <summary>
/// Dark mode background.
/// </summary>
public class StarFieldBackground : Control
{
    private readonly List<Star> _stars = new();
    private readonly Random _random = new();
    private DispatcherTimer? _animationTimer;
    private bool _isInitialized;
    private Size _lastSize;
    private bool _isAttached;
    private int _frameCount;

    private static readonly Color[] StarColors = new[]
    {
        Color.FromRgb(252, 252, 252),
        Color.FromRgb(188, 188, 188),
        Color.FromRgb(124, 124, 124),
        Color.FromRgb(168, 204, 252),
        Color.FromRgb(252, 252, 168),
    };

    private enum StarSize { Small, Medium, Large }

    private record Star(
        double X,
        double Y,
        StarSize Size,
        int ColorIndex,
        int TwinkleRate,
        int TwinkleOffset);

    public static readonly StyledProperty<int> StarCountProperty =
        AvaloniaProperty.Register<StarFieldBackground, int>(nameof(StarCount), defaultValue: 100);

    public int StarCount
    {
        get => GetValue(StarCountProperty);
        set => SetValue(StarCountProperty, value);
    }

    public static readonly StyledProperty<bool> EnableTwinkleProperty =
        AvaloniaProperty.Register<StarFieldBackground, bool>(nameof(EnableTwinkle), defaultValue: true);

    public bool EnableTwinkle
    {
        get => GetValue(EnableTwinkleProperty);
        set => SetValue(EnableTwinkleProperty, value);
    }

    static StarFieldBackground()
    {
        AffectsRender<StarFieldBackground>(StarCountProperty, EnableTwinkleProperty);
    }

    public StarFieldBackground()
    {
        IsHitTestVisible = false;
        ThemeService.Instance.ThemeChanged += OnThemeChanged;
    }

    private void OnThemeChanged()
    {
        UpdateAnimationState();
        InvalidateVisual();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _isAttached = true;
        UpdateAnimationState();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _isAttached = false;
        StopAnimation();
    }

    private void UpdateAnimationState()
    {
        if (ThemeService.Instance.IsDarkMode && EnableTwinkle && _isAttached)
        {
            StartAnimation();
        }
        else
        {
            StopAnimation();
        }
    }

    private void StartAnimation()
    {
        if (_animationTimer != null) return;

        _animationTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(200)
        };
        _animationTimer.Tick += (_, _) =>
        {
            _frameCount++;
            InvalidateVisual();
        };
        _animationTimer.Start();
    }

    private void StopAnimation()
    {
        _animationTimer?.Stop();
        _animationTimer = null;
    }

    private void GenerateStars(Size size)
    {
        _stars.Clear();

        const int pixelSize = 2;

        for (int i = 0; i < StarCount; i++)
        {
            double x = Math.Floor(_random.NextDouble() * size.Width / pixelSize) * pixelSize;
            double y = Math.Floor(_random.NextDouble() * size.Height / pixelSize) * pixelSize;

            double sizeCategory = _random.NextDouble();
            StarSize starSize;

            if (sizeCategory < 0.75)
            {
                starSize = StarSize.Small;
            }
            else if (sizeCategory < 0.95)
            {
                starSize = StarSize.Medium;
            }
            else
            {
                starSize = StarSize.Large;
            }

            int colorIndex = _random.Next(StarColors.Length);

            int twinkleRate = 15 + _random.Next(30); // 15-45 frames between state changes

            int twinkleOffset = _random.Next(100);

            _stars.Add(new Star(x, y, starSize, colorIndex, twinkleRate, twinkleOffset));
        }

        _isInitialized = true;
        _lastSize = size;
    }

    public override void Render(DrawingContext context)
    {
        var bounds = Bounds;
        if (bounds.Width <= 0 || bounds.Height <= 0) return;

        if (!ThemeService.Instance.IsDarkMode)
        {
            return;
        }

        if (!_isInitialized ||
            Math.Abs(_lastSize.Width - bounds.Width) > 10 ||
            Math.Abs(_lastSize.Height - bounds.Height) > 10)
        {
            GenerateStars(bounds.Size);
        }

        foreach (var star in _stars)
        {
            bool isVisible = true;
            if (EnableTwinkle)
            {
                int adjustedFrame = (_frameCount + star.TwinkleOffset) % (star.TwinkleRate * 2);
                isVisible = adjustedFrame < star.TwinkleRate;
            }

            if (isVisible)
            {
                DrawPixelStar(context, star);
            }
        }
    }

    private static void DrawPixelStar(DrawingContext context, Star star)
    {
        var color = StarColors[star.ColorIndex];
        var brush = new SolidColorBrush(color);
        const int pixelSize = 2;

        switch (star.Size)
        {
            case StarSize.Small:
                context.FillRectangle(brush, new Rect(star.X, star.Y, pixelSize, pixelSize));
                break;

            case StarSize.Medium:
                context.FillRectangle(brush, new Rect(star.X, star.Y, pixelSize * 2, pixelSize * 2));
                break;

            case StarSize.Large:
                var brightBrush = new SolidColorBrush(StarColors[0]);

                context.FillRectangle(brightBrush, new Rect(star.X, star.Y, pixelSize, pixelSize));

                context.FillRectangle(brush, new Rect(star.X, star.Y - pixelSize, pixelSize, pixelSize));
                context.FillRectangle(brush, new Rect(star.X, star.Y + pixelSize, pixelSize, pixelSize));
                context.FillRectangle(brush, new Rect(star.X - pixelSize, star.Y, pixelSize, pixelSize));
                context.FillRectangle(brush, new Rect(star.X + pixelSize, star.Y, pixelSize, pixelSize));
                break;
        }
    }
}
