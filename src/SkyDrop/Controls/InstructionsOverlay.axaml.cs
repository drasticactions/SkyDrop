using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;

namespace SkyDrop.Controls;

public partial class InstructionsOverlay : UserControl
{
    public static readonly StyledProperty<bool> IsVisibleOverlayProperty =
        AvaloniaProperty.Register<InstructionsOverlay, bool>(nameof(IsVisibleOverlay));

    public static readonly StyledProperty<string?> ModeInstructionsProperty =
        AvaloniaProperty.Register<InstructionsOverlay, string?>(nameof(ModeInstructions));

    public bool IsVisibleOverlay
    {
        get => GetValue(IsVisibleOverlayProperty);
        set => SetValue(IsVisibleOverlayProperty, value);
    }

    public string? ModeInstructions
    {
        get => GetValue(ModeInstructionsProperty);
        set => SetValue(ModeInstructionsProperty, value);
    }

    public event EventHandler? StartRequested;

    public InstructionsOverlay()
    {
        InitializeComponent();
        IsVisible = false;

        var startButton = this.FindControl<Button>("StartButton");
        if (startButton != null)
            startButton.Click += OnStartClick;
    }

    private void OnStartClick(object? sender, RoutedEventArgs e)
    {
        StartRequested?.Invoke(this, EventArgs.Empty);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        if (IsVisibleOverlay && (e.Key == Key.Enter || e.Key == Key.Space))
        {
            StartRequested?.Invoke(this, EventArgs.Empty);
            e.Handled = true;
        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == IsVisibleOverlayProperty)
        {
            IsVisible = (bool)(change.NewValue ?? false);
            if (IsVisible)
            {
                // Defer focus to ensure control is ready
                Dispatcher.UIThread.Post(() => Focus(), DispatcherPriority.Background);
            }
        }
        else if (change.Property == ModeInstructionsProperty)
        {
            var modeText = this.FindControl<TextBlock>("ModeInstructionsText");
            if (modeText != null)
            {
                var instructions = change.NewValue as string;
                modeText.Text = instructions;
                modeText.IsVisible = !string.IsNullOrEmpty(instructions);
            }
        }
    }
}
