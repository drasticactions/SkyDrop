using System.Collections.Concurrent;
using JMDict;

namespace SkyDrop.Services;

/// <summary>
/// A word entry with kana reading and kanji representation.
/// </summary>
/// <param name="Kana">The kana (hiragana) reading.</param>
/// <param name="Kanji">The kanji representation (or kana if no kanji exists).</param>
/// <param name="Priority">Priority score (lower is more common).</param>
public record JapaneseWord(string Kana, string Kanji, int Priority);

/// <summary>
/// Service for Japanese kana-to-kanji conversion using JMdict.
/// </summary>
public class JapaneseDictionaryService
{
    private readonly ConcurrentDictionary<string, List<JapaneseWord>> _kanaToWords = new();
    private bool _isLoaded;

    /// <summary>
    /// Gets whether the dictionary has been loaded.
    /// </summary>
    public bool IsLoaded => _isLoaded;

    /// <summary>
    /// Loads the JMdict dictionary from the specified XML file path.
    /// </summary>
    /// <param name="xmlPath">Path to the JMdict XML file.</param>
    public void LoadFromJmdict(string xmlPath)
    {
        if (_isLoaded)
            return;

        var parser = new DictParser();
        var jmdict = parser.ParseXml<Jmdict>(xmlPath);

        if (jmdict?.Entries == null)
            return;

        foreach (var entry in jmdict.Entries)
        {
            ProcessEntry(entry);
        }

        _isLoaded = true;
    }

    /// <summary>
    /// Loads the dictionary from an embedded Avalonia resource.
    /// </summary>
    /// <param name="resourceUri">The avares:// URI to the JMdict XML file.</param>
    public void LoadFromAvaloniaResource(string resourceUri)
    {
        if (_isLoaded)
            return;

        try
        {
            var uri = new Uri(resourceUri);
            using var stream = Avalonia.Platform.AssetLoader.Open(uri);

            // JMDict parser needs a file path, so we need to copy to temp file
            var tempPath = Path.GetTempFileName();
            try
            {
                using (var fileStream = File.Create(tempPath))
                {
                    // Check if the resource is gzipped
                    if (resourceUri.EndsWith(".gz", StringComparison.OrdinalIgnoreCase))
                    {
                        using var gzipStream = new System.IO.Compression.GZipStream(
                            stream, System.IO.Compression.CompressionMode.Decompress);
                        gzipStream.CopyTo(fileStream);
                    }
                    else
                    {
                        stream.CopyTo(fileStream);
                    }
                }
                LoadFromJmdict(tempPath);
            }
            finally
            {
                if (File.Exists(tempPath))
                    File.Delete(tempPath);
            }
        }
        catch (Exception)
        {
            // Failed to load dictionary - service will return empty results
        }
    }

    /// <summary>
    /// Loads the dictionary asynchronously from a file path.
    /// </summary>
    public Task LoadFromJmdictAsync(string xmlPath)
    {
        return Task.Run(() => LoadFromJmdict(xmlPath));
    }

    /// <summary>
    /// Processes a single JMdict entry and adds it to the lookup dictionary.
    /// </summary>
    private void ProcessEntry(JmdictEntry entry)
    {
        if (entry.Readings == null)
            return;

        // Get all kana readings
        var readings = entry.Readings
            .Where(r => r.Kana != null)
            .Select(r => r.Kana!)
            .ToList();

        // Get all kanji representations (or use kana if none)
        var kanjiList = entry.Kanji?
            .Where(k => k.Expression != null)
            .Select(k => k.Expression!)
            .ToList() ?? new List<string>();

        // Calculate priority (lower = more common)
        // Priority markers in JMdict: news1/2, ichi1/2, spec1/2, gai1/2, nf01-48
        var priority = CalculatePriority(entry);

        foreach (var kana in readings)
        {
            var words = new List<JapaneseWord>();

            if (kanjiList.Count > 0)
            {
                // Add each kanji representation
                foreach (var kanji in kanjiList)
                {
                    words.Add(new JapaneseWord(kana, kanji, priority));
                }
            }
            else
            {
                // No kanji - use kana as both reading and output
                words.Add(new JapaneseWord(kana, kana, priority));
            }

            // Add to dictionary, merging with existing entries
            _kanaToWords.AddOrUpdate(
                kana,
                words,
                (_, existing) =>
                {
                    existing.AddRange(words);
                    return existing;
                });
        }
    }

