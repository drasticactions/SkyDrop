using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Microsoft.Extensions.DependencyInjection;
using SkyDrop.Models;
using SkyDrop.Services;
using SkyDrop.ViewModels;

namespace SkyDrop.Views;

public partial class NormalModeOptionsView : UserControl
{
    private readonly IInputService _inputService;
    private enum NavSection { Level, Play, Back }

    private NavSection _currentSection = NavSection.Level;
    private Border? _levelSelectorBorder;
    private Border? _playButtonBorder;
    private Border? _backButtonBorder;

    public NormalModeOptionsView()
    {
        InitializeComponent();
        _inputService = App.Services.GetRequiredService<IInputService>();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        _levelSelectorBorder = this.FindControl<Border>("LevelSelectorBorder");
        _playButtonBorder = this.FindControl<Border>("PlayButtonBorder");
        _backButtonBorder = this.FindControl<Border>("BackButtonBorder");

        UpdateSelectionVisuals();
        Focus();
    }

    private void UpdateSelectionVisuals()
    {
        var accentBrush = this.FindResource("AccentCyanBrush") as IBrush;
        var transparentBrush = Brushes.Transparent;

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
        if (DataContext is not NormalModeOptionsViewModel vm) return;

        switch (e.Input)
        {
            case UIInput.Confirm:
                switch (_currentSection)
                {
                    case NavSection.Level:
                    case NavSection.Play:
                        vm.PlayCommand.Execute(null);
                        break;
                    case NavSection.Back:
                        vm.BackCommand.Execute(null);
                        break;
                }
                break;

            case UIInput.Cancel:
                vm.BackCommand.Execute(null);
                break;

            case UIInput.Up:
                _currentSection = _currentSection switch
                {
                    NavSection.Play => NavSection.Level,
                    NavSection.Back => NavSection.Play,
                    _ => _currentSection
                };
                UpdateSelectionVisuals();
                break;

            case UIInput.Down:
                _currentSection = _currentSection switch
                {
                    NavSection.Level => NavSection.Play,
                    NavSection.Play => NavSection.Back,
                    _ => _currentSection
                };
                UpdateSelectionVisuals();
                break;

            case UIInput.Left:
                if (_currentSection == NavSection.Level)
                {
                    vm.DecreaseLevelCommand.Execute(null);
                }
                break;

            case UIInput.Right:
                if (_currentSection == NavSection.Level)
                {
                    vm.IncreaseLevelCommand.Execute(null);
                }
                break;
        }
    }
}
