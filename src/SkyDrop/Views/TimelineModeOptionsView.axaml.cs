using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Microsoft.Extensions.DependencyInjection;
using SkyDrop.Models;
using SkyDrop.Services;
using SkyDrop.ViewModels;

namespace SkyDrop.Views;

public partial class TimelineModeOptionsView : UserControl
{
    private readonly IInputService _inputService;
    private enum NavSection { GameType, Level, Play, Back }

    private NavSection _currentSection = NavSection.GameType;
    private Border? _gameTypeSelectorBorder;
    private Border? _revealModeButton;
    private Border? _scrollModeButton;
    private Border? _levelSelectorBorder;
    private Border? _playButtonBorder;
    private Border? _backButtonBorder;

    private IBrush? _accentBrush;
    private IBrush? _buttonBrush;

    public TimelineModeOptionsView()
    {
        InitializeComponent();
        _inputService = App.Services.GetRequiredService<IInputService>();
        DataContextChanged += OnDataContextChanged;
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        _gameTypeSelectorBorder = this.FindControl<Border>("GameTypeSelectorBorder");
        _revealModeButton = this.FindControl<Border>("RevealModeButton");
        _scrollModeButton = this.FindControl<Border>("ScrollModeButton");
        _levelSelectorBorder = this.FindControl<Border>("LevelSelectorBorder");
        _playButtonBorder = this.FindControl<Border>("PlayButtonBorder");
        _backButtonBorder = this.FindControl<Border>("BackButtonBorder");

        _accentBrush = this.FindResource("AccentCyanBrush") as IBrush;
        _buttonBrush = this.FindResource("ButtonBackgroundBrush") as IBrush;

        // Wire up click handlers for game type buttons
        if (_revealModeButton != null)
        {
            _revealModeButton.PointerPressed += (_, _) =>
            {
                if (DataContext is TimelineModeOptionsViewModel vm)
                    vm.SelectRevealModeCommand.Execute(null);
            };
        }

        if (_scrollModeButton != null)
        {
            _scrollModeButton.PointerPressed += (_, _) =>
            {
                if (DataContext is TimelineModeOptionsViewModel vm)
                    vm.SelectScrollModeCommand.Execute(null);
            };
        }

        UpdateSelectionVisuals();
        UpdateGameTypeVisuals();
        Focus();
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (DataContext is TimelineModeOptionsViewModel vm)
        {
            vm.PropertyChanged += (_, args) =>
            {
                if (args.PropertyName == nameof(TimelineModeOptionsViewModel.SelectedGameType))
                {
                    UpdateGameTypeVisuals();
                }
            };
        }
    }

    private void UpdateGameTypeVisuals()
    {
        if (DataContext is not TimelineModeOptionsViewModel vm) return;

        var selectedBrush = _accentBrush ?? Brushes.Cyan;
        var normalBrush = _buttonBrush ?? Brushes.DarkGray;

        if (_revealModeButton != null)
        {
            _revealModeButton.Background = vm.IsRevealMode ? selectedBrush : normalBrush;
        }

        if (_scrollModeButton != null)
        {
            _scrollModeButton.Background = vm.IsScrollMode ? selectedBrush : normalBrush;
        }
    }

    private void UpdateSelectionVisuals()
    {
        var accentBrush = _accentBrush ?? Brushes.Cyan;
        var transparentBrush = Brushes.Transparent;

        if (_gameTypeSelectorBorder != null)
        {
            _gameTypeSelectorBorder.BorderBrush = _currentSection == NavSection.GameType ? accentBrush : transparentBrush;
        }

        if (_levelSelectorBorder != null)
        {
            _levelSelectorBorder.BorderBrush = _currentSection == NavSection.Level ? accentBrush : transparentBrush;
        }

        if (_playButtonBorder != null)
        {
            _playButtonBorder.BorderBrush = _currentSection == NavSection.Play ? accentBrush : transparentBrush;
        }

        if (_backButtonBorder != null)
        {
            _backButtonBorder.BorderBrush = _currentSection == NavSection.Back ? accentBrush : transparentBrush;
        }

        ScrollToCurrentSection();
    }

    private void ScrollToCurrentSection()
    {
        Control? targetControl = _currentSection switch
        {
            NavSection.GameType => _gameTypeSelectorBorder,
            NavSection.Level => _levelSelectorBorder,
            NavSection.Play => _playButtonBorder,
            NavSection.Back => _backButtonBorder,
            _ => null
        };

        targetControl?.BringIntoView();
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

    private void OnUIInputReceived(object? sender, UIInputEventArgs e)
    {
        if (DataContext is not TimelineModeOptionsViewModel vm) return;

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
                HandleLeft(vm);
                break;

            case UIInput.Right:
                HandleRight(vm);
                break;
        }
    }

    private void HandleConfirm(TimelineModeOptionsViewModel vm)
    {
        switch (_currentSection)
        {
            case NavSection.GameType:
                vm.ToggleGameTypeCommand.Execute(null);
                break;
            case NavSection.Level:
                vm.PlayCommand.Execute(null);
                break;
            case NavSection.Play:
                vm.PlayCommand.Execute(null);
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
            NavSection.Level => NavSection.GameType,
            NavSection.Play => NavSection.Level,
            NavSection.Back => NavSection.Play,
            _ => _currentSection
        };
        UpdateSelectionVisuals();
    }

    private void NavigateDown()
    {
        _currentSection = _currentSection switch
        {
            NavSection.GameType => NavSection.Level,
            NavSection.Level => NavSection.Play,
            NavSection.Play => NavSection.Back,
            _ => _currentSection
        };
        UpdateSelectionVisuals();
    }

    private void HandleLeft(TimelineModeOptionsViewModel vm)
    {
        switch (_currentSection)
        {
            case NavSection.Level:
                vm.DecreaseLevelCommand.Execute(null);
                break;
            case NavSection.GameType:
                vm.SelectRevealModeCommand.Execute(null);
                break;
        }
    }

    private void HandleRight(TimelineModeOptionsViewModel vm)
    {
        switch (_currentSection)
        {
            case NavSection.Level:
                vm.IncreaseLevelCommand.Execute(null);
                break;
            case NavSection.GameType:
                vm.SelectScrollModeCommand.Execute(null);
                break;
        }
    }
}
