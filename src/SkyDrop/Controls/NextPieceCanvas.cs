using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using SkyDrop.Models;
using SkyDrop.Services;

namespace SkyDrop.Controls;

/// <summary>
/// Custom control for rendering the next piece preview.
/// </summary>
public class NextPieceCanvas : Control
{
    public static readonly StyledProperty<Tetromino?> PieceProperty =
        AvaloniaProperty.Register<NextPieceCanvas, Tetromino?>(nameof(Piece));

    public Tetromino? Piece
    {
        get => GetValue(PieceProperty);
        set => SetValue(PieceProperty, value);
    }

    static NextPieceCanvas()
    {
        AffectsRender<NextPieceCanvas>(PieceProperty);
    }

    public NextPieceCanvas()
    {
        // Subscribe to theme changes
        ThemeService.Instance.ThemeChanged += () => InvalidateVisual();
    }

    private Color GetThemeColor(string key, Color fallback)
    {
        if (Application.Current?.Resources.TryGetResource(key, ActualThemeVariant, out var resource) == true
            && resource is Color color)
        {
            return color;
        }
        return fallback;
    }

    public override void Render(DrawingContext context)
    {
        var bounds = Bounds;

        var backgroundColor = GetThemeColor("NextPieceBackgroundColor", Color.Parse("#2A2A2A"));

        var backgroundBrush = new SolidColorBrush(backgroundColor);
        context.DrawRectangle(backgroundBrush, null, bounds);

        if (Piece == null) return;

        var cellSize = Math.Min(bounds.Width / 4, bounds.Height / 4);
        var shape = Piece.GetCurrentShape();

        int minX = shape.Min(p => p.X);
        int maxX = shape.Max(p => p.X);
        int minY = shape.Min(p => p.Y);
        int maxY = shape.Max(p => p.Y);

        int pieceWidth = maxX - minX + 1;
        int pieceHeight = maxY - minY + 1;

        var offsetX = (bounds.Width - pieceWidth * cellSize) / 2 - minX * cellSize;
        var offsetY = (bounds.Height - pieceHeight * cellSize) / 2 - minY * cellSize;

        var color = GameConstants.PieceColors[Piece.Type];
        var fillBrush = new SolidColorBrush(color);
        var borderPen = new Pen(new SolidColorBrush(GameConstants.BlockBorderColor), 1);

        foreach (var block in shape)
        {
            var rect = new Rect(
                offsetX + block.X * cellSize + 1,
                offsetY + block.Y * cellSize + 1,
                cellSize - 2,
                cellSize - 2);

            context.DrawRectangle(fillBrush, borderPen, rect, 2, 2);

            var highlightColor = Color.FromArgb(60, 255, 255, 255);
            var highlightBrush = new SolidColorBrush(highlightColor);
            var highlightRect = new Rect(rect.X + 2, rect.Y + 2, rect.Width - 4, (rect.Height - 4) / 2);
            context.DrawRectangle(highlightBrush, null, highlightRect, 1, 1);
        }
    }
}
