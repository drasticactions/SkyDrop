using System.Text.RegularExpressions;

namespace DaT9;

/// <summary>
/// Helper methods for T9 engine state management.
/// </summary>
public static partial class T9Helpers
{
    /// <summary>
    /// Matches the engine state to a given word.
    /// </summary>
    /// <param name="engine">The T9 engine.</param>
    /// <param name="wordToMatch">The word to match.</param>
    public static void MatchEngineToWord(T9Engine engine, string wordToMatch)
    {
        foreach (var c in wordToMatch)
        {
            if (c == '\'')
            {
                continue;
            }

            if (!T9Engine.T9Mapping.TryGetValue(char.ToLower(c), out var digit))
            {
                continue;
            }

            if (digit == 1 && !engine.HandlePunctuation)
            {
                engine.NewCompletion();
                engine.HandlePunctuation = true;
            }
            else if (engine.HandlePunctuation && digit != 1)
            {
                engine.NewCompletion();
                engine.HandlePunctuation = false;
            }

            try
            {
                engine.AddDigit(digit);
            }
            catch (WordNotFoundException)
            {
                engine.NewCompletion();
            }
        }
    }

    /// <summary>
    /// Determines the capitalization mode based on the current line.
    /// </summary>
    /// <param name="engine">The T9 engine.</param>
    /// <param name="line">The current line of text.</param>
    public static void DetermineCapitalization(T9Engine engine, string line)
    {
        if (line.Length == 0)
        {
            engine.CurrentCaseMode = T9Engine.CaseMode.Capitalize;
            return;
        }

        for (var i = line.Length - 1; i >= 0; i--)
        {
            var c = line[i];
            if (c == ' ')
            {
                continue;
            }

            if (c is '.' or '!' or '?')
            {
                engine.CurrentCaseMode = T9Engine.CaseMode.Capitalize;
            }
            else
            {
                engine.CurrentCaseMode = T9Engine.CaseMode.Normal;
            }
            break;
        }
    }

    /// <summary>
    /// Recalculates the engine state based on the current line.
    /// </summary>
    /// <param name="engine">The T9 engine.</param>
    /// <param name="line">The current line of text (will be modified).</param>
    /// <returns>The modified line.</returns>
    public static string RecalculateState(T9Engine engine, string line)
    {
        // Line is already empty
        if (line.Length == 0)
        {
            engine.NewCompletion();
            engine.CurrentCaseMode = T9Engine.CaseMode.Capitalize;
            return line;
        }

        // Determine the word we should try to match with the engine
        var parts = line.Split(' ');
        var wordToMatch = parts[^1];

        if (wordToMatch.Length == 0)
        {
            DetermineCapitalization(engine, line);
            return line;
        }

        // Split by sentence-ending punctuation
        var punctuationMatches = SentencePunctuationRegex().Split(wordToMatch)
            .Where(s => !string.IsNullOrEmpty(s))
            .ToArray();
        if (punctuationMatches.Length != 0)
        {
            wordToMatch = punctuationMatches[^1];
        }

        // Split by alphanumeric sequences
        var alphaMatches = AlphanumericRegex().Split(wordToMatch)
            .Where(s => !string.IsNullOrEmpty(s))
            .ToArray();
        if (alphaMatches.Length != 0)
        {
            wordToMatch = alphaMatches[^1];
        }

        engine.HandlePunctuation = false;
        MatchEngineToWord(engine, wordToMatch);

        if (engine.NumEngineChars == 0)
        {
            DetermineCapitalization(engine, line);
            return line;
        }

        // Try to recover the completion choice that was made
        var tryCaseless = false;
        try
        {
            while (engine.GetCompletion() != wordToMatch)
            {
                engine.NextCompletion();
            }
        }
        catch (WordNotFoundException)
        {
            engine.ResetCompletionChoice();
            tryCaseless = true;
        }

        // Weird capitalization can make the above search fail, so let's try again ignoring case
        if (tryCaseless)
        {
            try
            {
                while (!string.Equals(engine.GetCompletion(), wordToMatch, StringComparison.OrdinalIgnoreCase))
                {
                    engine.NextCompletion();
                }
            }
            catch (WordNotFoundException)
            {
                // If we can't find the word at all, just bail
                engine.NewCompletion();
                DetermineCapitalization(engine, line);
                return line;
            }
        }

        // Now that the word is loaded into the engine, remove it from the line
        var completionLen = engine.CurrentCompletionLength;
        if (completionLen > 0 && line.Length >= completionLen)
        {
            line = line[..^completionLen];
        }

        // Should we capitalize the word?
        if (!engine.HandlePunctuation)
        {
            DetermineCapitalization(engine, line);
        }

        return line;
    }

    [GeneratedRegex(@"([.!?]+)")]
    private static partial Regex SentencePunctuationRegex();

    [GeneratedRegex(@"([a-zA-Z'1-9]+)")]
    private static partial Regex AlphanumericRegex();
}
