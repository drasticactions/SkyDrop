namespace DaT9;

/// <summary>
/// T9 predictive text engine that provides word completion based on numeric key sequences.
/// </summary>
public class T9Engine
{
    /// <summary>
    /// Case mode for output text.
    /// </summary>
    public enum CaseMode
    {
        Normal = 0,
        Capitalize = 1,
        Upper = 2
    }

    /// <summary>
    /// T9 key mapping from characters to digits.
    /// </summary>
    public static readonly IReadOnlyDictionary<char, int> T9Mapping = new Dictionary<char, int>
    {
        { '.', 1 }, { ',', 1 }, { '!', 1 }, { '?', 1 },
        { ':', 1 }, { '-', 1 }, { '_', 1 }, { '\'', 1 }, { '/', 1 }, { '*', 1 },
        { '\\', 1 }, { '(', 1 }, { ')', 1 }, { '<', 1 }, { '>', 1 },
        { ';', 1 }, { '[', 1 }, { ']', 1 },
        { 'a', 2 }, { 'b', 2 }, { 'c', 2 },
        { 'd', 3 }, { 'e', 3 }, { 'f', 3 },
        { 'g', 4 }, { 'h', 4 }, { 'i', 4 },
        { 'j', 5 }, { 'k', 5 }, { 'l', 5 },
        { 'm', 6 }, { 'n', 6 }, { 'o', 6 },
        { 'p', 7 }, { 'q', 7 }, { 'r', 7 }, { 's', 7 },
        { 't', 8 }, { 'u', 8 }, { 'v', 8 },
        { 'w', 9 }, { 'x', 9 }, { 'y', 9 }, { 'z', 9 },
        { '0', 0 }, { '1', 1 }, { '2', 2 }, { '3', 3 },
        { '4', 4 }, { '5', 5 }, { '6', 6 }, { '7', 7 },
        { '8', 8 }, { '9', 9 }
    };

    private readonly TrieNode _lookup = new();
    private TrieNode _current;
    private readonly Stack<TrieNode> _history = new();
    private int _completionLength;
    private int _completionChoice;
    private CaseMode _caseMode = CaseMode.Capitalize;
    private bool _handlePunctuation;

    public T9Engine()
    {
        _current = _lookup;
    }

    /// <summary>
    /// Adds a word to the T9 dictionary.
    /// </summary>
    /// <param name="word">The word to add.</param>
    /// <returns>True if the word was added, false if it already exists or contains invalid characters.</returns>
    public bool AddWord(string word, int frequency = 0)
    {
        var cur = _lookup;

        // Words containing '1' are not supported
        if (word.Contains('1'))
        {
            return false;
        }

        foreach (var c in word)
        {
            if (c == '\'')
            {
                continue;
            }

            if (!T9Mapping.TryGetValue(char.ToLower(c), out var num))
            {
                return false;
            }

            if (!cur.Children.TryGetValue(num, out var child))
            {
                child = new TrieNode();
                cur.Children[num] = child;
            }
            cur = child;
        }

        // Check if word already exists
        for (int i = 0; i < cur.Words.Count; i++)
        {
            if (cur.Words[i].Word == word)
            {
                return false;
            }
        }

        // Insert word in frequency order (higher frequency first)
        int insertIndex = 0;
        for (int i = 0; i < cur.Words.Count; i++)
        {
            if (cur.Words[i].Frequency >= frequency)
            {
                insertIndex = i + 1;
            }
            else
            {
                break;
            }
        }
        cur.Words.Insert(insertIndex, (word, frequency));
        return true;
    }

    /// <summary>
    /// Loads a dictionary file, adding each word to the T9 engine.
    /// </summary>
    /// <param name="filename">Path to the dictionary file.</param>
    public void LoadDictionary(string filename)
    {
        foreach (var line in File.ReadLines(filename))
        {
            var parts = line.Split('\t');
            var word = parts[0].Trim();
            if (string.IsNullOrEmpty(word))
            {
                continue;
            }

            int frequency = 0;
            if (parts.Length > 1 && int.TryParse(parts[1].Trim(), out var freq))
            {
                frequency = freq;
            }

            AddWord(word, frequency);
        }
    }


