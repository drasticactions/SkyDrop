namespace DaT9;

/// <summary>
/// Result of converting text to a T9 sequence.
/// </summary>
/// <param name="OriginalText">The original input text.</param>
/// <param name="Sequence">The full T9 digit sequence.</param>
/// <param name="Steps">Individual conversion steps for each word.</param>
public record TextToSequenceResult(
    string OriginalText,
    string Sequence,
    IReadOnlyList<SequenceStep> Steps);

/// <summary>
/// A single step in the text-to-sequence conversion.
/// </summary>
/// <param name="Word">The word being converted.</param>
/// <param name="Sequence">The T9 sequence for this word.</param>
/// <param name="FollowedBySpace">Whether this word is followed by a space.</param>
public record SequenceStep(
    string Word,
    string Sequence,
    bool FollowedBySpace);

/// <summary>
/// Result of playing back a T9 sequence.
/// </summary>
/// <param name="Sequence">The input T9 sequence.</param>
/// <param name="GeneratedText">The text generated from the sequence.</param>
/// <param name="Steps">Individual playback steps.</param>
public record PlaySequenceResult(
    string Sequence,
    string GeneratedText,
    IReadOnlyList<PlaybackStep> Steps);

/// <summary>
/// Type of playback step.
/// </summary>
public enum PlaybackStepType
{
    /// <summary>A complete word was recognized.</summary>
    WordComplete,
    /// <summary>Punctuation was recognized.</summary>
    Punctuation,
    /// <summary>The sequence was not found in the dictionary.</summary>
    NotFound
}

/// <summary>
/// A single step in sequence playback.
/// </summary>
/// <param name="Sequence">The T9 sequence for this step.</param>
/// <param name="Output">The output text produced.</param>
/// <param name="Type">The type of step.</param>
public record PlaybackStep(
    string Sequence,
    string Output,
    PlaybackStepType Type);
