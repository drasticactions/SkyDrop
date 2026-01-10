using System.Collections;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Data.Converters;
using Avalonia.Interactivity;
using Avalonia.Media;
using Microsoft.Extensions.DependencyInjection;
using SkyDrop.Models;
using SkyDrop.Resources;
using SkyDrop.Services;
using SkyDrop.ViewModels;

namespace SkyDrop.Views;

/// <summary>
/// Converter to get 1-based index display for list items.
/// </summary>
public class IndexConverter : IValueConverter
{
    public static readonly IndexConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is IList list && parameter is string item)
        {
            var index = list.IndexOf(item);
            return (index + 1).ToString();
        }
        return "?";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converter to get the index of an item in a list.
/// </summary>
public class IndexOfConverter : IValueConverter
{
    public static readonly IndexOfConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is IList list && parameter is string item)
        {
            return list.IndexOf(item);
        }
        return -1;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Multi-value converter to get the index of an item in a list.
/// Values[0] = the list (IList)
/// Values[1] = the item
/// Returns the 0-based index of the item in the list.
/// </summary>
public class IndexOfMultiConverter : IMultiValueConverter
{
    public static readonly IndexOfMultiConverter Instance = new();

    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count >= 2 && values[0] is IList list && values[1] is not null)
        {
            return list.IndexOf(values[1]);
        }
        return -1;
    }
}

/// <summary>
/// Multi-value converter to get the 1-based index display for list items.
/// Values[0] = the list (IList)
/// Values[1] = the item
/// Returns "(index + 1)" as a string.
/// </summary>
public class IndexDisplayMultiConverter : IMultiValueConverter
{
    public static readonly IndexDisplayMultiConverter Instance = new();

    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count >= 2 && values[0] is IList list && values[1] is not null)
        {
            var index = list.IndexOf(values[1]);
            return (index + 1).ToString();
        }
        return "?";
    }
}

/// <summary>
/// Converter for "Editing post #X" label.
/// </summary>
public class EditingLabelConverter : IValueConverter
{
    public static readonly EditingLabelConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isEditing && isEditing)
        {
            return Strings.EditingPost;
        }
        return "";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converter for Add/Update button text.
/// </summary>
public class AddUpdateButtonConverter : IValueConverter
{
    public static readonly AddUpdateButtonConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isEditing && isEditing)
        {
            return Strings.UpdatePost;
        }
        return Strings.AddToQueue;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converter for character count color (red when over limit).
