using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using SkyDrop.Models;
using SkyDrop.Services;

namespace SkyDrop.Controls;

public class GameCanvas : Control
{
    private GameEngine? _engine;
    private double _cellSize;
    private double _offsetX;
    private double _offsetY;

    public static readonly StyledProperty<GameEngine?> EngineProperty =
        AvaloniaProperty.Register<GameCanvas, GameEngine?>(nameof(Engine));

    public GameEngine? Engine
    {
        get => GetValue(EngineProperty);
        set => SetValue(EngineProperty, value);
    }

    public static readonly StyledProperty<bool> ShowGhostProperty =
        AvaloniaProperty.Register<GameCanvas, bool>(nameof(ShowGhost), defaultValue: true);

    public bool ShowGhost
    {
        get => GetValue(ShowGhostProperty);
        set => SetValue(ShowGhostProperty, value);
    }

    public static readonly StyledProperty<bool> ShowGridProperty =
        AvaloniaProperty.Register<GameCanvas, bool>(nameof(ShowGrid), defaultValue: true);

    public bool ShowGrid
    {
        get => GetValue(ShowGridProperty);
        set => SetValue(ShowGridProperty, value);
    }

    static GameCanvas()
    {
        AffectsRender<GameCanvas>(EngineProperty, ShowGhostProperty, ShowGridProperty);
    }

    public GameCanvas()
    {
        ThemeService.Instance.ThemeChanged += () => InvalidateVisual();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == EngineProperty)
        {
            var oldEngine = change.GetOldValue<GameEngine?>();
            var newEngine = change.GetNewValue<GameEngine?>();

            if (oldEngine != null)
            {
                oldEngine.OnBoardChanged -= InvalidateVisual;
            }

            if (newEngine != null)
            {
                newEngine.OnBoardChanged += InvalidateVisual;
            }

            _engine = newEngine;
            InvalidateVisual();
        }
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

        var backgroundColor = GetThemeColor("BoardBackgroundColor", Color.Parse("#1A1A1A"));
        var gridColor = GetThemeColor("GridLineColor", Color.Parse("#333333"));

        var boardWidth = GameConstants.BoardWidth;
        var boardHeight = GameConstants.BoardHeight;

        _cellSize = Math.Min(bounds.Width / boardWidth, bounds.Height / boardHeight);
        _offsetX = (bounds.Width - (boardWidth * _cellSize)) / 2;
        _offsetY = (bounds.Height - (boardHeight * _cellSize)) / 2;

        var backgroundBrush = new SolidColorBrush(backgroundColor);
        context.DrawRectangle(backgroundBrush, null,
            new Rect(_offsetX, _offsetY, boardWidth * _cellSize, boardHeight * _cellSize));

        if (ShowGrid)
        {
            DrawGrid(context, gridColor);
        }

        if (_engine == null) return;

        foreach (var (pos, type) in _engine.Board.GetFilledCells())
        {
            DrawBlock(context, pos.X, pos.Y, type, 1.0);
        }

        if (ShowGhost && _engine.CurrentPiece != null && !_engine.IsGameOver)
        {
            var ghostY = _engine.GetGhostY();
            var piece = _engine.CurrentPiece;
            var shape = piece.GetCurrentShape();

            foreach (var block in shape)
            {
                int x = piece.Position.X + block.X;
                int y = ghostY + block.Y;
                if (y >= 0)
                {
                    DrawBlock(context, x, y, piece.Type, GameConstants.GhostOpacity);
                }
            }
        }

        if (_engine.CurrentPiece != null && !_engine.IsGameOver)
        {
            foreach (var pos in _engine.CurrentPiece.GetBlockPositions())
            {
                if (pos.Y >= 0)
                {
                    DrawBlock(context, pos.X, pos.Y, _engine.CurrentPiece.Type, 1.0);
                }
            }
        }
    }

    private void DrawGrid(DrawingContext context, Color gridColor)
    {
        var gridPen = new Pen(new SolidColorBrush(gridColor), 1);

        for (int x = 0; x <= GameConstants.BoardWidth; x++)
        {
            context.DrawLine(gridPen,
                new Point(_offsetX + x * _cellSize, _offsetY),
                new Point(_offsetX + x * _cellSize, _offsetY + GameConstants.BoardHeight * _cellSize));
        }

        for (int y = 0; y <= GameConstants.BoardHeight; y++)
        {
            context.DrawLine(gridPen,
                new Point(_offsetX, _offsetY + y * _cellSize),
                new Point(_offsetX + GameConstants.BoardWidth * _cellSize, _offsetY + y * _cellSize));
        }
    }

    private void DrawBlock(DrawingContext context, int x, int y, TetrominoType type, double opacity)
    {
        var color = GameConstants.PieceColors[type];
        var fillColor = Color.FromArgb((byte)(opacity * 255), color.R, color.G, color.B);

        var rect = new Rect(
            _offsetX + x * _cellSize + 1,
            _offsetY + y * _cellSize + 1,
            _cellSize - 2,
            _cellSize - 2);

        var fillBrush = new SolidColorBrush(fillColor);
        var borderPen = new Pen(new SolidColorBrush(GameConstants.BlockBorderColor), 1);

        context.DrawRectangle(fillBrush, borderPen, rect, 2, 2);

        if (opacity > 0.5)
        {
            var highlightColor = Color.FromArgb(60, 255, 255, 255);
            var highlightBrush = new SolidColorBrush(highlightColor);
            var highlightRect = new Rect(rect.X + 2, rect.Y + 2, rect.Width - 4, (rect.Height - 4) / 2);
            context.DrawRectangle(highlightBrush, null, highlightRect, 1, 1);
        }
    }
}
