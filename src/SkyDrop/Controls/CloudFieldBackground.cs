using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using SkyDrop.Services;

namespace SkyDrop.Controls;

/// <summary>
/// Light mode background.
/// </summary>
public class CloudFieldBackground : Control
{
    private readonly List<Cloud> _clouds = new();
    private readonly Random _random = new();
    private DispatcherTimer? _animationTimer;
    private bool _isInitialized;
    private Size _lastSize;
    private bool _isAttached;

    private static readonly Color CloudWhite = Color.FromRgb(252, 252, 252);
    private static readonly Color CloudLight = Color.FromRgb(188, 228, 252);
    private static readonly Color CloudShadow = Color.FromRgb(172, 172, 172);

    private const int PixelSize = 4;

    private enum CloudSize { Small, Medium, Large }

    private class Cloud
    {
        public double X { get; set; }
        public double Y { get; set; }
        public CloudSize Size { get; set; }
        public int SpriteVariant { get; set; }
        public double Speed { get; set; }
    }

    public static readonly StyledProperty<int> CloudCountProperty =
        AvaloniaProperty.Register<CloudFieldBackground, int>(nameof(CloudCount), defaultValue: 8);

    public int CloudCount
    {
        get => GetValue(CloudCountProperty);
        set => SetValue(CloudCountProperty, value);
    }

    public static readonly StyledProperty<bool> EnableAnimationProperty =
        AvaloniaProperty.Register<CloudFieldBackground, bool>(nameof(EnableAnimation), defaultValue: true);

    public bool EnableAnimation
    {
        get => GetValue(EnableAnimationProperty);
        set => SetValue(EnableAnimationProperty, value);
    }

    static CloudFieldBackground()
    {
        AffectsRender<CloudFieldBackground>(CloudCountProperty, EnableAnimationProperty);
    }

