using System.Collections.Specialized;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using SkyDrop.Controls;
using SkyDrop.Models;
using SkyDrop.Services;
using SkyDrop.ViewModels;

namespace SkyDrop.Views;

public partial class CreatePostGameView : UserControl
{
    private readonly IInputService _inputService;
    private readonly Border?[] _keyBorders = new Border?[13];
    private IBrush? _normalKeyBackground;
    private IBrush? _selectedKeyBackground;
    private ScrollViewer? _completedPostsScroller;
    private PauseOverlay? _pauseOverlay;
    private InstructionsOverlay? _instructionsOverlay;

    // Game Over menu elements
    private Border? _toggleOption;
    private Border? _postOption;
    private Border? _restartButton;
    private Border? _titleButton;
    private IBrush? _menuNormalBackground;
    private IBrush? _menuSelectedBackground;
    private IBrush? _greenBrush;
    private IBrush? _cyanBrush;
    private IBrush? _buttonBrush;

    // Track game over input state - wait for key release before accepting inputs
    private bool _gameOverInputReady;
    private bool _wasGameOver;

    public CreatePostGameView()
    {
        InitializeComponent();
        _inputService = App.Services.GetRequiredService<IInputService>();
        DataContextChanged += OnDataContextChanged;
    }

    protected override void OnLoaded(global::Avalonia.Interactivity.RoutedEventArgs e)
    {
        base.OnLoaded(e);

        // Cache key border references
        _keyBorders[0] = this.FindControl<Border>("Key1");
        _keyBorders[1] = this.FindControl<Border>("Key2");
        _keyBorders[2] = this.FindControl<Border>("Key3");
        _keyBorders[3] = this.FindControl<Border>("Key4");
        _keyBorders[4] = this.FindControl<Border>("Key5");
        _keyBorders[5] = this.FindControl<Border>("Key6");
        _keyBorders[6] = this.FindControl<Border>("Key7");
        _keyBorders[7] = this.FindControl<Border>("Key8");
        _keyBorders[8] = this.FindControl<Border>("Key9");
        _keyBorders[9] = this.FindControl<Border>("KeyStar");
        _keyBorders[10] = this.FindControl<Border>("Key0");
        _keyBorders[11] = this.FindControl<Border>("KeyPound");
        _keyBorders[12] = this.FindControl<Border>("KeyOK");

        // Get brush resources
        if (this.TryFindResource("ButtonBackgroundBrush", this.ActualThemeVariant, out var normalBrush))
            _normalKeyBackground = normalBrush as IBrush;
        if (this.TryFindResource("AccentGoldBrush", this.ActualThemeVariant, out var selectedBrush))
            _selectedKeyBackground = selectedBrush as IBrush;
        if (this.TryFindResource("NextPieceBackgroundBrush", this.ActualThemeVariant, out var menuNormal))
            _menuNormalBackground = menuNormal as IBrush;
        if (this.TryFindResource("AccentGreenBrush", this.ActualThemeVariant, out var green))
            _greenBrush = green as IBrush;
        if (this.TryFindResource("AccentCyanBrush", this.ActualThemeVariant, out var cyan))
            _cyanBrush = cyan as IBrush;
        if (this.TryFindResource("ButtonBackgroundBrush", this.ActualThemeVariant, out var button))
            _buttonBrush = button as IBrush;

        _menuSelectedBackground = _selectedKeyBackground;

        // Cache ScrollViewer reference
        _completedPostsScroller = this.FindControl<ScrollViewer>("CompletedPostsScroller");

        // Wire up PauseOverlay
        _pauseOverlay = this.FindControl<PauseOverlay>("PauseOverlay");
        if (_pauseOverlay != null)
        {
            _pauseOverlay.ResumeRequested += OnResumeRequested;
            _pauseOverlay.MainMenuRequested += OnMainMenuRequested;
        }

        // Wire up InstructionsOverlay
        _instructionsOverlay = this.FindControl<InstructionsOverlay>("InstructionsOverlay");
        if (_instructionsOverlay != null)
        {
            _instructionsOverlay.StartRequested += OnStartRequested;
        }

        // Cache game over menu elements
        _toggleOption = this.FindControl<Border>("ToggleOption");
        _postOption = this.FindControl<Border>("PostOption");
        _restartButton = this.FindControl<Border>("RestartButton");
        _titleButton = this.FindControl<Border>("TitleButton");

        // Wire up pointer handlers for mouse support
        if (_restartButton != null)
            _restartButton.PointerPressed += (_, _) => (DataContext as CreatePostGameViewModel)?.RestartGame();
        if (_titleButton != null)
            _titleButton.PointerPressed += (_, _) => (DataContext as CreatePostGameViewModel)?.ReturnToTitle();
        if (_toggleOption != null)
            _toggleOption.PointerPressed += (_, _) =>
            {
                if (DataContext is CreatePostGameViewModel vm)
                    vm.IncludeSkyDropSignature = !vm.IncludeSkyDropSignature;
            };
        if (_postOption != null)
            _postOption.PointerPressed += (_, _) =>
            {
                if (DataContext is CreatePostGameViewModel vm && vm.CanPost)
                    vm.PostToBlueskyCommand.Execute(null);
            };

        UpdateKeyHighlight();
    }