    /// <summary>
    /// Loads the dictionary from a stream containing tab-delimited word,frequency pairs.
    /// </summary>
    /// <param name="stream">The stream to read from.</param>
    public void LoadDictionaryFromStream(Stream stream)
    {
        using var reader = new StreamReader(stream);
        while (reader.ReadLine() is { } line)
        {
            var parts = line.Split('\t');
            var word = parts[0].Trim();
            if (string.IsNullOrEmpty(word))
            {
                continue;
            }

            int frequency = 0;
            if (parts.Length > 1 && int.TryParse(parts[1].Trim(), out var freq))
            {
                frequency = freq;
            }

            AddWord(word, frequency);
        }
    }

    /// <summary>
    /// Adds a digit to the current T9 sequence.
    /// </summary>
    /// <param name="digit">The digit (0-9) to add.</param>
    /// <returns>The current completion string.</returns>
    /// <exception cref="WordNotFoundException">Thrown when no words match the current sequence.</exception>
    public string AddDigit(int digit)
    {
        if (!_current.Children.TryGetValue(digit, out var child))
        {
            throw new WordNotFoundException();
        }

        _completionLength++;
        _history.Push(_current);
        _current = child;
        _completionChoice = 0;
        return GetCompletion();
    }

    /// <summary>
    /// Removes the last digit from the current T9 sequence.
    /// </summary>
    /// <returns>The current completion string, or null if the sequence is empty.</returns>
    public string? Backspace()
    {
        _completionChoice = 0;
        if (_history.Count > 0)
        {
            _current = _history.Pop();
        }
        _completionLength = Math.Max(_completionLength - 1, 0);
        return GetCompletion();
    }

    /// <summary>
    /// Gets the current word completion based on the entered digits.
    /// </summary>
    /// <returns>The completion string, or empty string if no completion is available.</returns>
    public string GetCompletion()
    {
        if (_completionLength == 0)
        {
            return string.Empty;
        }

        var candidate = _current;
        while (candidate != null)
        {
            if (candidate.Words.Count > 0)
            {
                var word = candidate.Words[_completionChoice % candidate.Words.Count].Word;
                return _caseMode switch
                {
                    CaseMode.Capitalize when word.Length > 0 => char.ToUpper(word[0]) + word[1..],
                    CaseMode.Upper => word.ToUpper(),
                    _ => word
                };
            }

            // Find the first child to continue searching
            foreach (var child in candidate.Children.Values)
            {
                candidate = child;
                break;
            }

            if (candidate.Children.Count == 0 && candidate.Words.Count == 0)
            {
                break;
            }
        }

        return string.Empty;
    }

    /// <summary>
    /// Cycles to the next available completion for the current digit sequence.
    /// </summary>
    /// <exception cref="WordNotFoundException">Thrown when there are no more completions available.</exception>
    public void NextCompletion()
    {
        if (_completionLength == 0)
        {
            return;
        }

        if (_current.Words.Count == 0 || (_completionChoice + 1) >= _current.Words.Count)
        {
            throw new WordNotFoundException();
        }

        _completionChoice++;
    }

    /// <summary>
    /// Resets the completion choice to the first available word.
    /// </summary>
    public void ResetCompletionChoice()
    {
        _completionChoice = 0;
    }

    /// <summary>
    /// Resets the engine for a new word completion.
    /// </summary>
    public void NewCompletion()
    {
        _current = _lookup;
        _history.Clear();
        _completionChoice = 0;
        _completionLength = 0;
    }

    /// <summary>
    /// Gets the length of the current completion string.
    /// </summary>
    public int CurrentCompletionLength => GetCompletion().Length;

    /// <summary>
    /// Gets the number of digits entered for the current completion.
    /// </summary>
    public int NumEngineChars => _completionLength;

    /// <summary>
    /// Gets or sets the current case mode.
    /// </summary>
    public CaseMode CurrentCaseMode
    {
        get => _caseMode;
        set => _caseMode = value;
    }

    /// <summary>
    /// Cycles through the available case modes.
    /// </summary>
    public void CycleCaseMode()
    {
        _caseMode = (CaseMode)(((int)_caseMode + 1) % 3);
    }

