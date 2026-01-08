using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Interactivity;
using Avalonia.Media;
using SkyDrop.ViewModels;

namespace SkyDrop.Controls.Bluesky;

public partial class FeedSelectorView : UserControl
{
    private TextBox? _searchTextBox;
    private Border? _searchBarBorder;
    private Border? _loadMoreBorder;
    private ItemsControl? _feedItemsControl;
    private ScrollViewer? _feedScrollViewer;

    private IBrush? _accentBrush;

    public FeedSelectorView()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    /// <summary>
    /// Gets whether the search TextBox is currently focused.
    /// </summary>
    public bool IsSearchTextBoxFocused => _searchTextBox?.IsFocused == true;

    /// <summary>
    /// Focuses the search TextBox.
    /// </summary>
    public void FocusSearchTextBox()
    {
        _searchTextBox?.Focus();
    }

    /// <summary>
    /// Removes focus from the search TextBox.
    /// </summary>
    public void UnfocusSearchTextBox()
    {
        Focus();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        _searchTextBox = this.FindControl<TextBox>("SearchTextBox");
        _searchBarBorder = this.FindControl<Border>("SearchBarBorder");
        _loadMoreBorder = this.FindControl<Border>("LoadMoreBorder");
        _feedItemsControl = this.FindControl<ItemsControl>("FeedItemsControl");
        _feedScrollViewer = this.FindControl<ScrollViewer>("FeedScrollViewer");

        _accentBrush = this.FindResource("AccentCyanBrush") as IBrush;

        UpdateHighlightVisuals();

        // Auto-load feeds when the control is loaded
        if (DataContext is FeedSelectorViewModel vm && vm.Generators.Count == 0 && !vm.IsLoading)
        {
            _ = vm.LoadFeedsCommand.ExecuteAsync(null);
        }
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (DataContext is FeedSelectorViewModel vm)
        {
            vm.PropertyChanged += (_, args) =>
            {
                if (args.PropertyName == nameof(FeedSelectorViewModel.IsSearchBarHighlighted) ||
                    args.PropertyName == nameof(FeedSelectorViewModel.IsLoadMoreHighlighted) ||
                    args.PropertyName == nameof(FeedSelectorViewModel.HighlightedIndex))
                {
                    UpdateHighlightVisuals();
                }
            };
        }
    }

    private void UpdateHighlightVisuals()
    {
        if (DataContext is not FeedSelectorViewModel vm) return;

        var accentBrush = _accentBrush ?? Brushes.Cyan;
        var transparentBrush = Brushes.Transparent;

        if (_searchBarBorder != null)
        {
            _searchBarBorder.BorderBrush = vm.IsSearchBarHighlighted ? accentBrush : transparentBrush;
        }

        if (_loadMoreBorder != null)
        {
            _loadMoreBorder.BorderBrush = vm.IsLoadMoreHighlighted ? accentBrush : transparentBrush;
        }

        UpdateFeedItemHighlights(vm.HighlightedIndex, accentBrush, transparentBrush);
    }

    private void UpdateFeedItemHighlights(int highlightedIndex, IBrush accentBrush, IBrush transparentBrush)
    {
        if (_feedItemsControl == null) return;

        for (int i = 0; i < _feedItemsControl.ItemCount; i++)
        {
            var container = _feedItemsControl.ContainerFromIndex(i);
            if (container is ContentPresenter presenter)
            {
                var border = FindChildBorder(presenter);
                if (border != null)
                {
                    border.Background = i == highlightedIndex
                        ? (this.FindResource("ButtonBackgroundBrush") as IBrush ?? Brushes.DarkGray)
                        : transparentBrush;
                }
            }
        }

        // Scroll to highlighted item
        if (highlightedIndex >= 0 && _feedScrollViewer != null)
        {
            var container = _feedItemsControl.ContainerFromIndex(highlightedIndex);
            if (container is Control control)
            {
                control.BringIntoView();
            }
        }
    }

    private static Border? FindChildBorder(ContentPresenter presenter)
    {
        if (presenter.Child is Border border)
        {
            return border;
        }

        return null;
    }
}
