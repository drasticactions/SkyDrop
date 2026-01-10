using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DaT9;
using SkyDrop.Resources;

namespace SkyDrop.ViewModels;

/// <summary>
/// ViewModel for the T9 Generator utility.
/// Provides bidirectional T9 conversion: Text to T9 and T9 to Text.
/// </summary>
public partial class T9GeneratorViewModel : ViewModelBase
{
    private readonly T9Engine _engine;

    public T9GeneratorViewModel()
    {
        _engine = new T9Engine();
        LoadDictionary();
    }

    /// <summary>
    /// Event raised when the user requests to go back.
    /// </summary>
    public event Action? BackRequested;

    // === Mode Selection ===
    [ObservableProperty]
    private T9Mode _currentMode = T9Mode.TextToT9;

    public bool IsTextToT9Mode => CurrentMode == T9Mode.TextToT9;
    public bool IsT9ToTextMode => CurrentMode == T9Mode.T9ToText;

    // === Text to T9 Mode ===
    [ObservableProperty]
    private string _inputText = string.Empty;

    [ObservableProperty]
    private string _t9Output = string.Empty;

    [ObservableProperty]
    private ObservableCollection<SequenceStepDisplay> _sequenceSteps = new();

    // === T9 to Text Mode ===
    [ObservableProperty]
    private string _t9Input = string.Empty;

    [ObservableProperty]
    private ObservableCollection<string> _possibleWords = new();

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    private void LoadDictionary()
    {
        var uri = new Uri("avares://SkyDrop/Assets/en-utf8.csv");
        using var stream = Avalonia.Platform.AssetLoader.Open(uri);
        _engine.LoadDictionaryFromStream(stream);
    }

    partial void OnCurrentModeChanged(T9Mode value)
    {
        OnPropertyChanged(nameof(IsTextToT9Mode));
        OnPropertyChanged(nameof(IsT9ToTextMode));
    }

    partial void OnInputTextChanged(string value)
    {
        if (CurrentMode == T9Mode.TextToT9)
        {
            ConvertTextToT9();
        }
    }

    partial void OnT9InputChanged(string value)
    {
        if (CurrentMode == T9Mode.T9ToText)
        {
            ConvertT9ToText();
        }
    }

    [RelayCommand]
    private void ConvertTextToT9()
    {
        ErrorMessage = string.Empty;
        SequenceSteps.Clear();

        if (string.IsNullOrWhiteSpace(InputText))
        {
            T9Output = string.Empty;
            return;
        }

        try
        {
            var result = T9Engine.TextToSequence(InputText);
            T9Output = result.Sequence;

            foreach (var step in result.Steps)
            {
                SequenceSteps.Add(new SequenceStepDisplay(step.Word, step.Sequence));
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error: {ex.Message}";
            T9Output = string.Empty;
        }
    }

    [RelayCommand]
    private void ConvertT9ToText()
    {
        ErrorMessage = string.Empty;
        PossibleWords.Clear();

        if (string.IsNullOrWhiteSpace(T9Input))
        {
            return;
        }

        if (!T9Input.All(c => char.IsDigit(c)))
        {
            ErrorMessage = Strings.T9InputDigitsOnly;
            return;
        }

        var wordSequence = new string(T9Input.Where(c => c >= '2' && c <= '9').ToArray());

        if (string.IsNullOrEmpty(wordSequence))
        {
            ErrorMessage = Strings.T9EnterDigitsHint;
            return;
        }

        var completions = _engine.GetAllCompletions(wordSequence);

        if (completions.Count == 0)
        {
            ErrorMessage = Strings.NoMatchingWords;
            return;
        }

        foreach (var word in completions.Take(30))
        {
            PossibleWords.Add(word);
        }

        if (completions.Count > 30)
        {
            ErrorMessage = string.Format(Strings.T9ShowingMatches, completions.Count);
        }
    }

    [RelayCommand]
    private void SwitchMode()
    {
        CurrentMode = CurrentMode == T9Mode.TextToT9
            ? T9Mode.T9ToText
            : T9Mode.TextToT9;
    }

    [RelayCommand]
    private void ClearAll()
    {
        InputText = string.Empty;
        T9Output = string.Empty;
        SequenceSteps.Clear();
        T9Input = string.Empty;
        PossibleWords.Clear();
        ErrorMessage = string.Empty;
    }

    [RelayCommand]
    private void Back()
    {
        BackRequested?.Invoke();
    }
}

/// <summary>
/// Display model for a T9 sequence step showing word and its T9 sequence.
/// </summary>
public record SequenceStepDisplay(string Word, string Sequence);
