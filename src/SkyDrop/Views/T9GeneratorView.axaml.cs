using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Microsoft.Extensions.DependencyInjection;
using SkyDrop.Models;
using SkyDrop.Services;
using SkyDrop.ViewModels;

namespace SkyDrop.Views;

public partial class T9GeneratorView : UserControl
{
    private readonly IInputService _inputService;

    private enum NavSection { ModeTab, Input, Clear, Back }

    private NavSection _currentSection = NavSection.Input;
    private Border? _textToT9TabBorder;
    private Border? _t9ToTextTabBorder;
    private Border? _inputTextBorder;
    private Border? _t9InputBorder;
    private Border? _clearButtonBorder;
    private Border? _backButtonBorder;
    private TextBox? _inputTextBox;
    private TextBox? _t9InputTextBox;

    public T9GeneratorView()
    {
        InitializeComponent();
        _inputService = App.Services.GetRequiredService<IInputService>();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        _textToT9TabBorder = this.FindControl<Border>("TextToT9TabBorder");
        _t9ToTextTabBorder = this.FindControl<Border>("T9ToTextTabBorder");
        _inputTextBorder = this.FindControl<Border>("InputTextBorder");
        _t9InputBorder = this.FindControl<Border>("T9InputBorder");
        _clearButtonBorder = this.FindControl<Border>("ClearButtonBorder");
        _backButtonBorder = this.FindControl<Border>("BackButtonBorder");
        _inputTextBox = this.FindControl<TextBox>("InputTextBox");
        _t9InputTextBox = this.FindControl<TextBox>("T9InputTextBox");

        if (DataContext is T9GeneratorViewModel vm)
        {
            vm.PropertyChanged += (_, args) =>
            {
                if (args.PropertyName == nameof(T9GeneratorViewModel.CurrentMode))
                {
                    UpdateSelectionVisuals();
                }
            };
        }

        UpdateSelectionVisuals();
        Focus();
    }

    private void UpdateSelectionVisuals()
    {
        var accentBrush = this.FindResource("AccentCyanBrush") as IBrush;
        var transparentBrush = Brushes.Transparent;
        var buttonBrush = this.FindResource("ButtonBackgroundBrush") as IBrush;

        var vm = DataContext as T9GeneratorViewModel;
        var isTextToT9 = vm?.IsTextToT9Mode ?? true;

        // Update tab backgrounds based on current mode
        if (_textToT9TabBorder != null)
        {
            _textToT9TabBorder.Background = isTextToT9 ? accentBrush : buttonBrush;
            _textToT9TabBorder.BorderBrush = _currentSection == NavSection.ModeTab
                ? Brushes.White
                : transparentBrush;
        }

        if (_t9ToTextTabBorder != null)
        {
            _t9ToTextTabBorder.Background = !isTextToT9 ? accentBrush : buttonBrush;
            _t9ToTextTabBorder.BorderBrush = _currentSection == NavSection.ModeTab
                ? Brushes.White
                : transparentBrush;
        }

        // Update input border
        if (_inputTextBorder != null)
        {
            _inputTextBorder.BorderBrush = (_currentSection == NavSection.Input && isTextToT9)
                ? accentBrush
                : transparentBrush;
        }

        if (_t9InputBorder != null)
        {
            _t9InputBorder.BorderBrush = (_currentSection == NavSection.Input && !isTextToT9)
                ? accentBrush
                : transparentBrush;
        }

        // Update button borders
        if (_clearButtonBorder != null)
        {
            _clearButtonBorder.BorderBrush = _currentSection == NavSection.Clear
                ? accentBrush
                : transparentBrush;
        }

        if (_backButtonBorder != null)
        {
            _backButtonBorder.BorderBrush = _currentSection == NavSection.Back
                ? accentBrush
                : transparentBrush;
        }
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _inputService.UIInputReceived += OnUIInputReceived;
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _inputService.UIInputReceived -= OnUIInputReceived;
        base.OnDetachedFromVisualTree(e);
    }

    private bool IsTextBoxFocused()
    {
        return _inputTextBox?.IsFocused == true || _t9InputTextBox?.IsFocused == true;
    }

    private void OnUIInputReceived(object? sender, UIInputEventArgs e)
    {
        if (DataContext is not T9GeneratorViewModel vm) return;

        var isTextBoxFocused = IsTextBoxFocused();

        // Handle TextBox-specific inputs
        if (isTextBoxFocused)
        {
            if (e.Input == UIInput.Cancel)
            {
                Focus();
            }
            // Let other inputs propagate to the TextBox
            return;
        }

        switch (e.Input)
        {
            case UIInput.Confirm:
                HandleConfirm(vm);
                break;

            case UIInput.Cancel:
                vm.BackCommand.Execute(null);
                break;

            case UIInput.Up:
                NavigateUp();
                break;

            case UIInput.Down:
                NavigateDown();
                break;

            case UIInput.Left:
            case UIInput.Right:
                if (_currentSection == NavSection.ModeTab)
                {
                    vm.SwitchModeCommand.Execute(null);
                }
                break;

            case UIInput.Secondary: // Tab
                vm.SwitchModeCommand.Execute(null);
                _currentSection = NavSection.Input;
                UpdateSelectionVisuals();
                break;
        }
    }

    private void HandleConfirm(T9GeneratorViewModel vm)
    {
        switch (_currentSection)
        {
            case NavSection.ModeTab:
                vm.SwitchModeCommand.Execute(null);
                break;
            case NavSection.Input:
                var textBox = vm.IsTextToT9Mode ? _inputTextBox : _t9InputTextBox;
                textBox?.Focus();
                break;
            case NavSection.Clear:
                vm.ClearAllCommand.Execute(null);
                break;
            case NavSection.Back:
                vm.BackCommand.Execute(null);
                break;
        }
    }

    private void NavigateUp()
    {
        _currentSection = _currentSection switch
        {
            NavSection.Input => NavSection.ModeTab,
            NavSection.Clear => NavSection.Input,
            NavSection.Back => NavSection.Clear,
            _ => _currentSection
        };
        UpdateSelectionVisuals();
    }

    private void NavigateDown()
    {
        _currentSection = _currentSection switch
        {
            NavSection.ModeTab => NavSection.Input,
            NavSection.Input => NavSection.Clear,
            NavSection.Clear => NavSection.Back,
            _ => _currentSection
        };
        UpdateSelectionVisuals();
    }
}