    /// <summary>
    /// Gets or sets whether the engine is currently processing punctuation.
    /// </summary>
    public bool HandlePunctuation
    {
        get => _handlePunctuation;
        set => _handlePunctuation = value;
    }

    /// <summary>
    /// Converts text to a T9 key sequence.
    /// </summary>
    /// <param name="text">The text to convert.</param>
    /// <returns>A result containing the T9 sequence and metadata about the conversion.</returns>
    public static TextToSequenceResult TextToSequence(string text)
    {
        var sequence = new List<string>();
        var steps = new List<SequenceStep>();

        var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        for (var wordIndex = 0; wordIndex < words.Length; wordIndex++)
        {
            var word = words[wordIndex];
            var wordSequence = new System.Text.StringBuilder();

            foreach (var c in word)
            {
                if (c == '\'')
                {
                    continue; // Apostrophes are skipped in T9
                }

                if (T9Mapping.TryGetValue(char.ToLower(c), out var digit))
                {
                    wordSequence.Append(digit);
                }
            }

            if (wordSequence.Length > 0)
            {
                var seq = wordSequence.ToString();
                sequence.Add(seq);
                steps.Add(new SequenceStep(word, seq, wordIndex < words.Length - 1));
            }
        }

        // Join with '0' for spaces
        var fullSequence = string.Join("0", sequence);

        return new TextToSequenceResult(text, fullSequence, steps);
    }

    /// <summary>
    /// Plays back a T9 sequence and returns the resulting text.
    /// </summary>
    /// <param name="sequence">The T9 digit sequence (e.g., "43556" for "hello").</param>
    /// <param name="tabPresses">Optional dictionary mapping position to number of tab presses for alternate words.</param>
    /// <returns>A result containing the generated text and playback details.</returns>
    public PlaySequenceResult PlaySequence(string sequence, Dictionary<int, int>? tabPresses = null)
    {
        tabPresses ??= new Dictionary<int, int>();

        var result = new System.Text.StringBuilder();
        var steps = new List<PlaybackStep>();
        var wordStart = 0;

        NewCompletion();
        CurrentCaseMode = CaseMode.Capitalize;
        HandlePunctuation = false;

        for (var i = 0; i < sequence.Length; i++)
        {
            var c = sequence[i];
            if (!char.IsDigit(c))
            {
                continue;
            }

            var digit = c - '0';

            if (digit == 0)
            {
                // Space - accept current completion
                var completion = GetCompletion();
                result.Append(completion);
                result.Append(' ');

                steps.Add(new PlaybackStep(
                    sequence[wordStart..i],
                    completion,
                    PlaybackStepType.WordComplete));

                NewCompletion();
                if (completion.Length > 0 && completion is "." or "!" or "?")
                {
                    CurrentCaseMode = CaseMode.Capitalize;
                }
                else
                {
                    CurrentCaseMode = CaseMode.Normal;
                }
                HandlePunctuation = false;
                wordStart = i + 1;
            }
            else if (digit == 1)
            {
                // Punctuation
                if (!HandlePunctuation)
                {
                    var prevCompletion = GetCompletion();
                    if (prevCompletion.Length > 0)
                    {
                        result.Append(prevCompletion);
                        steps.Add(new PlaybackStep(
                            sequence[wordStart..i],
                            prevCompletion,
                            PlaybackStepType.WordComplete));
                        wordStart = i;
                    }
                    HandlePunctuation = true;
                    NewCompletion();
                }

                try
                {
                    AddDigit(digit);
                }
                catch (WordNotFoundException)
                {
                    steps.Add(new PlaybackStep(
                        sequence[wordStart..(i + 1)],
                        "?",
                        PlaybackStepType.NotFound));
                }
            }
            else
            {
                // Normal letter input
                if (HandlePunctuation)
                {
                    var punctuation = GetCompletion();
                    if (punctuation.Length > 0)
                    {
                        result.Append(punctuation);
                        steps.Add(new PlaybackStep(
                            sequence[wordStart..i],
                            punctuation,
                            PlaybackStepType.Punctuation));

                        if (punctuation is "." or "!" or "?")
                        {
                            CurrentCaseMode = CaseMode.Capitalize;
                        }
                        wordStart = i;
                    }
                    HandlePunctuation = false;
                    NewCompletion();
                }

                try
                {
                    AddDigit(digit);

                    // Apply tab presses for this position if specified
                    if (tabPresses.TryGetValue(i, out var presses))
                    {
                        for (var t = 0; t < presses; t++)
                        {
                            try
                            {
                                NextCompletion();
                            }
                            catch (WordNotFoundException)
                            {
                                break;
                            }
                        }
                    }
                }
                catch (WordNotFoundException)
                {
                    steps.Add(new PlaybackStep(
                        sequence[wordStart..(i + 1)],
                        "?",
                        PlaybackStepType.NotFound));
                }
            }
        }

        // Handle any remaining completion
        var finalCompletion = GetCompletion();
        if (finalCompletion.Length > 0)
        {
            result.Append(finalCompletion);
            steps.Add(new PlaybackStep(
                sequence[wordStart..],
                finalCompletion,
                PlaybackStepType.WordComplete));
        }

        NewCompletion();
        CurrentCaseMode = CaseMode.Capitalize;

        return new PlaySequenceResult(sequence, result.ToString().TrimEnd(), steps);
    }

