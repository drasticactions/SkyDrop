using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Interactivity;
using Avalonia.Media;
using Microsoft.Extensions.DependencyInjection;
using SkyDrop.Models;
using SkyDrop.Services;
using SkyDrop.ViewModels;

namespace SkyDrop.Views;

public partial class CreatePostModeOptionsView : UserControl
{
    private readonly IInputService _inputService;
    private enum NavSection { Level, InputMode, Controls, Play, Back }

    private NavSection _currentSection = NavSection.Level;
    private int _carouselIndex = 0;
    private const int CarouselPageCount = 4; // Controls, T9, ABC, Kana

    private ScrollViewer? _mainScrollViewer;
    private Border? _levelSelectorBorder;
    private Border? _inputModeSelectorBorder;
    private Border? _controlsCarouselBorder;
    private Border? _playButtonBorder;
    private Border? _backButtonBorder;

    // Carousel panels
    private StackPanel? _controlsPanel;
    private StackPanel? _t9Panel;
    private StackPanel? _abcPanel;
    private StackPanel? _kanaPanel;

    // Carousel indicators
    private Ellipse? _indicator0;
    private Ellipse? _indicator1;
    private Ellipse? _indicator2;
    private Ellipse? _indicator3;

    // Carousel buttons
    private Button? _carouselLeftButton;
    private Button? _carouselRightButton;

    public CreatePostModeOptionsView()
    {
        InitializeComponent();
        _inputService = App.Services.GetRequiredService<IInputService>();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        _mainScrollViewer = this.FindControl<ScrollViewer>("MainScrollViewer");
        _levelSelectorBorder = this.FindControl<Border>("LevelSelectorBorder");
        _inputModeSelectorBorder = this.FindControl<Border>("InputModeSelectorBorder");
        _controlsCarouselBorder = this.FindControl<Border>("ControlsCarouselBorder");
        _playButtonBorder = this.FindControl<Border>("PlayButtonBorder");
        _backButtonBorder = this.FindControl<Border>("BackButtonBorder");

        // Carousel panels
        _controlsPanel = this.FindControl<StackPanel>("ControlsPanel");
        _t9Panel = this.FindControl<StackPanel>("T9Panel");
        _abcPanel = this.FindControl<StackPanel>("AbcPanel");
        _kanaPanel = this.FindControl<StackPanel>("KanaPanel");

        // Carousel indicators
        _indicator0 = this.FindControl<Ellipse>("Indicator0");
        _indicator1 = this.FindControl<Ellipse>("Indicator1");
        _indicator2 = this.FindControl<Ellipse>("Indicator2");
        _indicator3 = this.FindControl<Ellipse>("Indicator3");

        // Carousel buttons
        _carouselLeftButton = this.FindControl<Button>("CarouselLeftButton");
        _carouselRightButton = this.FindControl<Button>("CarouselRightButton");

        if (_carouselLeftButton != null)
        {
            _carouselLeftButton.Click += (_, _) => CycleCarousel(false);
        }

        if (_carouselRightButton != null)
        {
            _carouselRightButton.Click += (_, _) => CycleCarousel(true);
        }

        UpdateSelectionVisuals();
        UpdateCarouselVisuals();
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

        if (_inputModeSelectorBorder != null)
        {
            _inputModeSelectorBorder.BorderBrush = _currentSection == NavSection.InputMode ? accentBrush : transparentBrush;
        }

        if (_controlsCarouselBorder != null)
        {
            _controlsCarouselBorder.BorderBrush = _currentSection == NavSection.Controls ? accentBrush : transparentBrush;
        }

        if (_playButtonBorder != null)
        {
            _playButtonBorder.BorderBrush = _currentSection == NavSection.Play ? accentBrush : transparentBrush;
        }

        if (_backButtonBorder != null)
        {
            _backButtonBorder.BorderBrush = _currentSection == NavSection.Back ? accentBrush : transparentBrush;
        }

        // Scroll to make selected section visible
        ScrollToCurrentSection();
    }

    private void ScrollToCurrentSection()
    {
        Control? targetControl = _currentSection switch
        {
            NavSection.Level => _levelSelectorBorder,
            NavSection.InputMode => _inputModeSelectorBorder,
            NavSection.Controls => _controlsCarouselBorder,
            NavSection.Play => _playButtonBorder,
            NavSection.Back => _backButtonBorder,
            _ => null
        };

        targetControl?.BringIntoView();
    }

    private void CycleCarousel(bool forward)
    {
        if (forward)
        {
            _carouselIndex = (_carouselIndex + 1) % CarouselPageCount;
        }
        else
        {
            _carouselIndex = (_carouselIndex - 1 + CarouselPageCount) % CarouselPageCount;
        }

        UpdateCarouselVisuals();
    }

    private void UpdateCarouselVisuals()
    {
        // Update panel visibility
        if (_controlsPanel != null) _controlsPanel.IsVisible = _carouselIndex == 0;
        if (_t9Panel != null) _t9Panel.IsVisible = _carouselIndex == 1;
        if (_abcPanel != null) _abcPanel.IsVisible = _carouselIndex == 2;
        if (_kanaPanel != null) _kanaPanel.IsVisible = _carouselIndex == 3;

        // Update indicators
        var activeBrush = this.FindResource("AccentCyanBrush") as IBrush ?? Brushes.Cyan;
        var inactiveBrush = this.FindResource("ButtonBackgroundBrush") as IBrush ?? Brushes.Gray;

        if (_indicator0 != null) _indicator0.Fill = _carouselIndex == 0 ? activeBrush : inactiveBrush;
        if (_indicator1 != null) _indicator1.Fill = _carouselIndex == 1 ? activeBrush : inactiveBrush;
        if (_indicator2 != null) _indicator2.Fill = _carouselIndex == 2 ? activeBrush : inactiveBrush;
        if (_indicator3 != null) _indicator3.Fill = _carouselIndex == 3 ? activeBrush : inactiveBrush;
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
        if (DataContext is not CreatePostModeOptionsViewModel vm) return;

        switch (e.Input)
        {
            case UIInput.Confirm:
                switch (_currentSection)
                {
                    case NavSection.Level:
                    case NavSection.Play:
                        vm.PlayCommand.Execute(null);
                        break;
                    case NavSection.InputMode:
                        vm.ToggleInputModeCommand.Execute(null);
                        break;
                    case NavSection.Controls:
                        // On confirm, cycle carousel forward
                        CycleCarousel(true);
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
                    NavSection.InputMode => NavSection.Level,
                    NavSection.Controls => NavSection.InputMode,
                    NavSection.Play => NavSection.Controls,
                    NavSection.Back => NavSection.Play,
                    _ => _currentSection
                };
                UpdateSelectionVisuals();
                break;

            case UIInput.Down:
                _currentSection = _currentSection switch
                {
                    NavSection.Level => NavSection.InputMode,
                    NavSection.InputMode => NavSection.Controls,
                    NavSection.Controls => NavSection.Play,
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
                else if (_currentSection == NavSection.InputMode)
                {
                    vm.ToggleInputModeCommand.Execute(null);
                }
                else if (_currentSection == NavSection.Controls)
                {
                    CycleCarousel(false);
                }
                break;

            case UIInput.Right:
                if (_currentSection == NavSection.Level)
                {
                    vm.IncreaseLevelCommand.Execute(null);
                }
                else if (_currentSection == NavSection.InputMode)
                {
                    vm.ToggleInputModeCommand.Execute(null);
                }
                else if (_currentSection == NavSection.Controls)
                {
                    CycleCarousel(true);
                }
                break;
        }
    }
}