    /// <summary>
    /// Calculates a priority score for an entry (lower = more common).
    /// </summary>
    private static int CalculatePriority(JmdictEntry entry)
    {
        // Default priority for uncommon words
        var priority = 1000;

        // Check reading element priorities
        if (entry.Readings != null)
        {
            foreach (var reading in entry.Readings)
            {
                if (reading.Priorities != null)
                {
                    foreach (var p in reading.Priorities)
                    {
                        priority = Math.Min(priority, GetPriorityValue(p));
                    }
                }
            }
        }

        // Check kanji element priorities
        if (entry.Kanji != null)
        {
            foreach (var kanji in entry.Kanji)
            {
                if (kanji.Priorities != null)
                {
                    foreach (var p in kanji.Priorities)
                    {
                        priority = Math.Min(priority, GetPriorityValue(p));
                    }
                }
            }
        }

        return priority;
    }

    /// <summary>
    /// Converts a JMdict priority string to a numeric value.
    /// </summary>
    private static int GetPriorityValue(string priority)
    {
        // news1, ichi1, spec1, gai1 = most common (10)
        // news2, ichi2, spec2, gai2 = common (50)
        // nf01-nf10 = very common (1-10)
        // nf11-nf48 = common (11-48)
        return priority switch
        {
            "news1" or "ichi1" or "spec1" or "gai1" => 10,
            "news2" or "ichi2" or "spec2" or "gai2" => 50,
            _ when priority.StartsWith("nf") && int.TryParse(priority[2..], out var nf) => nf,
            _ => 500
        };
    }

    /// <summary>
    /// Looks up words by exact kana reading.
    /// </summary>
    /// <param name="kana">The kana reading to look up.</param>
    /// <returns>List of matching words, sorted by priority.</returns>
    public IReadOnlyList<JapaneseWord> LookupExact(string kana)
    {
        if (!_isLoaded || string.IsNullOrEmpty(kana))
            return Array.Empty<JapaneseWord>();

        if (_kanaToWords.TryGetValue(kana, out var words))
        {
            return words.OrderBy(w => w.Priority).ToList();
        }

        return Array.Empty<JapaneseWord>();
    }

    /// <summary>
    /// Looks up words by kana prefix (for predictive input).
    /// </summary>
    /// <param name="kanaPrefix">The kana prefix to search for.</param>
    /// <param name="maxResults">Maximum number of results to return.</param>
    /// <returns>List of matching words, sorted by priority.</returns>
    public IReadOnlyList<JapaneseWord> LookupByPrefix(string kanaPrefix, int maxResults = 10)
    {
        if (!_isLoaded || string.IsNullOrEmpty(kanaPrefix))
            return Array.Empty<JapaneseWord>();

        var results = new List<JapaneseWord>();

        // First, add exact matches
        if (_kanaToWords.TryGetValue(kanaPrefix, out var exactMatches))
        {
            results.AddRange(exactMatches);
        }

        // Then add prefix matches
        foreach (var kvp in _kanaToWords)
        {
            if (kvp.Key.StartsWith(kanaPrefix) && kvp.Key != kanaPrefix)
            {
                results.AddRange(kvp.Value);
            }
        }

        return results
            .OrderBy(w => w.Priority)
            .ThenBy(w => w.Kana.Length) // Prefer shorter words
            .Take(maxResults)
            .ToList();
    }

    /// <summary>
    /// Gets the number of unique kana readings in the dictionary.
    /// </summary>
    public int Count => _kanaToWords.Count;
}
