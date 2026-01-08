using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Microsoft.Extensions.DependencyInjection;
using SkyDrop.Controls.Bluesky;
using SkyDrop.Models;
using SkyDrop.Services;
using SkyDrop.ViewModels;

namespace SkyDrop.Views;

public partial class DiscoverFeedModeOptionsView : UserControl
{
    private readonly IInputService _inputService;
    private enum NavSection { GameType, Level, Feed, Play, Back }

    private NavSection _currentSection = NavSection.GameType;
    private Border? _gameTypeSelectorBorder;
    private Border? _revealModeButton;
    private Border? _scrollModeButton;
    private Border? _levelSelectorBorder;
    private Border? _feedSelectorBorder;
    private Border? _playButtonBorder;
    private Border? _backButtonBorder;
    private FeedSelectorView? _feedSelectorView;

    private IBrush? _accentBrush;
    private IBrush? _buttonBrush;

    public DiscoverFeedModeOptionsView()
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
        _feedSelectorBorder = this.FindControl<Border>("FeedSelectorBorder");
        _playButtonBorder = this.FindControl<Border>("PlayButtonBorder");
        _backButtonBorder = this.FindControl<Border>("BackButtonBorder");
        _feedSelectorView = this.FindControl<FeedSelectorView>("FeedSelectorView");

        _accentBrush = this.FindResource("AccentCyanBrush") as IBrush;
        _buttonBrush = this.FindResource("ButtonBackgroundBrush") as IBrush;

        // Wire up click handlers for game type buttons
        if (_revealModeButton != null)
        {
            _revealModeButton.PointerPressed += (_, _) =>
            {
                if (DataContext is DiscoverFeedModeOptionsViewModel vm)
                    vm.SelectRevealModeCommand.Execute(null);
            };
        }

        if (_scrollModeButton != null)
        {
            _scrollModeButton.PointerPressed += (_, _) =>
            {
                if (DataContext is DiscoverFeedModeOptionsViewModel vm)
                    vm.SelectScrollModeCommand.Execute(null);
            };
        }

        UpdateSelectionVisuals();
        UpdateGameTypeVisuals();
        Focus();
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (DataContext is DiscoverFeedModeOptionsViewModel vm)
        {
            vm.PropertyChanged += (_, args) =>
            {
                if (args.PropertyName == nameof(DiscoverFeedModeOptionsViewModel.SelectedGameType))
                {
                    UpdateGameTypeVisuals();
                }
            };
        }
    }

    private void UpdateGameTypeVisuals()
    {
        if (DataContext is not DiscoverFeedModeOptionsViewModel vm) return;

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

        if (_feedSelectorBorder != null)
        {
            _feedSelectorBorder.BorderBrush = _currentSection == NavSection.Feed ? accentBrush : transparentBrush;
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
            NavSection.GameType => _gameTypeSelectorBorder,
            NavSection.Level => _levelSelectorBorder,
            NavSection.Feed => _feedSelectorBorder,
            NavSection.Play => _playButtonBorder,
            NavSection.Back => _backButtonBorder,
            _ => null
        };

        // Use BringIntoView to scroll the target into view
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
        if (DataContext is not DiscoverFeedModeOptionsViewModel vm) return;

        // If FeedSelector search TextBox is focused, handle special cases
        if (_feedSelectorView?.IsSearchTextBoxFocused == true)
        {
            if (e.Input == UIInput.Cancel)
            {
                // Escape unfocuses the TextBox and returns to navigation
                _feedSelectorView.UnfocusSearchTextBox();
                Focus();
            }
            else if (e.Input == UIInput.Confirm)
            {
                // Enter triggers search
                vm.FeedSelector.SearchFeedsCommand.Execute(null);
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
                HandleUp(vm);
                break;

            case UIInput.Down:
                HandleDown(vm);
                break;

            case UIInput.Left:
                HandleLeft(vm);
                break;

            case UIInput.Right:
                HandleRight(vm);
                break;
        }
    }

    private void HandleConfirm(DiscoverFeedModeOptionsViewModel vm)
    {
        switch (_currentSection)
        {
            case NavSection.GameType:
                vm.ToggleGameTypeCommand.Execute(null);
                break;
            case NavSection.Level:
                vm.PlayCommand.Execute(null);
                break;
            case NavSection.Feed:
                if (vm.FeedSelector.IsSearchBarHighlighted)
                {
                    // Focus the search text box
                    _feedSelectorView?.FocusSearchTextBox();
                }
                else if (vm.FeedSelector.IsLoadMoreHighlighted)
                {
                    // Trigger load more
                    vm.FeedSelector.LoadMoreFeedsCommand.Execute(null);
                }
                else if (vm.FeedSelector.HighlightedIndex >= 0)
                {
                    // Select the highlighted feed
                    vm.FeedSelector.SelectHighlightedCommand.Execute(null);
                }
                break;
            case NavSection.Play:
                vm.PlayCommand.Execute(null);
                break;
            case NavSection.Back:
                vm.BackCommand.Execute(null);
                break;
        }
    }

    private void HandleUp(DiscoverFeedModeOptionsViewModel vm)
    {
        // If in Feed section, navigate within feed list
        if (_currentSection == NavSection.Feed)
        {
            if (vm.FeedSelector.IsSearchBarHighlighted)
            {
                // Already at top (search bar), exit feed navigation
                vm.FeedSelector.ClearHighlight();
                _currentSection = NavSection.Level;
                UpdateSelectionVisuals();
            }
            else
            {
                // Move up within feed list (will go to search bar if at top)
                vm.FeedSelector.MoveHighlightUpCommand.Execute(null);
            }
        }
        else
        {
            _currentSection = _currentSection switch
            {
                NavSection.Level => NavSection.GameType,
                NavSection.Play => NavSection.Feed,
                NavSection.Back => NavSection.Play,
                _ => _currentSection
            };

            // When entering Feed section from below, start at Load More if available, otherwise last item
            if (_currentSection == NavSection.Feed)
            {
                if (vm.FeedSelector.HasMoreItems)
                {
                    vm.FeedSelector.IsLoadMoreHighlighted = true;
                }
                else if (vm.FeedSelector.Generators.Count > 0)
                {
                    vm.FeedSelector.HighlightedIndex = vm.FeedSelector.Generators.Count - 1;
                }
                else
                {
                    vm.FeedSelector.IsSearchBarHighlighted = true;
                }
            }

            UpdateSelectionVisuals();
        }
    }

    private void HandleDown(DiscoverFeedModeOptionsViewModel vm)
    {
        // If in Feed section, navigate within feed list
        if (_currentSection == NavSection.Feed)
        {
            if (vm.FeedSelector.IsAtBottom)
            {
                // At bottom, exit feed navigation
                vm.FeedSelector.ClearHighlight();
                _currentSection = NavSection.Play;
                UpdateSelectionVisuals();
            }
            else
            {
                // Move down within feed selector (search bar -> feeds -> load more)
                vm.FeedSelector.MoveHighlightDownCommand.Execute(null);
            }
        }
        else
        {
            _currentSection = _currentSection switch
            {
                NavSection.GameType => NavSection.Level,
                NavSection.Level => NavSection.Feed,
                NavSection.Play => NavSection.Back,
                _ => _currentSection
            };

            // When entering Feed section from above, start at search bar
            if (_currentSection == NavSection.Feed)
            {
                vm.FeedSelector.ResetHighlight();
            }

            UpdateSelectionVisuals();
        }
    }

    private void HandleLeft(DiscoverFeedModeOptionsViewModel vm)
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

    private void HandleRight(DiscoverFeedModeOptionsViewModel vm)
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
