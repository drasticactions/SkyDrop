namespace SkyDrop.Models;

/// <summary>
/// Represents a user who has logged in, including identifying information and session details. 
/// </summary>
public class LoginUser
{
    /// <summary>
    /// Gets or sets the users handle.
    /// </summary>
    public string Handle { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the email address.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the users ATDid.
    /// </summary>
    public string Did { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the session data from the login.
    /// </summary>
    public string SessionData { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the login type.
    /// </summary>
    public LoginType LoginType { get; set; }
}