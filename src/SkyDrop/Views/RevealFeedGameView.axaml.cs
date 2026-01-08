using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using SkyDrop.Controls;
using SkyDrop.Models;
using SkyDrop.Services;
using SkyDrop.ViewModels;

namespace SkyDrop.Views;

public partial class RevealFeedGameView : UserControl
{
    private readonly IInputService _inputService;
    private PauseOverlay? _pauseOverlay;
    private InstructionsOverlay? _instructionsOverlay;
    // Track if first input after game over has been consumed (to ignore held keys)
    private bool _gameOverInputReady;
    private bool _wasGameOver;

    public RevealFeedGameView()
    {
        InitializeComponent();
        _inputService = App.Services.GetRequiredService<IInputService>();
    }

    protected override void OnLoaded(global::Avalonia.Interactivity.RoutedEventArgs e)
    {
        base.OnLoaded(e);

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
    }

    private void OnResumeRequested(object? sender, EventArgs e)
    {
        if (DataContext is RevealFeedGameViewModel vm)
        {
            vm.HandleKeyDown(GameInput.Pause);
            Focus();
        }
    }

    private void OnMainMenuRequested(object? sender, EventArgs e)
    {
        if (DataContext is RevealFeedGameViewModel vm)
        {
            vm.ReturnToTitle();
        }
    }

    private void OnStartRequested(object? sender, EventArgs e)
    {
        if (DataContext is RevealFeedGameViewModel vm)
        {
            vm.DismissInstructionsAndStart();
            Focus();
        }
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
            if (DataContext is not RevealFeedGameViewModel vm) return;

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
            if (DataContext is not RevealFeedGameViewModel vm) return;

            // Handle game over restart with Confirm (Enter/Space)
            if (vm.IsGameOver && _gameOverInputReady && e.Input == UIInput.Confirm)
            {
                _ = vm.RestartGameAsync();
            }
        });
    }
}
