using Avalonia.Controls;
using Avalonia.Input;
using Microsoft.Extensions.DependencyInjection;
using SkyDrop.Services;
using SkyDrop.ViewModels;

namespace SkyDrop.Views;

public partial class MainView : UserControl
{
    private readonly IInputService _inputService;

    public MainView()
    {
        InitializeComponent();
        _inputService = App.Services.GetRequiredService<IInputService>();
        DataContext = App.Services.GetRequiredService<MainWindowViewModel>();
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _inputService.ProcessKeyDown(e.Key);
    }

    protected override void OnKeyUp(KeyEventArgs e)
    {
        base.OnKeyUp(e);
        _inputService.ProcessKeyUp(e.Key);
    }
}