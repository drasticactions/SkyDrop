using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;

namespace SkyDrop.Controls;

public partial class PauseOverlay : UserControl
{
    public static readonly StyledProperty<bool> IsPausedProperty =
        AvaloniaProperty.Register<PauseOverlay, bool>(nameof(IsPaused));

    public bool IsPaused
    {
        get => GetValue(IsPausedProperty);
        set => SetValue(IsPausedProperty, value);
    }

    private int _selectedButtonIndex = 0;
    private Button? _resumeButton;
    private Button? _mainMenuButton;
    private Border? _resumeButtonBorder;
    private Border? _mainMenuButtonBorder;

    public event EventHandler? ResumeRequested;
    public event EventHandler? MainMenuRequested;

    public PauseOverlay()
    {
        InitializeComponent();
        IsVisible = false;

        _resumeButton = this.FindControl<Button>("ResumeButton");
        _mainMenuButton = this.FindControl<Button>("MainMenuButton");
        _resumeButtonBorder = this.FindControl<Border>("ResumeButtonBorder");
        _mainMenuButtonBorder = this.FindControl<Border>("MainMenuButtonBorder");

        if (_resumeButton != null)
            _resumeButton.Click += OnResumeClick;
        if (_mainMenuButton != null)
            _mainMenuButton.Click += OnMainMenuClick;
    }

    private void OnResumeClick(object? sender, RoutedEventArgs e)
    {
        ResumeRequested?.Invoke(this, EventArgs.Empty);
    }

    private void OnMainMenuClick(object? sender, RoutedEventArgs e)
    {
        MainMenuRequested?.Invoke(this, EventArgs.Empty);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == IsPausedProperty)
        {
            var isPaused = (bool)(change.NewValue ?? false);
            IsVisible = isPaused;

            if (isPaused)
            {
                _selectedButtonIndex = 0;
                UpdateButtonSelection();
                Dispatcher.UIThread.Post(() => Focus(), DispatcherPriority.Background);
            }
        }
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        if (!IsVisible) return;

        switch (e.Key)
        {
            case Key.Up:
            case Key.W:
                _selectedButtonIndex = 0;
                UpdateButtonSelection();
                e.Handled = true;
                break;

            case Key.Down:
            case Key.S:
                _selectedButtonIndex = 1;
                UpdateButtonSelection();
                e.Handled = true;
                break;

            case Key.Enter:
            case Key.Space:
                ActivateSelectedButton();
                e.Handled = true;
                break;
        }
    }

    private void UpdateButtonSelection()
    {
        var accentBrush = this.FindResource("AccentCyanBrush") as IBrush;
        var transparentBrush = Brushes.Transparent;

        if (_resumeButtonBorder != null)
        {
            _resumeButtonBorder.BorderBrush = _selectedButtonIndex == 0 ? accentBrush : transparentBrush;
        }
        if (_mainMenuButtonBorder != null)
        {
            _mainMenuButtonBorder.BorderBrush = _selectedButtonIndex == 1 ? accentBrush : transparentBrush;
        }
    }

    private void ActivateSelectedButton()
    {
        if (_selectedButtonIndex == 0)
        {
            ResumeRequested?.Invoke(this, EventArgs.Empty);
        }
        else
        {
            MainMenuRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}
