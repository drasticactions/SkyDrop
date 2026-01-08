using CommunityToolkit.Mvvm.ComponentModel;
using SkyDrop.Models;

namespace SkyDrop.ViewModels;

/// <summary>
/// ViewModel for the main window/view
/// </summary>
public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty]
    private ViewModelBase _currentViewModel;

    private readonly TitleScreenViewModel _titleScreenViewModel;
    private readonly NormalModeOptionsViewModel _normalModeOptionsViewModel;
    private readonly DiscoverFeedModeOptionsViewModel _discoverFeedModeOptionsViewModel;
    private readonly AuthorFeedModeOptionsViewModel _authorFeedModeOptionsViewModel;
    private readonly TimelineModeOptionsViewModel _timelineModeOptionsViewModel;
    private readonly CreatePostModeOptionsViewModel _createPostModeOptionsViewModel;
    private readonly GameViewModel _gameViewModel;
    private readonly DiscoverFeedGameViewModel _discoverFeedGameViewModel;
    private readonly DiscoverFeedScrollGameViewModel _discoverFeedScrollGameViewModel;
    private readonly RevealFeedGameViewModel _revealFeedGameViewModel;
    private readonly ScrollFeedGameViewModel _scrollFeedGameViewModel;
    private readonly CreatePostGameViewModel _createPostGameViewModel;
    private readonly LoginViewModel _loginViewModel;
    private readonly T9GeneratorViewModel _t9GeneratorViewModel;
    private readonly CreditsViewModel _creditsViewModel;

    public MainWindowViewModel(
        LoginViewModel loginViewModel,
        DiscoverFeedGameViewModel discoverFeedGameViewModel,
        DiscoverFeedScrollGameViewModel discoverFeedScrollGameViewModel,
        RevealFeedGameViewModel revealFeedGameViewModel,
        ScrollFeedGameViewModel scrollFeedGameViewModel,
        CreatePostGameViewModel createPostGameViewModel,
        DiscoverFeedModeOptionsViewModel discoverFeedModeOptionsViewModel,
        AuthorFeedModeOptionsViewModel authorFeedModeOptionsViewModel,
        TimelineModeOptionsViewModel timelineModeOptionsViewModel,
        T9GeneratorViewModel t9GeneratorViewModel)
    {
        _loginViewModel = loginViewModel;
        _discoverFeedGameViewModel = discoverFeedGameViewModel;
        _discoverFeedScrollGameViewModel = discoverFeedScrollGameViewModel;
        _revealFeedGameViewModel = revealFeedGameViewModel;
        _scrollFeedGameViewModel = scrollFeedGameViewModel;
        _createPostGameViewModel = createPostGameViewModel;
        _discoverFeedModeOptionsViewModel = discoverFeedModeOptionsViewModel;
        _authorFeedModeOptionsViewModel = authorFeedModeOptionsViewModel;
        _timelineModeOptionsViewModel = timelineModeOptionsViewModel;
        _t9GeneratorViewModel = t9GeneratorViewModel;

        _titleScreenViewModel = new TitleScreenViewModel();
        _normalModeOptionsViewModel = new NormalModeOptionsViewModel();
        _createPostModeOptionsViewModel = new CreatePostModeOptionsViewModel();
        _gameViewModel = new GameViewModel();
        _creditsViewModel = new CreditsViewModel();

        _titleScreenViewModel.ModeSelected += OnModeSelected;
        _titleScreenViewModel.LoginRequested += OnLoginRequested;
        _titleScreenViewModel.CreditsRequested += OnCreditsRequested;

        _normalModeOptionsViewModel.StartGameRequested += OnStartGame;
        _normalModeOptionsViewModel.BackRequested += OnBackToModeSelection;

        _discoverFeedModeOptionsViewModel.StartGameRequested += OnStartDiscoverFeedGame;
        _discoverFeedModeOptionsViewModel.BackRequested += OnBackToModeSelection;

        _authorFeedModeOptionsViewModel.StartGameRequested += OnStartAuthorFeedGame;
        _authorFeedModeOptionsViewModel.BackRequested += OnBackToModeSelection;

        _timelineModeOptionsViewModel.StartGameRequested += OnStartTimelineGame;
        _timelineModeOptionsViewModel.BackRequested += OnBackToModeSelection;

        _createPostModeOptionsViewModel.StartGameRequested += OnStartCreatePostGame;
        _createPostModeOptionsViewModel.BackRequested += OnBackToModeSelection;

        _loginViewModel.BackRequested += OnBackToModeSelection;

        _t9GeneratorViewModel.BackRequested += OnBackToModeSelection;

        _creditsViewModel.BackRequested += OnBackToModeSelection;

        _gameViewModel.RequestReturnToTitle += OnReturnToTitle;
        _discoverFeedGameViewModel.RequestReturnToTitle += OnReturnToTitle;
        _discoverFeedScrollGameViewModel.RequestReturnToTitle += OnReturnToTitle;
        _revealFeedGameViewModel.RequestReturnToTitle += OnReturnToTitle;
        _scrollFeedGameViewModel.RequestReturnToTitle += OnReturnToTitle;
        _createPostGameViewModel.RequestReturnToTitle += OnReturnToTitle;

        _currentViewModel = _titleScreenViewModel;
    }

    private void OnModeSelected(GameModeInfo mode)
    {
        CurrentViewModel = mode.Mode switch
        {
            GameMode.Normal => _normalModeOptionsViewModel,
            GameMode.DiscoverFeed or GameMode.DiscoverFeedScroll => _discoverFeedModeOptionsViewModel,
            GameMode.AuthorFeed or GameMode.AuthorFeedScroll => _authorFeedModeOptionsViewModel,
            GameMode.Timeline or GameMode.TimelineScroll => _timelineModeOptionsViewModel,
            GameMode.CreatePost => _createPostModeOptionsViewModel,
            GameMode.T9Generator => _t9GeneratorViewModel,
            _ => _titleScreenViewModel
        };
    }

    private void OnStartGame(IGameModeOptions options)
    {
        CurrentViewModel = _gameViewModel;
        _gameViewModel.StartGame(options);
    }

    private async void OnStartDiscoverFeedGame(IGameModeOptions options)
    {
        if (options is DiscoverFeedScrollModeOptions scrollOptions)
        {
            CurrentViewModel = _discoverFeedScrollGameViewModel;
            await _discoverFeedScrollGameViewModel.StartGameAsync(scrollOptions);
        }
        else if (options is DiscoverFeedModeOptions discoverOptions)
        {
            CurrentViewModel = _discoverFeedGameViewModel;
            await _discoverFeedGameViewModel.StartGameAsync(discoverOptions);
        }
    }

    private async void OnStartAuthorFeedGame(IGameModeOptions options)
    {
        if (options is FeedModeOptions feedOptions)
        {
            if (feedOptions.GameType == FeedGameType.Scroll)
            {
                CurrentViewModel = _scrollFeedGameViewModel;
                await _scrollFeedGameViewModel.StartGameAsync(feedOptions);
            }
            else
            {
                CurrentViewModel = _revealFeedGameViewModel;
                await _revealFeedGameViewModel.StartGameAsync(feedOptions);
            }
        }
    }

    private async void OnStartTimelineGame(IGameModeOptions options)
    {
        if (options is FeedModeOptions feedOptions)
        {
            if (feedOptions.GameType == FeedGameType.Scroll)
            {
                CurrentViewModel = _scrollFeedGameViewModel;
                await _scrollFeedGameViewModel.StartGameAsync(feedOptions);
            }
            else
            {
                CurrentViewModel = _revealFeedGameViewModel;
                await _revealFeedGameViewModel.StartGameAsync(feedOptions);
            }
        }
    }

    private void OnStartCreatePostGame(IGameModeOptions options)
    {
        if (options is CreatePostModeOptions createPostOptions)
        {
            CurrentViewModel = _createPostGameViewModel;
            _createPostGameViewModel.StartGame(createPostOptions);
        }
    }

    private void OnBackToModeSelection()
    {
        CurrentViewModel = _titleScreenViewModel;
    }

    private void OnReturnToTitle()
    {
        CurrentViewModel = _titleScreenViewModel;
    }

    private void OnLoginRequested()
    {
        CurrentViewModel = _loginViewModel;
    }

    private void OnCreditsRequested()
    {
        CurrentViewModel = _creditsViewModel;
    }

    /// <summary>
    /// Gets the game view model for input handling.
    /// </summary>
    public GameViewModel GameViewModel => _gameViewModel;

    /// <summary>
    /// Checks if we're currently in the game screen.
    /// </summary>
    public bool IsInGame => CurrentViewModel == _gameViewModel
        || CurrentViewModel == _discoverFeedGameViewModel
        || CurrentViewModel == _discoverFeedScrollGameViewModel
        || CurrentViewModel == _revealFeedGameViewModel
        || CurrentViewModel == _scrollFeedGameViewModel
        || CurrentViewModel == _createPostGameViewModel;

    /// <summary>
    /// Gets the discover feed game view model.
    /// </summary>
    public DiscoverFeedGameViewModel DiscoverFeedGameViewModel => _discoverFeedGameViewModel;

    /// <summary>
    /// Gets the discover feed scroll game view model.
    /// </summary>
    public DiscoverFeedScrollGameViewModel DiscoverFeedScrollGameViewModel => _discoverFeedScrollGameViewModel;

    /// <summary>
    /// Gets the create post game view model.
    /// </summary>
    public CreatePostGameViewModel CreatePostGameViewModel => _createPostGameViewModel;
}
