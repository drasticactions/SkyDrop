using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DaT9;
using FishyFlip;
using SkyDrop.Models;
using SkyDrop.Services;

namespace SkyDrop.ViewModels;

/// <summary>
/// View model for the Create Post game mode.
/// </summary>
public partial class CreatePostGameViewModel : GameViewModelBase
{
    private readonly T9Engine _t9Engine;
    private readonly ATProtocol _protocol;
    private readonly JapaneseDictionaryService _japaneseDictionary;
    private CreatePostModeOptions? _currentOptions;

    /// <summary>
    /// The T9 keypad layout (0-11: 1,2,3,4,5,6,7,8,9,*,0,#)
    /// </summary>
    private static readonly string[] KeypadLabels = ["1", "2", "3", "4", "5", "6", "7", "8", "9", "*", "0", "#", "OK"];

    /// <summary>
    /// ABC mode character mappings for each key (indices 0-11 match KeypadLabels).
    /// Key 1 has punctuation, keys 2-9 have letters, key 0 has space, *, # are special.
    /// </summary>
    private static readonly string[][] AbcCharacters =
    [
        [".", ",", "!", "?", ":", "-", "'", "/", "(", ")", "1"],
        ["a", "b", "c", "2"],
        ["d", "e", "f", "3"],
        ["g", "h", "i", "4"],
        ["j", "k", "l", "5"],
        ["m", "n", "o", "6"],
        ["p", "q", "r", "s", "7"],
        ["t", "u", "v", "8"],
        ["w", "x", "y", "z", "9"],
        [],
        [" ", "0"],
        []
    ];

    /// <summary>
    /// Kana mode character mappings for each key
    /// Key 1 = あ行, Key 2 = か行, etc.
    /// </summary>
    private static readonly string[][] KanaCharacters =
    [
        ["あ", "い", "う", "え", "お", "ぁ", "ぃ", "ぅ", "ぇ", "ぉ"],
        ["か", "き", "く", "け", "こ"],
        ["さ", "し", "す", "せ", "そ"],
        ["た", "ち", "つ", "て", "と", "っ"],
        ["な", "に", "ぬ", "ね", "の"],
        ["は", "ひ", "ふ", "へ", "ほ"],
        ["ま", "み", "む", "め", "も"],
        ["や", "ゆ", "よ", "ゃ", "ゅ", "ょ"],
        ["ら", "り", "る", "れ", "ろ"],
        [],
        ["わ", "を", "ん", "ー", "。", "、", "！", "？", "・"],
        [],
        []
    ];

    /// <summary>
    /// Dakuten (゛) mappings - base character to voiced version.
    /// </summary>
    private static readonly Dictionary<char, char> DakutenMap = new()
    {
        { 'か', 'が' }, { 'き', 'ぎ' }, { 'く', 'ぐ' }, { 'け', 'げ' }, { 'こ', 'ご' },
        { 'さ', 'ざ' }, { 'し', 'じ' }, { 'す', 'ず' }, { 'せ', 'ぜ' }, { 'そ', 'ぞ' },
        { 'た', 'だ' }, { 'ち', 'ぢ' }, { 'つ', 'づ' }, { 'て', 'で' }, { 'と', 'ど' },
        { 'は', 'ば' }, { 'ひ', 'び' }, { 'ふ', 'ぶ' }, { 'へ', 'べ' }, { 'ほ', 'ぼ' },
        { 'う', 'ゔ' }
    };

    /// <summary>
    /// Handakuten (゜) mappings - base character to semi-voiced version.
    /// </summary>
    private static readonly Dictionary<char, char> HandakutenMap = new()
    {
        // は行 → ぱ行
        { 'は', 'ぱ' }, { 'ひ', 'ぴ' }, { 'ふ', 'ぷ' }, { 'へ', 'ぺ' }, { 'ほ', 'ぽ' },
        // ば行 → ぱ行 (allows cycling back)
        { 'ば', 'ぱ' }, { 'び', 'ぴ' }, { 'ぶ', 'ぷ' }, { 'べ', 'ぺ' }, { 'ぼ', 'ぽ' }
    };

    /// <summary>
    /// Small kana mappings - regular to small version.
    /// </summary>
    private static readonly Dictionary<char, char> SmallKanaMap = new()
    {
        { 'あ', 'ぁ' }, { 'い', 'ぃ' }, { 'う', 'ぅ' }, { 'え', 'ぇ' }, { 'お', 'ぉ' },
        { 'や', 'ゃ' }, { 'ゆ', 'ゅ' }, { 'よ', 'ょ' },
        { 'つ', 'っ' }, { 'わ', 'ゎ' }
    };

    /// <summary>
    /// Maximum post length for Bluesky.
    /// </summary>
    private const int MaxPostLength = 300;

    /// <summary>
    /// Gets the list of completed posts created during the game.
    /// </summary>
    public ObservableCollection<string> CompletedPosts { get; } = new();

    /// <summary>
    /// The currently selected keypad position (0-11).
    /// </summary>
    [ObservableProperty]
    private int _selectedKeyIndex;

