using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SkyDrop.Models;

namespace SkyDrop.ViewModels;

/// <summary>
/// View model for the Create Post mode options screen.
/// </summary>
public partial class CreatePostModeOptionsViewModel : GameModeOptionsViewModelBase
{
    [ObservableProperty]
    private int _selectedLevel;

    [ObservableProperty]
    private TextInputMode _selectedInputMode = TextInputMode.T9;

    /// <summary>
    /// Available levels (0-19).
    /// </summary>
    public int[] AvailableLevels { get; } = Enumerable.Range(0, 20).ToArray();

    /// <summary>
    /// Display name for the currently selected input mode.
    /// </summary>
    public string SelectedInputModeDisplay => SelectedInputMode switch
    {
        TextInputMode.T9 => "T9",
        TextInputMode.ABC => "ABC",
        TextInputMode.Kana => "かな",
        _ => "T9"
    };

    /// <inheritdoc/>
    public override GameMode Mode => GameMode.CreatePost;

    /// <inheritdoc/>
    public override IGameModeOptions GetOptions() => new CreatePostModeOptions(SelectedLevel, SelectedInputMode);

    partial void OnSelectedInputModeChanged(TextInputMode value)
    {
        OnPropertyChanged(nameof(SelectedInputModeDisplay));
    }

    partial void OnSelectedLevelChanging(int value)
    {
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

    [RelayCommand]
    private void ToggleInputMode()
    {
        SelectedInputMode = SelectedInputMode switch
        {
            TextInputMode.T9 => TextInputMode.ABC,
            TextInputMode.ABC => TextInputMode.Kana,
            TextInputMode.Kana => TextInputMode.T9,
            _ => TextInputMode.T9
        };
    }
}