/// </summary>
public class BoolToColorConverter : IValueConverter
{
    public static readonly BoolToColorConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isOver && isOver)
        {
            return Brushes.Red;
        }
        return Brushes.Gray;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public partial class CreatePostModeOptionsView : UserControl
{
    private readonly IInputService _inputService;

    private enum NavSection { Variant, Level, InputMode, Controls, QueuedPosts, PostEditor, Play, Back }

    private NavSection _currentSection = NavSection.Variant;
    private int _carouselIndex = 0;
    private const int CarouselPageCount = 4; // Controls, T9, ABC, Kana

    private ScrollViewer? _mainScrollViewer;
    private Border? _variantSelectorBorder;
    private Border? _standardVariantButton;
    private Border? _queuedVariantButton;
    private Border? _levelSelectorBorder;
    private Border? _inputModeSelectorBorder;
    private Border? _controlsCarouselBorder;
    private Border? _playButtonBorder;
    private Border? _backButtonBorder;

    private IBrush? _accentBrush;
    private IBrush? _buttonBrush;

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

    // Post editor
    private TextBox? _postEditorTextBox;

    public CreatePostModeOptionsView()
    {
        InitializeComponent();
        _inputService = App.Services.GetRequiredService<IInputService>();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (DataContext is CreatePostModeOptionsViewModel vm)
        {
            vm.PropertyChanged += (_, args) =>
            {
                if (args.PropertyName == nameof(CreatePostModeOptionsViewModel.SelectedVariant))
                {
                    UpdateVariantVisuals();
                }
            };
        }
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        _mainScrollViewer = this.FindControl<ScrollViewer>("MainScrollViewer");
        _variantSelectorBorder = this.FindControl<Border>("VariantSelectorBorder");
        _standardVariantButton = this.FindControl<Border>("StandardVariantButton");
        _queuedVariantButton = this.FindControl<Border>("QueuedVariantButton");
        _levelSelectorBorder = this.FindControl<Border>("LevelSelectorBorder");
        _inputModeSelectorBorder = this.FindControl<Border>("InputModeSelectorBorder");
        _controlsCarouselBorder = this.FindControl<Border>("ControlsCarouselBorder");
        _playButtonBorder = this.FindControl<Border>("PlayButtonBorder");
        _backButtonBorder = this.FindControl<Border>("BackButtonBorder");

        _accentBrush = this.FindResource("AccentCyanBrush") as IBrush;
        _buttonBrush = this.FindResource("ButtonBackgroundBrush") as IBrush;

        // Wire up click handlers for variant buttons
        if (_standardVariantButton != null)
        {
            _standardVariantButton.PointerPressed += (_, _) =>
            {
                if (DataContext is CreatePostModeOptionsViewModel vm)
                    vm.SelectStandardVariantCommand.Execute(null);
            };
        }

        if (_queuedVariantButton != null)
        {
            _queuedVariantButton.PointerPressed += (_, _) =>
            {
                if (DataContext is CreatePostModeOptionsViewModel vm)
                    vm.SelectQueuedVariantCommand.Execute(null);
            };
        }

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

        // Post editor
        _postEditorTextBox = this.FindControl<TextBox>("PostEditorTextBox");

        if (_carouselLeftButton != null)
        {
            _carouselLeftButton.Click += (_, _) => CycleCarousel(false);
        }

        if (_carouselRightButton != null)
        {
            _carouselRightButton.Click += (_, _) => CycleCarousel(true);
        }

        UpdateSelectionVisuals();
        UpdateVariantVisuals();
        UpdateCarouselVisuals();
        Focus();
    }

    private void UpdateVariantVisuals()
    {
        if (DataContext is not CreatePostModeOptionsViewModel vm) return;

        var selectedBrush = _accentBrush ?? Brushes.Cyan;
        var normalBrush = _buttonBrush ?? Brushes.DarkGray;

        if (_standardVariantButton != null)
        {
            _standardVariantButton.Background = vm.IsStandardVariant ? selectedBrush : normalBrush;
        }

        if (_queuedVariantButton != null)
        {
            _queuedVariantButton.Background = vm.IsQueuedVariant ? selectedBrush : normalBrush;
        }
    }

    private void UpdateSelectionVisuals()
    {
        var accentBrush = this.FindResource("AccentCyanBrush") as IBrush;
        var transparentBrush = Brushes.Transparent;

        if (_variantSelectorBorder != null)
        {
            _variantSelectorBorder.BorderBrush = _currentSection == NavSection.Variant ? accentBrush : transparentBrush;
        }

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

        // Handle focus for post editor in queued mode
        if (_currentSection == NavSection.PostEditor && _postEditorTextBox != null)
        {
            _postEditorTextBox.Focus();
        }

        // Scroll to make selected section visible
        ScrollToCurrentSection();
    }

    private void ScrollToCurrentSection()
    {
        Control? targetControl = _currentSection switch
        {
            NavSection.Variant => _variantSelectorBorder,
            NavSection.Level => _levelSelectorBorder,
            NavSection.InputMode => _inputModeSelectorBorder,
            NavSection.Controls => _controlsCarouselBorder,
            NavSection.Play => _playButtonBorder,
            NavSection.Back => _backButtonBorder,
            NavSection.PostEditor => _postEditorTextBox,
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

    private NavSection GetNextSectionDown(CreatePostModeOptionsViewModel vm)
    {
        if (vm.IsStandardVariant)
        {
            return _currentSection switch
            {
                NavSection.Variant => NavSection.Level,
                NavSection.Level => NavSection.InputMode,
                NavSection.InputMode => NavSection.Controls,
                NavSection.Controls => NavSection.Play,
                NavSection.Play => NavSection.Back,
                _ => _currentSection
            };
        }
        else // Queued variant
        {
            return _currentSection switch
            {
                NavSection.Variant => NavSection.Level,
                NavSection.Level => NavSection.PostEditor,
                NavSection.QueuedPosts => NavSection.PostEditor,
                NavSection.PostEditor => NavSection.Play,
                NavSection.Play => NavSection.Back,
                _ => _currentSection
            };
        }
    }

    private NavSection GetNextSectionUp(CreatePostModeOptionsViewModel vm)
    {
        if (vm.IsStandardVariant)
        {
            return _currentSection switch
            {
                NavSection.Level => NavSection.Variant,
                NavSection.InputMode => NavSection.Level,
                NavSection.Controls => NavSection.InputMode,
                NavSection.Play => NavSection.Controls,
                NavSection.Back => NavSection.Play,
                _ => _currentSection
            };
        }
        else // Queued variant
        {
            return _currentSection switch
            {
                NavSection.Level => NavSection.Variant,
                NavSection.QueuedPosts => NavSection.Level,
                NavSection.PostEditor => NavSection.Level,
                NavSection.Play => NavSection.PostEditor,
                NavSection.Back => NavSection.Play,
                _ => _currentSection
            };
        }
    }

    private void OnUIInputReceived(object? sender, UIInputEventArgs e)
    {
        if (DataContext is not CreatePostModeOptionsViewModel vm) return;

        // Don't handle navigation when text box is focused (let it handle its own input)
        if (_postEditorTextBox?.IsFocused == true && _currentSection == NavSection.PostEditor)
        {
            // Only handle escape to exit text editing
            if (e.Input == UIInput.Cancel)
            {
                _currentSection = NavSection.Play;
                UpdateSelectionVisuals();
                Focus(); // Return focus to the control
            }
            return;
        }

        switch (e.Input)
        {
            case UIInput.Confirm:
                switch (_currentSection)
                {
                    case NavSection.Variant:
                        vm.ToggleVariantCommand.Execute(null);
                        // Adjust current section if needed after variant change
                        if (vm.IsQueuedVariant && (_currentSection == NavSection.InputMode || _currentSection == NavSection.Controls))
                        {
                            _currentSection = NavSection.PostEditor;
                        }
                        UpdateSelectionVisuals();
                        break;
                    case NavSection.Level:
                    case NavSection.Play:
                        vm.PlayCommand.Execute(null);
                        break;
                    case NavSection.InputMode:
                        vm.ToggleInputModeCommand.Execute(null);
                        break;
                    case NavSection.Controls:
                        CycleCarousel(true);
                        break;
                    case NavSection.PostEditor:
                        // Focus the text box for editing
                        _postEditorTextBox?.Focus();
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
                _currentSection = GetNextSectionUp(vm);
                UpdateSelectionVisuals();
                break;

            case UIInput.Down:
                _currentSection = GetNextSectionDown(vm);
                UpdateSelectionVisuals();
                break;

            case UIInput.Left:
                if (_currentSection == NavSection.Level)
                {
                    vm.DecreaseLevelCommand.Execute(null);
                }
                else if (_currentSection == NavSection.Variant)
                {
                    vm.SelectStandardVariantCommand.Execute(null);
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
                else if (_currentSection == NavSection.Variant)
                {
                    vm.SelectQueuedVariantCommand.Execute(null);
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