    private void OnResumeRequested(object? sender, EventArgs e)
    {
        if (DataContext is CreatePostGameViewModel vm)
        {
            vm.HandleKeyDown(GameInput.Pause);
            Focus();
        }
    }

    private void OnMainMenuRequested(object? sender, EventArgs e)
    {
        if (DataContext is CreatePostGameViewModel vm)
        {
            vm.ReturnToTitle();
        }
    }

    private void OnStartRequested(object? sender, EventArgs e)
    {
        if (DataContext is CreatePostGameViewModel vm)
        {
            vm.DismissInstructionsAndStart();
            Focus();
        }
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (DataContext is CreatePostGameViewModel vm)
        {
            vm.PropertyChanged += (_, args) =>
            {
                if (args.PropertyName == nameof(CreatePostGameViewModel.SelectedKeyIndex))
                {
                    UpdateKeyHighlight();
                }
                else if (args.PropertyName == nameof(CreatePostGameViewModel.GameOverMenuIndex) ||
                         args.PropertyName == nameof(CreatePostGameViewModel.IsGameOver) ||
                         args.PropertyName == nameof(CreatePostGameViewModel.HasPosted))
                {
                    UpdateGameOverMenuHighlight();
                }
            };

            // Auto-scroll to bottom when posts are added
            vm.CompletedPosts.CollectionChanged += OnCompletedPostsChanged;
        }
    }

