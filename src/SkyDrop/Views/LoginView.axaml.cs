using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using SkyDrop.ViewModels;

namespace SkyDrop.Views;

public partial class LoginView : UserControl
{
    private enum NavSection { Identifier, Password, Login, Logout, Back }

    private NavSection _currentSection = NavSection.Identifier;
    private Border? _identifierBorder;
    private Border? _passwordBorder;
    private Border? _loginButtonBorder;
    private Border? _logoutButtonBorder;
    private Border? _backButtonBorder;
    private TextBox? _identifierTextBox;
    private TextBox? _passwordTextBox;

    public LoginView()
    {
        InitializeComponent();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        _identifierBorder = this.FindControl<Border>("IdentifierBorder");
        _passwordBorder = this.FindControl<Border>("PasswordBorder");
        _loginButtonBorder = this.FindControl<Border>("LoginButtonBorder");
        _logoutButtonBorder = this.FindControl<Border>("LogoutButtonBorder");
        _backButtonBorder = this.FindControl<Border>("BackButtonBorder");
        _identifierTextBox = this.FindControl<TextBox>("IdentifierTextBox");
        _passwordTextBox = this.FindControl<TextBox>("PasswordTextBox");

        // Set initial section based on authentication state
        if (DataContext is LoginViewModel vm)
        {
            _currentSection = vm.IsAuthenticated ? NavSection.Logout : NavSection.Identifier;
        }

        UpdateSelectionVisuals();
        Focus();
    }

    private void UpdateSelectionVisuals()
    {
        var accentBrush = this.FindResource("AccentCyanBrush") as IBrush;
        var transparentBrush = Brushes.Transparent;

        if (_identifierBorder != null)
        {
            _identifierBorder.BorderBrush = _currentSection == NavSection.Identifier ? accentBrush : transparentBrush;
        }

        if (_passwordBorder != null)
        {
            _passwordBorder.BorderBrush = _currentSection == NavSection.Password ? accentBrush : transparentBrush;
        }

        if (_loginButtonBorder != null)
        {
            _loginButtonBorder.BorderBrush = _currentSection == NavSection.Login ? accentBrush : transparentBrush;
        }

        if (_logoutButtonBorder != null)
        {
            _logoutButtonBorder.BorderBrush = _currentSection == NavSection.Logout ? accentBrush : transparentBrush;
        }

        if (_backButtonBorder != null)
        {
            _backButtonBorder.BorderBrush = _currentSection == NavSection.Back ? accentBrush : transparentBrush;
        }
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        if (DataContext is not LoginViewModel vm) return;

        // Check if a TextBox has focus and is handling input
        var focusedElement = TopLevel.GetTopLevel(this)?.FocusManager?.GetFocusedElement();
        var isTextBoxFocused = focusedElement is TextBox;

        switch (e.Key)
        {
            case Key.Enter:
            case Key.Space:
                if (isTextBoxFocused && e.Key == Key.Space)
                {
                    // Let TextBox handle space for text input
                    return;
                }

                switch (_currentSection)
                {
                    case NavSection.Identifier:
                        // Focus the identifier textbox for input
                        _identifierTextBox?.Focus();
                        e.Handled = true;
                        break;
                    case NavSection.Password:
                        // Focus the password textbox for input
                        _passwordTextBox?.Focus();
                        e.Handled = true;
                        break;
                    case NavSection.Login:
                        if (vm.CanLoginWithPassword && !vm.IsLoggingIn)
                        {
                            vm.LoginWithPasswordCommand.Execute(null);
                        }
                        e.Handled = true;
                        break;
                    case NavSection.Logout:
                        vm.LogoutCommand.Execute(null);
                        // After logout, reset to identifier section
                        _currentSection = NavSection.Identifier;
                        UpdateSelectionVisuals();
                        e.Handled = true;
                        break;
                    case NavSection.Back:
                        vm.BackCommand.Execute(null);
                        e.Handled = true;
                        break;
                }
                break;

            case Key.Escape:
                if (isTextBoxFocused)
                {
                    // Return focus to the view itself
                    Focus();
                    e.Handled = true;
                }
                else
                {
                    vm.BackCommand.Execute(null);
                    e.Handled = true;
                }
                break;

            case Key.Up:
            case Key.W:
                if (isTextBoxFocused)
                {
                    // Return focus to view and handle navigation
                    Focus();
                }

                if (vm.IsAuthenticated)
                {
                    // Authenticated: Logout -> Back cycle
                    _currentSection = _currentSection switch
                    {
                        NavSection.Back => NavSection.Logout,
                        _ => _currentSection
                    };
                }
                else
                {
                    // Not authenticated: Identifier -> Password -> Login -> Back
                    _currentSection = _currentSection switch
                    {
                        NavSection.Password => NavSection.Identifier,
                        NavSection.Login => NavSection.Password,
                        NavSection.Back => NavSection.Login,
                        _ => _currentSection
                    };
                }
                UpdateSelectionVisuals();
                e.Handled = true;
                break;

            case Key.Down:
            case Key.S:
                if (isTextBoxFocused)
                {
                    // Return focus to view and handle navigation
                    Focus();
                }

                if (vm.IsAuthenticated)
                {
                    // Authenticated: Logout -> Back cycle
                    _currentSection = _currentSection switch
                    {
                        NavSection.Logout => NavSection.Back,
                        _ => _currentSection
                    };
                }
                else
                {
                    // Not authenticated: Identifier -> Password -> Login -> Back
                    _currentSection = _currentSection switch
                    {
                        NavSection.Identifier => NavSection.Password,
                        NavSection.Password => NavSection.Login,
                        NavSection.Login => NavSection.Back,
                        _ => _currentSection
                    };
                }
                UpdateSelectionVisuals();
                e.Handled = true;
                break;

            case Key.Tab:
                if (!isTextBoxFocused) return;

                // Handle Tab navigation between text fields
                if (_identifierTextBox?.IsFocused == true)
                {
                    _currentSection = NavSection.Password;
                    _passwordTextBox?.Focus();
                    UpdateSelectionVisuals();
                    e.Handled = true;
                }
                else if (_passwordTextBox?.IsFocused == true)
                {
                    _currentSection = NavSection.Login;
                    Focus();
                    UpdateSelectionVisuals();
                    e.Handled = true;
                }
                break;
        }
    }
}
