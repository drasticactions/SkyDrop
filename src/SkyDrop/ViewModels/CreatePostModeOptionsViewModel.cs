using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SkyDrop.Models;
using SkyDrop.Resources;

namespace SkyDrop.ViewModels;

/// <summary>
/// View model for the Create Post mode options screen.
/// </summary>
public partial class CreatePostModeOptionsViewModel : GameModeOptionsViewModelBase
{
    private const int MaxPostLength = 300;

    [ObservableProperty]
    private int _selectedLevel;

    [ObservableProperty]
    private TextInputMode _selectedInputMode = TextInputMode.T9;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsStandardVariant))]
    [NotifyPropertyChangedFor(nameof(IsQueuedVariant))]
    [NotifyPropertyChangedFor(nameof(CanPlay))]
    private CreatePostVariant _selectedVariant = CreatePostVariant.Standard;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CurrentQueuedPostLength))]
    [NotifyPropertyChangedFor(nameof(IsCurrentPostTooLong))]
    [NotifyPropertyChangedFor(nameof(CanAddQueuedPost))]
    private string _currentQueuedPostText = "";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanAddQueuedPost))]
    private int? _editingPostIndex;

    /// <summary>
    /// Collection of queued posts for the Queued variant.
    /// </summary>
    public ObservableCollection<string> QueuedPosts { get; } = new();

    /// <summary>
    /// Available levels (0-19).
    /// </summary>
    public int[] AvailableLevels { get; } = Enumerable.Range(0, 20).ToArray();

    /// <summary>
    /// Display name for the currently selected input mode.
    /// </summary>
    public string SelectedInputModeDisplay => SelectedInputMode switch
    {
        TextInputMode.T9 => Strings.InputModeT9,
        TextInputMode.ABC => Strings.InputModeABC,
        TextInputMode.Kana => Strings.InputModeKana,
        _ => Strings.InputModeT9
    };

    /// <summary>
    /// Display name for the currently selected variant.
    /// </summary>
    public string SelectedVariantDisplay => SelectedVariant switch
    {
        CreatePostVariant.Standard => Strings.VariantStandard,
        CreatePostVariant.Queued => Strings.VariantQueued,
        _ => Strings.VariantStandard
    };

    /// <summary>
    /// Description of the currently selected variant.
    /// </summary>
    public string VariantDescription => SelectedVariant switch
    {
        CreatePostVariant.Standard => Strings.VariantStandardDesc,
        CreatePostVariant.Queued => Strings.VariantQueuedDesc,
        _ => Strings.VariantStandardDesc
    };

    /// <summary>
    /// Whether the Standard variant is selected.
    /// </summary>
    public bool IsStandardVariant => SelectedVariant == CreatePostVariant.Standard;

    /// <summary>
    /// Whether the Queued variant is selected.
    /// </summary>
    public bool IsQueuedVariant => SelectedVariant == CreatePostVariant.Queued;

    /// <summary>
    /// Character count for the current post being edited.
    /// </summary>
    public int CurrentQueuedPostLength => CurrentQueuedPostText?.Length ?? 0;

    /// <summary>
    /// Whether the current post exceeds the maximum length.
    /// </summary>
    public bool IsCurrentPostTooLong => CurrentQueuedPostLength > MaxPostLength;

    /// <summary>
    /// Whether a queued post can be added (has content and not too long).
    /// </summary>
    public bool CanAddQueuedPost => !string.IsNullOrWhiteSpace(CurrentQueuedPostText)
        && !IsCurrentPostTooLong;

    /// <summary>
    /// Whether currently editing an existing post (vs adding new).
    /// </summary>
    public bool IsEditingPost => EditingPostIndex.HasValue;

    /// <summary>
    /// Total word count across all queued posts.
    /// </summary>
    public int TotalQueuedWords => QueuedPosts
        .SelectMany(p => p.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries))
        .Count();

    /// <summary>
    /// Whether the Play button should be enabled.
    /// For Queued variant, requires at least one post in the queue.
    /// </summary>
    public bool CanPlay => SelectedVariant == CreatePostVariant.Standard
        || QueuedPosts.Count > 0;

    /// <inheritdoc/>
    public override GameMode Mode => GameMode.CreatePost;

    /// <inheritdoc/>
    public override IGameModeOptions GetOptions() => new CreatePostModeOptions(
        SelectedLevel,
        SelectedInputMode,
        SelectedVariant,
        SelectedVariant == CreatePostVariant.Queued ? QueuedPosts.ToList() : null);

    public CreatePostModeOptionsViewModel()
    {
        QueuedPosts.CollectionChanged += (_, _) =>
        {
            OnPropertyChanged(nameof(TotalQueuedWords));
            OnPropertyChanged(nameof(CanPlay));
        };
    }

    partial void OnSelectedInputModeChanged(TextInputMode value)
    {
        OnPropertyChanged(nameof(SelectedInputModeDisplay));
    }

    partial void OnSelectedVariantChanged(CreatePostVariant value)
    {
        OnPropertyChanged(nameof(SelectedVariantDisplay));
        OnPropertyChanged(nameof(VariantDescription));
    }

    partial void OnSelectedLevelChanging(int value)
    {
        if (value < 0) _selectedLevel = 0;
        else if (value > 19) _selectedLevel = 19;
    }

    [RelayCommand]
    private void Play()
    {
        if (!CanPlay) return;

        // Clear queued posts when starting Standard mode
        if (SelectedVariant == CreatePostVariant.Standard)
        {
            QueuedPosts.Clear();
            CancelEdit();
        }

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

    [RelayCommand]
    private void ToggleVariant()
    {
        SelectedVariant = SelectedVariant switch
        {
            CreatePostVariant.Standard => CreatePostVariant.Queued,
            CreatePostVariant.Queued => CreatePostVariant.Standard,
            _ => CreatePostVariant.Standard
        };
    }

    [RelayCommand]
    private void SelectStandardVariant()
    {
        SelectedVariant = CreatePostVariant.Standard;
    }

    [RelayCommand]
    private void SelectQueuedVariant()
    {
        SelectedVariant = CreatePostVariant.Queued;
    }

    [RelayCommand]
    private void AddOrUpdateQueuedPost()
    {
        if (!CanAddQueuedPost) return;

        var postText = CurrentQueuedPostText.Trim();

        if (EditingPostIndex.HasValue)
        {
            // Update existing post
            QueuedPosts[EditingPostIndex.Value] = postText;
            EditingPostIndex = null;
        }
        else
        {
            // Add new post
            QueuedPosts.Add(postText);
        }

        CurrentQueuedPostText = "";
        OnPropertyChanged(nameof(IsEditingPost));
    }

    [RelayCommand]
    private void CancelEdit()
    {
        CurrentQueuedPostText = "";
        EditingPostIndex = null;
        OnPropertyChanged(nameof(IsEditingPost));
    }

    [RelayCommand]
    private void EditQueuedPost(int index)
    {
        if (index < 0 || index >= QueuedPosts.Count) return;

        CurrentQueuedPostText = QueuedPosts[index];
        EditingPostIndex = index;
        OnPropertyChanged(nameof(IsEditingPost));
    }

    [RelayCommand]
    private void RemoveQueuedPost(int index)
    {
        if (index < 0 || index >= QueuedPosts.Count) return;

        // If we're editing this post, cancel the edit
        if (EditingPostIndex == index)
        {
            CancelEdit();
        }
        else if (EditingPostIndex > index)
        {
            // Adjust editing index if we removed a post before it
            EditingPostIndex--;
        }

        QueuedPosts.RemoveAt(index);
    }

    [RelayCommand]
    private void MoveQueuedPostUp(int index)
    {
        if (index <= 0 || index >= QueuedPosts.Count) return;

        (QueuedPosts[index], QueuedPosts[index - 1]) = (QueuedPosts[index - 1], QueuedPosts[index]);

        // Adjust editing index if needed
        if (EditingPostIndex == index)
            EditingPostIndex = index - 1;
        else if (EditingPostIndex == index - 1)
            EditingPostIndex = index;
    }

    [RelayCommand]
    private void MoveQueuedPostDown(int index)
    {
        if (index < 0 || index >= QueuedPosts.Count - 1) return;

        (QueuedPosts[index], QueuedPosts[index + 1]) = (QueuedPosts[index + 1], QueuedPosts[index]);

        // Adjust editing index if needed
        if (EditingPostIndex == index)
            EditingPostIndex = index + 1;
        else if (EditingPostIndex == index + 1)
            EditingPostIndex = index;
    }

    /// <summary>
    /// Clears all queued posts.
    /// </summary>
    [RelayCommand]
    private void ClearQueue()
    {
        QueuedPosts.Clear();
        CancelEdit();
    }
}
