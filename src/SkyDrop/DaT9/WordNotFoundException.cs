namespace DaT9;

/// <summary>
/// Exception thrown when a word cannot be found in the T9 dictionary.
/// </summary>
public class WordNotFoundException : Exception
{
    public WordNotFoundException() : base() { }
    public WordNotFoundException(string message) : base(message) { }
    public WordNotFoundException(string message, Exception innerException) : base(message, innerException) { }
}
