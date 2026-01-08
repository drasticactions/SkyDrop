using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SkyDrop.Models;

namespace SkyDrop.ViewModels;

/// <summary>
/// ViewModel for Normal mode options screen.
/// </summary>
public partial class NormalModeOptionsViewModel : GameModeOptionsViewModelBase
{
    [ObservableProperty]
    private int _selectedLevel;

    /// <summary>
    /// Available levels (0-19).
    /// </summary>
    public int[] AvailableLevels { get; } = Enumerable.Range(0, 20).ToArray();

    /// <inheritdoc/>
    public override GameMode Mode => GameMode.Normal;

    /// <inheritdoc/>
    public override IGameModeOptions GetOptions() => new NormalModeOptions(SelectedLevel);

    partial void OnSelectedLevelChanging(int value)
    {
        // Clamp the value between 0 and 19
        if (value < 0) _selectedLevel = 0;
        else if (value > 19) _selectedLevel = 19;
    }

    [RelayCommand]
    private void Play()
    {
        RequestStartGame();
    }

    [RelayCommand]
    private void Back()
    {
        RequestBack();
    }

    [RelayCommand]
    private void IncreaseLevel()
    {
        if (SelectedLevel < 19)
            SelectedLevel++;
    }

    [RelayCommand]
    private void DecreaseLevel()
    {
        if (SelectedLevel > 0)
            SelectedLevel--;
    }
}