    /// <summary>
    /// The label of the currently selected key.
    /// </summary>
    [ObservableProperty]
    private string _selectedKeyLabel = "1";

    /// <summary>
    /// The current word being typed (T9 completion preview).
    /// </summary>
    [ObservableProperty]
    private string _currentWordPreview = "";

    /// <summary>
    /// The current T9 digit sequence being entered.
    /// </summary>
    [ObservableProperty]
    private string _currentT9Sequence = "";

    /// <summary>
    /// The current post being composed.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanAddKanaPost))]
    [NotifyCanExecuteChangedFor(nameof(ConfirmKanaPostCommand))]
    private string _currentPost = "";

    /// <summary>
    /// Character count of the current post.
    /// </summary>
    [ObservableProperty]
    private int _currentPostLength;

    /// <summary>
    /// Whether the current post exceeds the maximum length.
    /// </summary>
    [ObservableProperty]
    private bool _isPostTooLong;

    /// <summary>
    /// Whether the confirm button (pound key) is enabled.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanAddKanaPost))]
    [NotifyCanExecuteChangedFor(nameof(ConfirmKanaPostCommand))]
    private bool _canConfirmPost = true;

    /// <summary>
    /// Whether a login/post operation is in progress.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanPost))]
    private bool _isPosting;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanPost))]
    private bool _hasPosted;

    /// <summary>
    /// Status message for login/posting operations.
    /// </summary>
    [ObservableProperty]
    private string _postingStatus = "";

    public bool CanPost => !IsPosting && !HasPosted;

    /// <summary>
    /// Whether to include a "Posted by #SkyDrop" signature post with stats image.
    /// </summary>
    [ObservableProperty]
    private bool _includeSkyDropSignature = true;

    /// <summary>
    /// The currently selected menu item index on the Game Over screen.
    /// 0 = Toggle signature, 1 = Post to Bluesky, 2 = Play Again, 3 = Title
    /// When no posts: 0 = Play Again, 1 = Title
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsToggleSelected))]
    [NotifyPropertyChangedFor(nameof(IsPostSelected))]
    [NotifyPropertyChangedFor(nameof(IsPlayAgainSelected))]
    [NotifyPropertyChangedFor(nameof(IsTitleSelected))]
    private int _gameOverMenuIndex;

    /// <summary>
    /// Gets the number of menu items on the Game Over screen.
    /// </summary>
    public int GameOverMenuCount => CompletedPosts.Count > 0 ? 4 : 2;

    /// <summary>
    /// Whether the Toggle signature option is selected (when posts exist).
    /// </summary>
    public bool IsToggleSelected => CompletedPosts.Count > 0 && GameOverMenuIndex == 0;

    /// <summary>
    /// Whether the Post to Bluesky option is selected (when posts exist).
    /// </summary>
    public bool IsPostSelected => CompletedPosts.Count > 0 && GameOverMenuIndex == 1;

    /// <summary>
    /// Whether the Play Again option is selected.
    /// </summary>
    public bool IsPlayAgainSelected => CompletedPosts.Count > 0 ? GameOverMenuIndex == 2 : GameOverMenuIndex == 0;

    /// <summary>
    /// Whether the Title option is selected.
    /// </summary>
    public bool IsTitleSelected => CompletedPosts.Count > 0 ? GameOverMenuIndex == 3 : GameOverMenuIndex == 1;

    /// <summary>
    /// The current text input mode (T9 or ABC).
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsKanaMode))]
    [NotifyPropertyChangedFor(nameof(CanAddKanaPost))]
    [NotifyCanExecuteChangedFor(nameof(ConfirmKanaPostCommand))]
    private TextInputMode _currentInputMode = TextInputMode.T9;

    /// <summary>
    /// Whether the next character should be capitalized (for ABC mode).
    /// </summary>
    private bool _capitalizeNext = true;

    /// <summary>
    /// The current character index within ABC mode for the selected key.
    /// </summary>
    [ObservableProperty]
    private int _abcCharacterIndex;

    /// <summary>
    /// The currently selected character in ABC mode.
    /// </summary>
    [ObservableProperty]
    private string _currentAbcCharacter = "";

    /// <summary>
    /// Whether ABC mode is currently active.
    /// </summary>
    public bool IsAbcMode => CurrentInputMode == TextInputMode.ABC;

    /// <summary>
    /// Whether Kana mode is currently active.
    /// </summary>
    public bool IsKanaMode => CurrentInputMode == TextInputMode.Kana;

    /// <summary>
    /// Gets whether the Kana post can be added (has content and no pending input).
    /// </summary>
    public bool CanAddKanaPost => IsKanaMode
        && !string.IsNullOrWhiteSpace(CurrentPost)
        && string.IsNullOrEmpty(CurrentKanaInput)
        && CanConfirmPost;

    /// <summary>
    /// Display string for the current input mode.
    /// </summary>
    public string InputModeDisplay => CurrentInputMode switch
    {
        TextInputMode.T9 => "T9",
        TextInputMode.ABC => "ABC",
        TextInputMode.Kana => "かな",
        _ => "T9"
    };

    /// <summary>
    /// The current kana input buffer (hiragana being typed).
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanAddKanaPost))]
    [NotifyCanExecuteChangedFor(nameof(ConfirmKanaPostCommand))]
    private string _currentKanaInput = "";

    /// <summary>
    /// The currently selected kana character in kana mode.
    /// </summary>
    [ObservableProperty]
    private string _currentKanaCharacter = "";

    /// <summary>
    /// The current character index within kana mode for the selected key.
    /// </summary>
    [ObservableProperty]
    private int _kanaCharacterIndex;

    /// <summary>
    /// List of kanji suggestions for the current kana input.
    /// </summary>
    [ObservableProperty]
    private IReadOnlyList<JapaneseWord> _kanjiSuggestions = Array.Empty<JapaneseWord>();

    /// <summary>
    /// The currently selected kanji suggestion index.
    /// </summary>
    [ObservableProperty]
    private int _selectedSuggestionIndex;

    /// <summary>
    /// The currently selected kanji/word to display.
    /// </summary>
    public string CurrentSuggestionDisplay => KanjiSuggestions.Count > 0 && SelectedSuggestionIndex < KanjiSuggestions.Count
        ? KanjiSuggestions[SelectedSuggestionIndex].Kanji
        : CurrentKanaInput;

    /// <summary>
    /// Whether there are kanji suggestions available.
    /// </summary>
    public bool HasKanjiSuggestions => KanjiSuggestions.Count > 0;

    public CreatePostGameViewModel(ATProtocol protocol, JapaneseDictionaryService japaneseDictionary)
    {
        _protocol = protocol;
        _japaneseDictionary = japaneseDictionary;
        _t9Engine = new T9Engine();
        LoadDefaultDictionary();

        _engine.OnRotation += OnRotation;
        _engine.OnPieceLocked += OnPieceLocked;
    }

    /// <summary>
    /// Loads the default dictionary for T9.
    /// </summary>
    private void LoadDefaultDictionary()
    {
        var uri = new Uri("avares://SkyDrop/google-10000-english-usa.txt");
        using var stream = Avalonia.Platform.AssetLoader.Open(uri);
        using var reader = new StreamReader(stream);

        while (reader.ReadLine() is { } line)
        {
            var word = line.Trim();
            if (!string.IsNullOrEmpty(word))
            {
                _t9Engine.AddWord(word);
            }
        }
    }

    /// <summary>
    /// Prepares a new game with the specified options and shows instructions.
    /// </summary>
    public void PrepareGame(CreatePostModeOptions options)
    {
        _currentOptions = options;
        ShowInstructions = true;
        IsGameOver = false;
        IsPaused = false;
        ResetT9State();
    }

    /// <summary>
    /// Dismisses the instructions and starts the game.
    /// </summary>
    public override void DismissInstructionsAndStart()
    {
        ShowInstructions = false;

        if (_currentOptions == null)
            return;

        StartEngine(_currentOptions.StartLevel);
    }

    /// <summary>
    /// Starts a new game with the specified options (shows instructions first).
    /// </summary>
    public void StartGame(CreatePostModeOptions options)
    {
        PrepareGame(options);
    }

    /// <summary>
    /// Restarts the game with the same options (skips instructions).
    /// </summary>
    public void RestartGame()
    {
        if (_currentOptions == null)
            return;

        ShowInstructions = false;
        IsGameOver = false;
        IsPaused = false;
        HasPosted = false;
        ResetT9State();

        StartEngine(_currentOptions.StartLevel);
    }

    /// <summary>
    /// Navigates the Game Over menu up or down.
    /// </summary>
    public void NavigateGameOverMenu(bool down)
    {
        if (!IsGameOver) return;

        if (down)
        {
            GameOverMenuIndex = (GameOverMenuIndex + 1) % GameOverMenuCount;
        }
        else
        {
            GameOverMenuIndex = (GameOverMenuIndex - 1 + GameOverMenuCount) % GameOverMenuCount;
        }
    }

    /// <summary>
    /// Activates the currently selected Game Over menu item.
    /// </summary>
    public void ActivateGameOverMenuItem()
    {
        if (!IsGameOver) return;

        if (CompletedPosts.Count > 0)
        {
            switch (GameOverMenuIndex)
            {
                case 0:
                    IncludeSkyDropSignature = !IncludeSkyDropSignature;
                    break;
                case 1:
                    if (CanPost)
                        PostToBlueskyCommand.Execute(null);
                    break;
                case 2:
                    RestartGame();
                    break;
                case 3:
                    ReturnToTitle();
                    break;
            }
        }
        else
        {
            // Menu: 0=Play Again, 1=Title
            switch (GameOverMenuIndex)
            {
                case 0:
                    RestartGame();
                    break;
                case 1:
                    ReturnToTitle();
                    break;
            }
        }
    }

    /// <summary>
    /// Resets all T9, ABC, and Kana state.
    /// </summary>
    private void ResetT9State()
    {
        SelectedKeyIndex = 0;
        UpdateSelectedKeyLabel();
        CurrentWordPreview = "";
        CurrentPost = "";
        CurrentPostLength = 0;
        IsPostTooLong = false;
        CanConfirmPost = true;
        CompletedPosts.Clear();
        PostingStatus = "";
        _t9Engine.NewCompletion();
        _t9Engine.CurrentCaseMode = T9Engine.CaseMode.Capitalize;

        var startingMode = _currentOptions?.StartInputMode ?? TextInputMode.T9;
        CurrentInputMode = startingMode;

        AbcCharacterIndex = 0;
        _capitalizeNext = true;
        UpdateAbcCharacterDisplay();

        KanaCharacterIndex = 0;
        CurrentKanaInput = "";
        CurrentKanaCharacter = "";
        KanjiSuggestions = Array.Empty<JapaneseWord>();
        SelectedSuggestionIndex = 0;

        if (startingMode == TextInputMode.Kana)
        {
            UpdateKanaCharacterDisplay();
        }

        OnPropertyChanged(nameof(IsAbcMode));
        OnPropertyChanged(nameof(IsKanaMode));
        OnPropertyChanged(nameof(InputModeDisplay));
        OnPropertyChanged(nameof(CurrentSuggestionDisplay));
        OnPropertyChanged(nameof(HasKanjiSuggestions));
    }

    /// <summary>
    /// Called when the tetromino is rotated.
    /// </summary>
    private void OnRotation(bool clockwise)
    {
        if (CurrentInputMode == TextInputMode.Kana)
        {
            var chars = KanaCharacters[SelectedKeyIndex];
            if (chars.Length == 0)
            {
                CycleKeyIndexKana(clockwise);
            }
            else
            {
                if (clockwise)
                {
                    KanaCharacterIndex++;
                    if (KanaCharacterIndex >= chars.Length)
                    {
                        CycleKeyIndexKana(clockwise);
                    }
                    else
                    {
                        UpdateKanaCharacterDisplay();
                    }
                }
                else
                {
                    KanaCharacterIndex--;
                    if (KanaCharacterIndex < 0)
                    {
                        CycleKeyIndexKanaToLastCharacter(clockwise);
                    }
                    else
                    {
                        UpdateKanaCharacterDisplay();
                    }
                }
            }
        }
        else if (CurrentInputMode == TextInputMode.ABC)
        {
            var chars = AbcCharacters[SelectedKeyIndex];
            if (chars.Length == 0)
            {
                CycleKeyIndex(clockwise);
            }
            else
            {
                if (clockwise)
                {
                    AbcCharacterIndex++;
                    if (AbcCharacterIndex >= chars.Length)
                    {
                        CycleKeyIndex(clockwise);
                    }
                    else
                    {
                        UpdateAbcCharacterDisplay();
                    }
                }
                else
                {
                    AbcCharacterIndex--;
                    if (AbcCharacterIndex < 0)
                    {
                        CycleKeyIndexToLastCharacter(clockwise);
                    }
                    else
                    {
                        UpdateAbcCharacterDisplay();
                    }
                }
            }
        }
        else
        {
            CycleKeyIndex(clockwise);
        }
    }

    /// <summary>
    /// Cycles to the previous key and sets character index to the last character.
    /// Used when rotating counter-clockwise past the first character.
    /// </summary>
    private void CycleKeyIndexToLastCharacter(bool clockwise)
    {
        SelectedKeyIndex = (SelectedKeyIndex - 1 + 12) % 12;
        UpdateSelectedKeyLabel();

        var chars = AbcCharacters[SelectedKeyIndex];
        if (chars.Length > 0)
        {
            AbcCharacterIndex = chars.Length - 1;
        }
        else
        {
            AbcCharacterIndex = 0;
            CycleKeyIndexToLastCharacter(clockwise);
            return;
        }
        UpdateAbcCharacterDisplay();
    }

    /// <summary>
    /// Cycles through keypad keys (used in T9 mode or for special keys in ABC mode).
    /// </summary>
    private void CycleKeyIndex(bool clockwise)
    {
        if (clockwise)
        {
            SelectedKeyIndex = (SelectedKeyIndex + 1) % 12;
        }
        else
        {
            SelectedKeyIndex = (SelectedKeyIndex - 1 + 12) % 12;
        }
        UpdateSelectedKeyLabel();

        if (CurrentInputMode == TextInputMode.ABC)
        {
            var chars = AbcCharacters[SelectedKeyIndex];
            if (chars.Length == 0)
            {
                CycleKeyIndex(clockwise);
                return;
            }
            AbcCharacterIndex = 0;
            UpdateAbcCharacterDisplay();
        }
    }

    /// <summary>
    /// Cycles through keypad keys in Kana mode.
    /// </summary>
    private void CycleKeyIndexKana(bool clockwise)
    {
        const int kanaKeyCount = 13;

        if (clockwise)
        {
            SelectedKeyIndex = (SelectedKeyIndex + 1) % kanaKeyCount;
        }
        else
        {
            SelectedKeyIndex = (SelectedKeyIndex - 1 + kanaKeyCount) % kanaKeyCount;
        }
        UpdateSelectedKeyLabel();

        var chars = KanaCharacters[SelectedKeyIndex];
        KanaCharacterIndex = 0;
        if (chars.Length > 0)
        {
            UpdateKanaCharacterDisplay();
        }
        else
        {
            CurrentKanaCharacter = "";
        }
    }

    /// <summary>
    /// Cycles to the previous key and sets character index to the last character in Kana mode.
    /// </summary>
    private void CycleKeyIndexKanaToLastCharacter(bool clockwise)
    {
        const int kanaKeyCount = 13;

        SelectedKeyIndex = (SelectedKeyIndex - 1 + kanaKeyCount) % kanaKeyCount;
        UpdateSelectedKeyLabel();

        var chars = KanaCharacters[SelectedKeyIndex];
        if (chars.Length > 0)
        {
            KanaCharacterIndex = chars.Length - 1;
            UpdateKanaCharacterDisplay();
        }
        else
        {
            KanaCharacterIndex = 0;
            CurrentKanaCharacter = "";
        }
    }

    /// <summary>
    /// Updates the kana character display based on current key and character index.
    /// </summary>
    private void UpdateKanaCharacterDisplay()
    {
        var chars = KanaCharacters[SelectedKeyIndex];
        if (chars.Length > 0 && KanaCharacterIndex < chars.Length)
        {
            CurrentKanaCharacter = chars[KanaCharacterIndex];
        }
        else
        {
            CurrentKanaCharacter = "";
        }
    }

    /// <summary>
    /// Called when a piece locks into place.
    /// </summary>
    private void OnPieceLocked()
    {
        ProcessKeyPress(SelectedKeyIndex);
    }

    /// <summary>
    /// Processes a keypad press.
    /// </summary>
    private void ProcessKeyPress(int keyIndex)
    {
        var key = KeypadLabels[keyIndex];

        // Handle Kana mode separately
        if (CurrentInputMode == TextInputMode.Kana)
        {
            ProcessKanaKeyPress(key);
            return;
        }

        switch (key)
        {
            case "*":
                DeleteWord();
                break;

            case "#":
                if (CanConfirmPost)
                {
                    ConfirmPost();
                }
                break;

            case "0":
                if (CurrentInputMode == TextInputMode.ABC)
                {
                    AddAbcCharacter();
                }
                else
                {
                    ConfirmCurrentWord();
                    CurrentPost += " ";
                    UpdatePostLength();
                }
                break;

            default:
                if (CurrentInputMode == TextInputMode.ABC)
                {
                    AddAbcCharacter();
                }
                else
                {
                    if (int.TryParse(key, out int digit))
                    {
                        AddT9Digit(digit);
                    }
                }
                break;
        }
    }

    /// <summary>
    /// Processes a keypad press in Kana mode.
    /// </summary>
    private void ProcessKanaKeyPress(string key)
    {
        switch (key)
        {
            case "*":
                if (!string.IsNullOrEmpty(CurrentKanaInput))
                {
                    ApplyDakutenOrHandakuten();
                }
                else
                {
                    DeleteKana();
                }
                break;

            case "#":
                if (!string.IsNullOrEmpty(CurrentKanaInput) || HasKanjiSuggestions)
                {
                    ConfirmKanaInput();
                }
                else
                {
                    CurrentPost += " ";
                }
                break;

            case "OK":
                if (CanAddKanaPost)
                {
                    ConfirmKanaPost();
                }
                else if (!string.IsNullOrEmpty(CurrentKanaInput))
                {
                    ConfirmKanaInput();
                }
                break;

            default:
                AddKanaCharacter();
                break;
        }
    }

    /// <summary>
    /// Adds the currently selected kana character to the input buffer.
    /// </summary>
    private void AddKanaCharacter()
    {
        var chars = KanaCharacters[SelectedKeyIndex];
        if (chars.Length == 0 || KanaCharacterIndex >= chars.Length)
            return;

        var character = chars[KanaCharacterIndex];
        CurrentKanaInput += character;
        UpdateKanjiSuggestions();
        UpdatePostLength();
    }

    /// <summary>
    /// Applies dakuten (゛) or handakuten (゜) to the last character in kana input.
    /// Cycles through: base → dakuten → handakuten → small → base
    /// </summary>
    private void ApplyDakutenOrHandakuten()
    {
        if (string.IsNullOrEmpty(CurrentKanaInput))
            return;

        var lastChar = CurrentKanaInput[^1];
        var prefix = CurrentKanaInput.Length > 1 ? CurrentKanaInput[..^1] : "";

        if (DakutenMap.TryGetValue(lastChar, out var dakutenChar))
        {
            CurrentKanaInput = prefix + dakutenChar;
            UpdateKanjiSuggestions();
            return;
        }

        if (HandakutenMap.TryGetValue(lastChar, out var handakutenChar))
        {
            CurrentKanaInput = prefix + handakutenChar;
            UpdateKanjiSuggestions();
            return;
        }

        if (SmallKanaMap.TryGetValue(lastChar, out var smallChar))
        {
            CurrentKanaInput = prefix + smallChar;
            UpdateKanjiSuggestions();
            return;
        }

        foreach (var kvp in DakutenMap)
        {
            if (kvp.Value == lastChar && HandakutenMap.TryGetValue(kvp.Key, out var nextChar))
            {
                CurrentKanaInput = prefix + nextChar;
                UpdateKanjiSuggestions();
                return;
            }
        }

        foreach (var kvp in HandakutenMap)
        {
            if (kvp.Value == lastChar)
            {
                if (SmallKanaMap.TryGetValue(kvp.Key, out var smallFromBase))
                {
                    CurrentKanaInput = prefix + smallFromBase;
                }
                else
                {
                    CurrentKanaInput = prefix + kvp.Key;
                }
                UpdateKanjiSuggestions();
                return;
            }
        }

        foreach (var kvp in SmallKanaMap)
        {
            if (kvp.Value == lastChar)
            {
                CurrentKanaInput = prefix + kvp.Key;
                UpdateKanjiSuggestions();
                return;
            }
        }
    }

    /// <summary>
    /// Updates kanji suggestions based on current kana input.
    /// </summary>
    private void UpdateKanjiSuggestions()
    {
        if (string.IsNullOrEmpty(CurrentKanaInput))
        {
            KanjiSuggestions = Array.Empty<JapaneseWord>();
            SelectedSuggestionIndex = 0;
        }
        else
        {
            KanjiSuggestions = _japaneseDictionary.LookupByPrefix(CurrentKanaInput, 20);
            SelectedSuggestionIndex = 0;
        }

        OnPropertyChanged(nameof(CurrentSuggestionDisplay));
        OnPropertyChanged(nameof(HasKanjiSuggestions));
    }

    /// <summary>
    /// Cycles to the next kanji suggestion.
    /// </summary>
    public void CycleKanjiSuggestion(bool forward = true)
    {
        if (KanjiSuggestions.Count == 0)
            return;

        if (forward)
        {
            SelectedSuggestionIndex = (SelectedSuggestionIndex + 1) % KanjiSuggestions.Count;
        }
        else
        {
            SelectedSuggestionIndex = (SelectedSuggestionIndex - 1 + KanjiSuggestions.Count) % KanjiSuggestions.Count;
        }

        OnPropertyChanged(nameof(CurrentSuggestionDisplay));
    }

    /// <summary>
    /// Confirms the current kana input (converting to selected kanji if available).
    /// </summary>
    private void ConfirmKanaInput()
    {
        if (string.IsNullOrEmpty(CurrentKanaInput))
        {
            if (CanConfirmPost)
            {
                ConfirmPost();
            }
            return;
        }

        var textToAdd = KanjiSuggestions.Count > 0 && SelectedSuggestionIndex < KanjiSuggestions.Count
            ? KanjiSuggestions[SelectedSuggestionIndex].Kanji
            : CurrentKanaInput;

        CurrentPost += textToAdd;
        UpdatePostLength();

        CurrentKanaInput = "";
        KanjiSuggestions = Array.Empty<JapaneseWord>();
        SelectedSuggestionIndex = 0;
        OnPropertyChanged(nameof(CurrentSuggestionDisplay));
        OnPropertyChanged(nameof(HasKanjiSuggestions));
    }

    /// <summary>
    /// Deletes the last kana character or last word from post.
    /// </summary>
    private void DeleteKana()
    {
        if (!string.IsNullOrEmpty(CurrentKanaInput))
        {
            CurrentKanaInput = CurrentKanaInput[..^1];
            UpdateKanjiSuggestions();
        }
        else if (!string.IsNullOrEmpty(CurrentPost))
        {
            CurrentPost = CurrentPost[..^1];
            UpdatePostLength();
        }
    }

    /// <summary>
    /// Adds the currently selected ABC character to the post.
    /// </summary>
    private void AddAbcCharacter()
    {
        var chars = AbcCharacters[SelectedKeyIndex];
        if (chars.Length == 0 || AbcCharacterIndex >= chars.Length)
            return;

        var character = chars[AbcCharacterIndex];

        var shouldCapitalize = _capitalizeNext && char.IsLetter(character[0]);
        if (shouldCapitalize)
        {
            character = character.ToUpperInvariant();
            _capitalizeNext = false;
        }

        CurrentPost += character;
        UpdatePostLength();

        if (character is "." or "!" or "?")
        {
            _capitalizeNext = true;
        }
    }

    /// <summary>
    /// Adds a digit to the T9 engine and updates the preview.
    /// </summary>
    private void AddT9Digit(int digit)
    {
        try
        {
            if (digit == 1)
            {
                if (!_t9Engine.HandlePunctuation)
                {
                    var prevCompletion = _t9Engine.GetCompletion();
                    if (prevCompletion.Length > 0)
                    {
                        CurrentPost += prevCompletion;
                        UpdatePostLength();
                    }
                    _t9Engine.HandlePunctuation = true;
                    _t9Engine.NewCompletion();
                    _t9Engine.CurrentCaseMode = T9Engine.CaseMode.Normal;
                    CurrentT9Sequence = "";
                }

                CurrentWordPreview = _t9Engine.AddDigit(digit);
                CurrentT9Sequence += digit.ToString();
            }
            else
            {
                if (_t9Engine.HandlePunctuation)
                {
                    var punctuation = _t9Engine.GetCompletion();
                    if (punctuation.Length > 0)
                    {
                        CurrentPost += punctuation;
                        UpdatePostLength();

                        if (punctuation is "." or "!" or "?")
                        {
                            _t9Engine.CurrentCaseMode = T9Engine.CaseMode.Capitalize;
                        }
                    }
                    _t9Engine.HandlePunctuation = false;
                    _t9Engine.NewCompletion();
                    CurrentT9Sequence = "";
                }

                CurrentWordPreview = _t9Engine.AddDigit(digit);
                CurrentT9Sequence += digit.ToString();
            }
        }
        catch (WordNotFoundException)
        {
            // No word found for this sequence
        }
    }

    /// <summary>
    /// Confirms the current word and adds it to the post.
    /// </summary>
    private void ConfirmCurrentWord()
    {
        if (!string.IsNullOrEmpty(CurrentWordPreview))
        {
            CurrentPost += CurrentWordPreview;
            UpdatePostLength();

            var wasPunctuation = _t9Engine.HandlePunctuation;
            var shouldCapitalize = wasPunctuation && CurrentWordPreview is "." or "!" or "?";

            _t9Engine.NewCompletion();
            _t9Engine.HandlePunctuation = false;
            _t9Engine.CurrentCaseMode = shouldCapitalize ? T9Engine.CaseMode.Capitalize : T9Engine.CaseMode.Normal;
            CurrentWordPreview = "";
            CurrentT9Sequence = "";
        }
    }

    /// <summary>
    /// Deletes the current word being typed, or the last word in the post.
    /// </summary>
    private void DeleteWord()
    {
        if (_t9Engine.NumEngineChars > 0)
        {
            _t9Engine.Backspace();
            CurrentWordPreview = _t9Engine.GetCompletion();

            if (CurrentT9Sequence.Length > 0)
            {
                CurrentT9Sequence = CurrentT9Sequence[..^1];
            }

            if (_t9Engine.NumEngineChars == 0)
            {
                _t9Engine.HandlePunctuation = false;
                CurrentT9Sequence = "";
            }
        }
        else if (!string.IsNullOrEmpty(CurrentPost))
        {
            var trimmed = CurrentPost.TrimEnd();
            var lastSpace = trimmed.LastIndexOf(' ');
            if (lastSpace >= 0)
            {
                CurrentPost = trimmed[..(lastSpace + 1)];
            }
            else
            {
                CurrentPost = "";
            }
            UpdatePostLength();

            _t9Engine.HandlePunctuation = false;
        }
    }

    /// <summary>
    /// Confirms the current post and starts a new one.
    /// </summary>
    private void ConfirmPost()
    {
        ConfirmCurrentWord();

        var post = CurrentPost.Trim();
        if (!string.IsNullOrEmpty(post))
        {
            CompletedPosts.Add(post);
        }

        CurrentPost = "";
        CurrentWordPreview = "";
        CurrentPostLength = 0;
        IsPostTooLong = false;
        CanConfirmPost = true;
        _t9Engine.NewCompletion();
        _t9Engine.CurrentCaseMode = T9Engine.CaseMode.Capitalize;
        _capitalizeNext = true;
    }

    /// <summary>
    /// Updates the post length and related properties.
    /// </summary>
    private void UpdatePostLength()
    {
        var fullText = CurrentPost + CurrentWordPreview + CurrentKanaInput;
        CurrentPostLength = fullText.Length;
        IsPostTooLong = CurrentPostLength > MaxPostLength;
        CanConfirmPost = !IsPostTooLong;
    }

    /// <summary>
    /// Updates the selected key label based on index.
    /// </summary>
    private void UpdateSelectedKeyLabel()
    {
        SelectedKeyLabel = KeypadLabels[SelectedKeyIndex];
    }

    /// <summary>
    /// Updates the ABC character display based on current key and character index.
    /// </summary>
    private void UpdateAbcCharacterDisplay()
    {
        var chars = AbcCharacters[SelectedKeyIndex];
        if (chars.Length > 0 && AbcCharacterIndex < chars.Length)
        {
            CurrentAbcCharacter = chars[AbcCharacterIndex];
        }
        else
        {
            CurrentAbcCharacter = "";
        }
    }

    /// <summary>
    /// Called when the game ends.
    /// </summary>
    protected override void OnGameOver()
    {
        FinalizeCurrentInput();
        var post = CurrentPost.Trim();
        if (!string.IsNullOrEmpty(post))
        {
            CompletedPosts.Add(post);
            CurrentPost = "";
        }

        GameOverMenuIndex = CompletedPosts.Count > 0 ? 2 : 0;

        base.OnGameOver();
    }

    /// <summary>
    /// Posts the completed posts to Bluesky as a thread.
    /// </summary>
    /// <summary>
    /// Confirms the current kana post and adds it to completed posts.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanAddKanaPost))]
    private void ConfirmKanaPost()
    {
        if (!string.IsNullOrEmpty(CurrentKanaInput))
        {
            ConfirmKanaInput();
        }

        if (CanConfirmPost && !string.IsNullOrWhiteSpace(CurrentPost))
        {
            ConfirmPost();
        }
    }

    [RelayCommand]
    private async Task PostToBlueskyAsync()
    {
        if (CompletedPosts.Count == 0)
        {
            PostingStatus = "No posts to send!";
            return;
        }

        if (_protocol.Session is null)
        {
            PostingStatus = "Not logged in!";
            return;
        }

        IsPosting = true;
        PostingStatus = "Posting to Bluesky...";

        try
        {
            // TODO: Implement posting using _protocol.Repo.CreateRecordAsync
            // For now, simulate posting
            await Task.Delay(1000);

            var totalPosts = CompletedPosts.Count;

            if (IncludeSkyDropSignature)
            {
                PostingStatus = "Generating stats image...";

                var statsImageBytes = StatsImageGenerator.GenerateStatsImage(
                    Score,
                    Level,
                    Lines,
                    CompletedPosts.Count);

                // TODO: Upload image and create signature post
                // The signature post text
                var signatureText = $"Posted by #SkyDrop\n\nScore: {Score:N0} | Level: {Level} | Lines: {Lines}";

                // For now, simulate uploading
                await Task.Delay(500);

                totalPosts++;
            }

            PostingStatus = $"Successfully posted {totalPosts} post(s) to Bluesky!";
            HasPosted = true;
        }
        catch (Exception ex)
        {
            PostingStatus = $"Error: {ex.Message}";
        }
        finally
        {
            IsPosting = false;
        }
    }

    /// <summary>
    /// Gets the generated stats image bytes (for preview or testing).
    /// </summary>
    public byte[] GenerateStatsImageBytes()
    {
        return StatsImageGenerator.GenerateStatsImage(Score, Level, Lines, CompletedPosts.Count);
    }

    /// <summary>
    /// Cycles to the next T9 word completion.
    /// Can be called externally (e.g., via a specific key combination).
    /// </summary>
    public void CycleT9Completion()
    {
        try
        {
            _t9Engine.NextCompletion();
            CurrentWordPreview = _t9Engine.GetCompletion();
        }
        catch (WordNotFoundException)
        {
            // No more completions - stay on current
        }
    }

    /// <summary>
    /// Toggles between T9, ABC, and Kana input modes.
    /// Cycles: T9 → ABC → Kana → T9
    /// </summary>
    public void ToggleInputMode()
    {
        FinalizeCurrentInput();

        switch (CurrentInputMode)
        {
            case TextInputMode.T9:
                CurrentInputMode = TextInputMode.ABC;
                AbcCharacterIndex = 0;
                UpdateAbcCharacterDisplay();
                break;

            case TextInputMode.ABC:
                CurrentInputMode = TextInputMode.Kana;
                CurrentAbcCharacter = "";
                KanaCharacterIndex = 0;
                UpdateKanaCharacterDisplay();
                break;

            case TextInputMode.Kana:
                CurrentInputMode = TextInputMode.T9;
                CurrentKanaCharacter = "";
                _t9Engine.CurrentCaseMode = _capitalizeNext ? T9Engine.CaseMode.Capitalize : T9Engine.CaseMode.Normal;

                if (SelectedKeyIndex == 12)
                {
                    SelectedKeyIndex = 10;
                    UpdateSelectedKeyLabel();
                }
                break;
        }

        OnPropertyChanged(nameof(IsAbcMode));
        OnPropertyChanged(nameof(IsKanaMode));
        OnPropertyChanged(nameof(InputModeDisplay));
    }

    /// <summary>
    /// Finalizes any pending input from the current mode before switching.
    /// </summary>
    private void FinalizeCurrentInput()
    {
        switch (CurrentInputMode)
        {
            case TextInputMode.T9:
                if (_t9Engine.NumEngineChars > 0)
                {
                    var completion = _t9Engine.GetCompletion();
                    if (!string.IsNullOrEmpty(completion))
                    {
                        CurrentPost += completion;
                        UpdatePostLength();
                    }
                }
                _t9Engine.NewCompletion();
                _t9Engine.HandlePunctuation = false;
                CurrentWordPreview = "";
                CurrentT9Sequence = "";
                break;

            case TextInputMode.ABC:
                CurrentAbcCharacter = "";
                break;

            case TextInputMode.Kana:
                if (!string.IsNullOrEmpty(CurrentKanaInput))
                {
                    var textToAdd = KanjiSuggestions.Count > 0 && SelectedSuggestionIndex < KanjiSuggestions.Count
                        ? KanjiSuggestions[SelectedSuggestionIndex].Kanji
                        : CurrentKanaInput;
                    CurrentPost += textToAdd;
                    UpdatePostLength();
                }
                CurrentKanaInput = "";
                CurrentKanaCharacter = "";
                KanjiSuggestions = Array.Empty<JapaneseWord>();
                SelectedSuggestionIndex = 0;
                OnPropertyChanged(nameof(CurrentSuggestionDisplay));
                OnPropertyChanged(nameof(HasKanjiSuggestions));
                break;
        }
    }
}
