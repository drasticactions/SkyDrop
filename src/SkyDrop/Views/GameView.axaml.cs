using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using SkyDrop.Controls;
using SkyDrop.Models;
using SkyDrop.Services;
using SkyDrop.ViewModels;

namespace SkyDrop.Views;

public partial class GameView : UserControl
{
    private readonly IInputService _inputService;
    private Grid? _landscapeLayout;
    private Grid? _portraitLayout;
    private bool _isPortrait;
    private PauseOverlay? _pauseOverlay;
    private InstructionsOverlay? _instructionsOverlay;
    // Track if first input after game over has been consumed (to ignore held keys)
    private bool _gameOverInputReady;
    private bool _wasGameOver;

    public GameView()
    {
        InitializeComponent();
        _inputService = App.Services.GetRequiredService<IInputService>();
    }

    protected override void OnLoaded(global::Avalonia.Interactivity.RoutedEventArgs e)
    {
        base.OnLoaded(e);

        _landscapeLayout = this.FindControl<Grid>("LandscapeLayout");
        _portraitLayout = this.FindControl<Grid>("PortraitLayout");
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

        UpdateLayout(Bounds.Width, Bounds.Height);
    }

    private void OnResumeRequested(object? sender, EventArgs e)
    {
        if (DataContext is GameViewModel vm)
        {
            vm.HandleKeyDown(GameInput.Pause);
            Focus();
        }
    }

    private void OnMainMenuRequested(object? sender, EventArgs e)
    {
        if (DataContext is GameViewModel vm)
        {
            vm.ReturnToTitle();
        }
    }

    private void OnStartRequested(object? sender, EventArgs e)
    {
        if (DataContext is GameViewModel vm)
        {
            vm.DismissInstructionsAndStart();
            Focus();
        }
    }

    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);
        UpdateLayout(e.NewSize.Width, e.NewSize.Height);
    }

    private void UpdateLayout(double width, double height)
    {
        if (_landscapeLayout == null || _portraitLayout == null)
            return;

        // Switch to portrait when height > width (or close to it)
        // Using a threshold to account for the stats panel width
        bool shouldBePortrait = height > width * 1.2;

        if (shouldBePortrait != _isPortrait)
        {
            _isPortrait = shouldBePortrait;
            _landscapeLayout.IsVisible = !_isPortrait;
            _portraitLayout.IsVisible = _isPortrait;
        }
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _inputService.GameInputChanged += OnGameInputChanged;
        Focus();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _inputService.GameInputChanged -= OnGameInputChanged;
        base.OnDetachedFromVisualTree(e);
    }

    private void OnGameInputChanged(object? sender, GameInputEventArgs e)
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (DataContext is not GameViewModel vm) return;

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

                if (e.Input == GameInput.HardDrop) // Space
                {
                    vm.RestartGame();
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
}
