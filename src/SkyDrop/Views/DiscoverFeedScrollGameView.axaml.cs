using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using SkyDrop.Controls;
using SkyDrop.Models;
using SkyDrop.Services;
using SkyDrop.ViewModels;

namespace SkyDrop.Views;

public partial class DiscoverFeedScrollGameView : UserControl
{
    private readonly IInputService _inputService;
    private ScrollViewer? _feedScrollViewer;
    private PauseOverlay? _pauseOverlay;
    private InstructionsOverlay? _instructionsOverlay;
    private const double ScrollAmount = 100; // Pixels to scroll per rotation

    // Smooth scrolling fields
    private double _targetScrollOffset;
    private double _currentScrollOffset;
    private DispatcherTimer? _scrollAnimationTimer;
    private const double ScrollAnimationSpeed = 0.15; // Lerp factor (0-1), higher = faster

    // Track if first input after game over has been consumed (to ignore held keys)
    private bool _gameOverInputReady;
    private bool _wasGameOver;

    public DiscoverFeedScrollGameView()
    {
        InitializeComponent();
        _inputService = App.Services.GetRequiredService<IInputService>();
    }

    protected override void OnLoaded(global::Avalonia.Interactivity.RoutedEventArgs e)
    {
        base.OnLoaded(e);

        _feedScrollViewer = this.FindControl<ScrollViewer>("FeedScrollViewer");
        _pauseOverlay = this.FindControl<PauseOverlay>("PauseOverlay");
        _instructionsOverlay = this.FindControl<InstructionsOverlay>("InstructionsOverlay");

        if (_pauseOverlay != null)
        {
            _pauseOverlay.ResumeRequested += OnResumeRequested;
            _pauseOverlay.MainMenuRequested += OnMainMenuRequested;
        }

        if (_instructionsOverlay != null)
        {
            _instructionsOverlay.StartRequested += OnStartRequested;
        }

        if (DataContext is DiscoverFeedScrollGameViewModel vm)
        {
            vm.RequestScroll += OnRequestScroll;

            if (_feedScrollViewer != null)
            {
                _feedScrollViewer.ScrollChanged += OnScrollChanged;
                _currentScrollOffset = _feedScrollViewer.Offset.Y;
                _targetScrollOffset = _currentScrollOffset;
            }
        }

        // Initialize scroll animation timer
        _scrollAnimationTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(16) // ~60 FPS
        };
        _scrollAnimationTimer.Tick += OnScrollAnimationTick;
    }

    private void OnResumeRequested(object? sender, EventArgs e)
    {
        if (DataContext is DiscoverFeedScrollGameViewModel vm)
        {
            vm.HandleKeyDown(GameInput.Pause);
            Focus();
        }
    }

    private void OnMainMenuRequested(object? sender, EventArgs e)
    {
        if (DataContext is DiscoverFeedScrollGameViewModel vm)
        {
            vm.ReturnToTitle();
        }
    }

    private void OnStartRequested(object? sender, EventArgs e)
    {
        if (DataContext is DiscoverFeedScrollGameViewModel vm)
        {
            vm.DismissInstructionsAndStart();
            Focus();
        }
    }

    protected override void OnUnloaded(global::Avalonia.Interactivity.RoutedEventArgs e)
    {
        base.OnUnloaded(e);

        if (DataContext is DiscoverFeedScrollGameViewModel vm)
        {
            vm.RequestScroll -= OnRequestScroll;
        }

        if (_feedScrollViewer != null)
        {
            _feedScrollViewer.ScrollChanged -= OnScrollChanged;
        }

        if (_scrollAnimationTimer != null)
        {
            _scrollAnimationTimer.Stop();
            _scrollAnimationTimer.Tick -= OnScrollAnimationTick;
            _scrollAnimationTimer = null;
        }
    }

    private void OnRequestScroll(bool scrollDown)
    {
        if (_feedScrollViewer == null) return;

        // Update target offset - allows accumulating multiple rotations
        var maxScroll = Math.Max(0, _feedScrollViewer.Extent.Height - _feedScrollViewer.Viewport.Height);

        if (scrollDown)
        {
            _targetScrollOffset = Math.Min(_targetScrollOffset + ScrollAmount, maxScroll);
        }
        else
        {
            _targetScrollOffset = Math.Max(0, _targetScrollOffset - ScrollAmount);
        }

        // Start animation if not already running
        if (_scrollAnimationTimer != null && !_scrollAnimationTimer.IsEnabled)
        {
            _scrollAnimationTimer.Start();
        }
    }

    private void OnScrollAnimationTick(object? sender, EventArgs e)
    {
        if (_feedScrollViewer == null)
        {
            _scrollAnimationTimer?.Stop();
            return;
        }

        // Calculate distance to target
        var distance = _targetScrollOffset - _currentScrollOffset;

        // If close enough, snap to target and stop animation
        if (Math.Abs(distance) < 0.5)
        {
            _currentScrollOffset = _targetScrollOffset;
            _feedScrollViewer.Offset = new Vector(_feedScrollViewer.Offset.X, _currentScrollOffset);
            _scrollAnimationTimer?.Stop();
            return;
        }

        // Smooth interpolation (lerp) towards target
        _currentScrollOffset += distance * ScrollAnimationSpeed;
        _feedScrollViewer.Offset = new Vector(_feedScrollViewer.Offset.X, _currentScrollOffset);
    }

    private void OnScrollChanged(object? sender, ScrollChangedEventArgs e)
    {
        if (_feedScrollViewer == null || DataContext is not DiscoverFeedScrollGameViewModel vm)
            return;

        vm.OnScrollPositionChanged(
            _feedScrollViewer.Offset.Y,
            _feedScrollViewer.Viewport.Height,
            _feedScrollViewer.Extent.Height);
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _inputService.GameInputChanged += OnGameInputChanged;
        _inputService.UIInputReceived += OnUIInputReceived;
        Focus();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _inputService.GameInputChanged -= OnGameInputChanged;
        _inputService.UIInputReceived -= OnUIInputReceived;
        base.OnDetachedFromVisualTree(e);
    }

    private void OnGameInputChanged(object? sender, GameInputEventArgs e)
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (DataContext is not DiscoverFeedScrollGameViewModel vm) return;

            // Track game over state transitions - disable input until first key release
            if (vm.IsGameOver && !_wasGameOver)
            {
                _gameOverInputReady = false;
            }
            _wasGameOver = vm.IsGameOver;

            // Handle game over inputs
            if (vm.IsGameOver)
            {
                // On key up, enable game over input
                if (!e.IsPressed && !_gameOverInputReady)
                {
                    _gameOverInputReady = true;
                    return;
                }

                // Ignore all inputs until a key has been released after game over
                if (!_gameOverInputReady || !e.IsPressed)
                {
                    return;
                }

                if (e.Input == GameInput.Pause) // Escape
                {
                    vm.ReturnToTitle();
                    return;
                }

                return;
            }

            // Normal gameplay input
            vm.HandleGameInput(e);
        });
    }

    private void OnUIInputReceived(object? sender, UIInputEventArgs e)
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (DataContext is not DiscoverFeedScrollGameViewModel vm) return;

            // Handle game over restart with Confirm (Enter/Space)
            if (vm.IsGameOver && _gameOverInputReady && e.Input == UIInput.Confirm)
            {
                _ = vm.RestartGameAsync();
            }
        });
    }
}