    private void OnCompletedPostsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Add && _completedPostsScroller != null)
        {
            // Scroll to bottom after layout updates
            Dispatcher.UIThread.Post(() =>
            {
                _completedPostsScroller.ScrollToEnd();
            }, DispatcherPriority.Background);
        }
    }

    private void UpdateKeyHighlight()
    {
        if (DataContext is not CreatePostGameViewModel vm) return;

        for (int i = 0; i < _keyBorders.Length; i++)
        {
            var border = _keyBorders[i];
            if (border == null) continue;

            // Always use same border thickness to prevent size changes
            border.BorderThickness = new Thickness(2);

            if (i == vm.SelectedKeyIndex)
            {
                border.Background = _selectedKeyBackground ?? Brushes.Gold;
                border.BorderBrush = Brushes.White;
            }
            else
            {
                border.Background = _normalKeyBackground ?? Brushes.DarkGray;
                border.BorderBrush = Brushes.Transparent;
            }
        }
    }

    private void UpdateGameOverMenuHighlight()
    {
        if (DataContext is not CreatePostGameViewModel vm) return;

        var hasPosts = vm.CompletedPosts.Count > 0;

        // Reset all to normal state with consistent border thickness
        if (_toggleOption != null)
        {
            _toggleOption.Background = _menuNormalBackground ?? Brushes.DarkGray;
            _toggleOption.BorderBrush = Brushes.Transparent;
            _toggleOption.BorderThickness = new Thickness(2);
        }

        if (_postOption != null && vm.CanPost)
        {
            _postOption.Background = _greenBrush ?? Brushes.Green;
            _postOption.BorderBrush = Brushes.Transparent;
            _postOption.BorderThickness = new Thickness(2);
        }

        if (_restartButton != null)
        {
            _restartButton.Background = _cyanBrush ?? Brushes.Cyan;
            _restartButton.BorderBrush = Brushes.Transparent;
            _restartButton.BorderThickness = new Thickness(2);
        }

        if (_titleButton != null)
        {
            _titleButton.Background = _buttonBrush ?? Brushes.Gray;
            _titleButton.BorderBrush = Brushes.Transparent;
            _titleButton.BorderThickness = new Thickness(2);
        }

        // Highlight selected item
        if (hasPosts)
        {
            // Menu: 0=Toggle, 1=Post, 2=Play Again, 3=Title
            switch (vm.GameOverMenuIndex)
            {
                case 0 when _toggleOption != null:
                    _toggleOption.Background = _menuSelectedBackground ?? Brushes.Gold;
                    _toggleOption.BorderBrush = Brushes.White;
                    break;
                case 1 when _postOption != null && vm.CanPost:
                    _postOption.Background = _menuSelectedBackground ?? Brushes.Gold;
                    _postOption.BorderBrush = Brushes.White;
                    break;
                case 2 when _restartButton != null:
                    _restartButton.Background = _menuSelectedBackground ?? Brushes.Gold;
                    _restartButton.BorderBrush = Brushes.White;
                    break;
                case 3 when _titleButton != null:
                    _titleButton.Background = _menuSelectedBackground ?? Brushes.Gold;
                    _titleButton.BorderBrush = Brushes.White;
                    break;
            }
        }
        else
        {
            // Menu: 0=Play Again, 1=Title
            switch (vm.GameOverMenuIndex)
            {
                case 0 when _restartButton != null:
                    _restartButton.Background = _menuSelectedBackground ?? Brushes.Gold;
                    _restartButton.BorderBrush = Brushes.White;
                    break;
                case 1 when _titleButton != null:
                    _titleButton.Background = _menuSelectedBackground ?? Brushes.Gold;
                    _titleButton.BorderBrush = Brushes.White;
                    break;
            }
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
            if (DataContext is not CreatePostGameViewModel vm) return;

            // Track game over state transitions - disable input until first key release
            if (vm.IsGameOver && !_wasGameOver)
            {
                _gameOverInputReady = false;
            }
            _wasGameOver = vm.IsGameOver;

            // Handle game over menu navigation
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

                // Game over menu navigation
                switch (e.Input)
                {
                    case GameInput.Left:
                    case GameInput.RotateCW: // Up
                        vm.NavigateGameOverMenu(down: false);
                        return;
                    case GameInput.Right:
                    case GameInput.SoftDrop: // Down
                        vm.NavigateGameOverMenu(down: true);
                        return;
                    case GameInput.Pause: // Escape
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
            if (DataContext is not CreatePostGameViewModel vm) return;

            // Handle game over menu confirm
            if (vm.IsGameOver && _gameOverInputReady)
            {
                if (e.Input == UIInput.Confirm)
                {
                    vm.ActivateGameOverMenuItem();
                    return;
                }
            }

            // Handle TAB for cycling T9 completions or kanji suggestions (during gameplay)
            if (!vm.IsGameOver && e.Input == UIInput.Secondary)
            {
                if (vm.IsKanaMode)
                {
                    vm.CycleKanjiSuggestion();
                }
                else
                {
                    vm.CycleT9Completion();
                }
            }
        });
    }

    // Special key handling that doesn't fit the standard input abstraction
    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        if (DataContext is not CreatePostGameViewModel vm) return;

        // Don't handle special keys during game over
        if (vm.IsGameOver) return;

        // Handle M for toggling between T9 and ABC modes
        if (e.Key == Key.M)
        {
            vm.ToggleInputMode();
            e.Handled = true;
        }
    }
}
