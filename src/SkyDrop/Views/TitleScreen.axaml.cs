using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using Microsoft.Extensions.DependencyInjection;
using SkyDrop.Models;
using SkyDrop.Services;
using SkyDrop.ViewModels;

namespace SkyDrop.Views;

public partial class TitleScreen : UserControl
{
    private readonly IInputService _inputService;
    private Border? _loginButtonBorder;
    private Border? _creditsButtonBorder;
    private Border? _languageButtonBorder;
    private Border? _themeButtonBorder;
    private ListBox? _modeListBox;

    public TitleScreen()
    {
        InitializeComponent();
        _inputService = App.Services.GetRequiredService<IInputService>();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        _loginButtonBorder = this.FindControl<Border>("LoginButtonBorder");
        _creditsButtonBorder = this.FindControl<Border>("CreditsButtonBorder");
        _languageButtonBorder = this.FindControl<Border>("LanguageButtonBorder");
        _themeButtonBorder = this.FindControl<Border>("ThemeButtonBorder");
        _modeListBox = this.FindControl<ListBox>("ModeListBox");

        if (DataContext is TitleScreenViewModel vm)
        {
            vm.PropertyChanged += OnViewModelPropertyChanged;
            UpdateSelectionVisuals(vm);
        }

        Focus();
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);

        if (DataContext is TitleScreenViewModel vm)
        {
            vm.PropertyChanged -= OnViewModelPropertyChanged;
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

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is TitleScreenViewModel vm &&
            (e.PropertyName == nameof(vm.CurrentSection) ||
             e.PropertyName == nameof(vm.SelectedTopBarButton) ||
             e.PropertyName == nameof(vm.SelectedModeIndex)))
        {
            UpdateSelectionVisuals(vm);
        }
    }

    private void UpdateSelectionVisuals(TitleScreenViewModel vm)
    {
        // Update top bar button selection
        bool isTopBarSelected = vm.CurrentSection == TitleScreenSection.TopBar;
        var accentBrush = this.FindResource("AccentCyanBrush") as Avalonia.Media.IBrush;
        var transparentBrush = Avalonia.Media.Brushes.Transparent;

        if (_loginButtonBorder != null)
        {
            _loginButtonBorder.BorderBrush = isTopBarSelected && vm.SelectedTopBarButton == TopBarButton.Login
                ? accentBrush
                : transparentBrush;
        }

        if (_creditsButtonBorder != null)
        {
            _creditsButtonBorder.BorderBrush = isTopBarSelected && vm.SelectedTopBarButton == TopBarButton.Credits
                ? accentBrush
                : transparentBrush;
        }

        if (_languageButtonBorder != null)
        {
            _languageButtonBorder.BorderBrush = isTopBarSelected && vm.SelectedTopBarButton == TopBarButton.Language
                ? accentBrush
                : transparentBrush;
        }

        if (_themeButtonBorder != null)
        {
            _themeButtonBorder.BorderBrush = isTopBarSelected && vm.SelectedTopBarButton == TopBarButton.Theme
                ? accentBrush
                : transparentBrush;
        }

        // Update mode list selection visual
        if (_modeListBox != null)
        {
            // Find all ModeBorder elements and update their selection state
            var listBoxItems = _modeListBox.GetVisualDescendants().OfType<ListBoxItem>().ToList();
            for (int i = 0; i < listBoxItems.Count; i++)
            {
                var border = listBoxItems[i].GetVisualDescendants().OfType<Border>()
                    .FirstOrDefault(b => b.Name == "ModeBorder");

                if (border != null)
                {
                    bool isSelected = !isTopBarSelected && i == vm.SelectedModeIndex;
                    border.BorderBrush = isSelected
                        ? this.FindResource("AccentCyanBrush") as Avalonia.Media.IBrush
                        : this.FindResource("BorderBrush") as Avalonia.Media.IBrush;
                }
            }
        }
    }

    private void OnUIInputReceived(object? sender, UIInputEventArgs e)
    {
        if (DataContext is not TitleScreenViewModel vm) return;

        switch (e.Input)
        {
            case UIInput.Confirm:
                vm.ActivateSelectionCommand.Execute(null);
                break;

            case UIInput.Up:
                vm.MoveUpCommand.Execute(null);
                break;

            case UIInput.Down:
                vm.MoveDownCommand.Execute(null);
                break;

            case UIInput.Left:
                vm.MoveLeftCommand.Execute(null);
                break;

            case UIInput.Right:
                vm.MoveRightCommand.Execute(null);
                break;
        }
    }
}