    /// <summary>
    /// Gets all possible completions for a given T9 sequence.
    /// </summary>
    /// <param name="sequence">The T9 digit sequence.</param>
    /// <returns>List of possible word completions.</returns>
    public List<string> GetAllCompletions(string sequence)
    {
        var completions = new List<string>();

        NewCompletion();

        try
        {
            foreach (var c in sequence)
            {
                if (!char.IsDigit(c) || c == '0')
                {
                    break;
                }
                AddDigit(c - '0');
            }

            // Collect all completions
            ResetCompletionChoice();
            while (true)
            {
                var completion = GetCompletion();
                if (string.IsNullOrEmpty(completion))
                {
                    break;
                }
                
                if (!completions.Contains(completion))
                {
                    completions.Add(completion);
                }
                try
                {
                    NextCompletion();
                }
                catch (WordNotFoundException)
                {
                    break;
                }
            }
        }
        catch (WordNotFoundException)
        {
            // No completions found
        }

        NewCompletion();
        return completions;
    }


    /// <summary>
    /// Checks if a word exists in the dictionary.
    /// </summary>
    /// <param name="word">The word to check.</param>
    /// <returns>True if the word is in the dictionary, false otherwise.</returns>
    public bool IsWordInDictionary(string word)
    {
        if (string.IsNullOrWhiteSpace(word))
        {
            return false;
        }

        // Strip leading and trailing punctuation
        var cleanedWord = word.Trim();
        while (cleanedWord.Length > 0 && char.IsPunctuation(cleanedWord[0]))
        {
            cleanedWord = cleanedWord[1..];
        }
        while (cleanedWord.Length > 0 && char.IsPunctuation(cleanedWord[^1]))
        {
            cleanedWord = cleanedWord[..^1];
        }

        if (string.IsNullOrEmpty(cleanedWord))
        {
            return false;
        }

        // Convert word to T9 sequence
        var sequence = new System.Text.StringBuilder();
        foreach (var c in cleanedWord)
        {
            if (c == '\'')
            {
                continue;
            }

            if (T9Mapping.TryGetValue(char.ToLower(c), out var digit))
            {
                sequence.Append(digit);
            }
        }

        if (sequence.Length == 0)
        {
            return false;
        }

        // Get all completions for this sequence and check if the word is among them
        var completions = GetAllCompletions(sequence.ToString());
        var normalizedWord = cleanedWord.ToLowerInvariant().Replace("'", "");
        
        return completions.Any(c => c.ToLowerInvariant().Replace("'", "") == normalizedWord);
    }

    /// <summary>
    /// Represents a node in the T9 trie structure.
    /// </summary>
    private class TrieNode
    {
        public Dictionary<int, TrieNode> Children { get; } = new();
        public List<(string Word, int Frequency)> Words { get; } = new();
    }
}
