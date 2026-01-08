namespace SkyDrop.Models;

/// <summary>
/// Login Types.
/// </summary>
public enum LoginType
{
    /// <summary>
    /// Unknown login type.
    /// </summary>
    Unknown,

    /// <summary>
    /// Password-based login.
    /// </summary>
    Password,

    /// <summary>
    /// OAuth-based login.
    /// </summary>
    OAuth,
}