    public CloudFieldBackground()
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
        if (!ThemeService.Instance.IsDarkMode && EnableAnimation && _isAttached)
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
            Interval = TimeSpan.FromMilliseconds(50)
        };
        _animationTimer.Tick += OnAnimationTick;
        _animationTimer.Start();
    }

    private void OnAnimationTick(object? sender, EventArgs e)
    {
        foreach (var cloud in _clouds)
        {
            cloud.X += cloud.Speed;

            int cloudWidth = GetCloudWidth(cloud.Size) * PixelSize;

            if (cloud.X > _lastSize.Width + cloudWidth)
            {
                cloud.X = -cloudWidth * 2;
                cloud.Y = Math.Floor(_random.NextDouble() * (_lastSize.Height * 0.5) / PixelSize) * PixelSize;
            }
        }

        InvalidateVisual();
    }

    private static int GetCloudWidth(CloudSize size) => size switch
    {
        CloudSize.Small => 8,
        CloudSize.Medium => 12,
        CloudSize.Large => 16,
        _ => 8
    };

    private void StopAnimation()
    {
        if (_animationTimer != null)
        {
            _animationTimer.Tick -= OnAnimationTick;
            _animationTimer.Stop();
            _animationTimer = null;
        }
    }

    private void GenerateClouds(Size size)
    {
        _clouds.Clear();

        for (int i = 0; i < CloudCount; i++)
        {
            var cloud = GenerateCloud(size, i);
            _clouds.Add(cloud);
        }

        _isInitialized = true;
        _lastSize = size;
    }

    private Cloud GenerateCloud(Size size, int index)
    {
        double startX = Math.Floor(((size.Width / CloudCount) * index + _random.NextDouble() * 100 - 50) / PixelSize) * PixelSize;

        double y = Math.Floor(_random.NextDouble() * (size.Height * 0.5) / PixelSize) * PixelSize;

        double sizeCategory = _random.NextDouble();
        CloudSize cloudSize;

        if (sizeCategory < 0.4)
        {
            cloudSize = CloudSize.Small;
        }
        else if (sizeCategory < 0.8)
        {
            cloudSize = CloudSize.Medium;
        }
        else
        {
            cloudSize = CloudSize.Large;
        }

        int spriteVariant = _random.Next(3);

        double speed = 0.3 + _random.NextDouble() * 0.4; // 0.3-0.7 pixels per tick for gentle drift

        return new Cloud
        {
            X = startX,
            Y = y,
            Size = cloudSize,
            SpriteVariant = spriteVariant,
            Speed = speed
        };
    }

    public override void Render(DrawingContext context)
    {
        var bounds = Bounds;
        if (bounds.Width <= 0 || bounds.Height <= 0) return;

        if (ThemeService.Instance.IsDarkMode)
        {
            return;
        }

        if (!_isInitialized ||
            Math.Abs(_lastSize.Width - bounds.Width) > 10 ||
            Math.Abs(_lastSize.Height - bounds.Height) > 10)
        {
            GenerateClouds(bounds.Size);
        }

        foreach (var cloud in _clouds)
        {
            DrawPixelCloud(context, cloud);
        }
    }

    private static void DrawPixelCloud(DrawingContext context, Cloud cloud)
    {
        var whiteBrush = new SolidColorBrush(CloudWhite);
        var shadowBrush = new SolidColorBrush(CloudShadow);
        var highlightBrush = new SolidColorBrush(CloudLight);

        var pattern = GetCloudPattern(cloud.Size, cloud.SpriteVariant);

        for (int row = 0; row < pattern.Length; row++)
        {
            for (int col = 0; col < pattern[row].Length; col++)
            {
                char pixel = pattern[row][col];
                if (pixel == ' ') continue;

                double x = cloud.X + col * PixelSize;
                double y = cloud.Y + row * PixelSize;

                IBrush brush = pixel switch
                {
                    'W' => whiteBrush,
                    'H' => highlightBrush,
                    'S' => shadowBrush,
                    _ => whiteBrush
                };

                context.FillRectangle(brush, new Rect(x, y, PixelSize, PixelSize));
            }
        }
    }

    /// <summary>
    /// Returns a pixel pattern for the cloud sprite.
    /// W = white (main body), H = highlight, S = shadow, space = transparent
    /// </summary>
    private static string[] GetCloudPattern(CloudSize size, int variant)
    {
        return size switch
        {
            CloudSize.Small => GetSmallCloudPattern(variant),
            CloudSize.Medium => GetMediumCloudPattern(variant),
            CloudSize.Large => GetLargeCloudPattern(variant),
            _ => GetSmallCloudPattern(0)
        };
    }

    private static string[] GetSmallCloudPattern(int variant)
    {
        return variant switch
        {
            0 => new[]
            {
                "  HH  ",
                " HWWH ",
                "HWWWWS",
                "WWWWWS",
                " SSSS "
            },
            1 => new[]
            {
                "   HH ",
                " HHWWH",
                "HWWWWS",
                "WWWWSS",
                " SSS  "
            },
            _ => new[]
            {
                " HHH  ",
                "HWWWH ",
                "WWWWWH",
                "WWWWSS",
                " SSSS "
            }
        };
    }

    private static string[] GetMediumCloudPattern(int variant)
    {
        return variant switch
        {
            0 => new[]
            {
                "   HHH    ",
                "  HWWWH   ",
                " HWWWWWHH ",
                "HWWWWWWWWH",
                "WWWWWWWWWS",
                "WWWWWWWWSS",
                " SSSSSSS  "
            },
            1 => new[]
            {
                "    HH    ",
                "  HHWWHH  ",
                " HWWWWWWH ",
                "HWWWWWWWWH",
                "WWWWWWWWWS",
                " WWWWWWSS ",
                "  SSSSS   "
            },
            _ => new[]
            {
                "  HHH     ",
                " HWWWHH   ",
                "HWWWWWWHH ",
                "WWWWWWWWWH",
                "WWWWWWWWWS",
                " WWWWWWSS ",
                "  SSSSS   "
            }
        };
    }

    private static string[] GetLargeCloudPattern(int variant)
    {
        return variant switch
        {
            0 => new[]
            {
                "     HHHH       ",
                "   HHWWWWHH     ",
                "  HWWWWWWWWH HH ",
                " HWWWWWWWWWHHWWH",
                "HWWWWWWWWWWWWWWS",
                "WWWWWWWWWWWWWWWS",
                "WWWWWWWWWWWWWWSS",
                " SSSSSSSSSSSSS  "
            },
            1 => new[]
            {
                "      HHH       ",
                "    HHWWWHH     ",
                "  HHWWWWWWWH    ",
                " HWWWWWWWWWWHH  ",
                "HWWWWWWWWWWWWWH ",
                "WWWWWWWWWWWWWWWS",
                " WWWWWWWWWWWWSS ",
                "  SSSSSSSSSS    "
            },
            _ => new[]
            {
                "    HHH    HH   ",
                "   HWWWH HHWWH  ",
                "  HWWWWWHHWWWWH ",
                " HWWWWWWWWWWWWH ",
                "HWWWWWWWWWWWWWWS",
                "WWWWWWWWWWWWWWSS",
                " WWWWWWWWWWWSS  ",
                "  SSSSSSSSSS    "
            }
        };
    }
}
