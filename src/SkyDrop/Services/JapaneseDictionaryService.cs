// <copyright file="JapaneseDictionaryService.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using System.Collections.Concurrent;
using System.Text.Json;
using SkyDrop.Models;

namespace SkyDrop.Services;

/// <summary>
/// A word entry with kana reading and kanji representation.
/// </summary>
/// <param name="Kana">The kana (hiragana) reading.</param>
/// <param name="Kanji">The kanji representation (or kana if no kanji exists).</param>
/// <param name="Priority">Priority score (lower is more common).</param>
public record JapaneseWord(string Kana, string Kanji, int Priority);

/// <summary>
/// Service for Japanese kana-to-kanji conversion using JMdict-simplified.
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
    /// Loads the JMdict dictionary from the specified JSON file path.
    /// </summary>
    /// <param name="jsonPath">Path to the JMdict-simplified JSON file.</param>
    public void LoadFromJson(string jsonPath)
    {
        if (_isLoaded)
            return;

        using var stream = File.OpenRead(jsonPath);
        var dictionary = JsonSerializer.Deserialize(stream, SourceGenerationContext.Default.JmdictDictionary);

        if (dictionary?.Words == null)
            return;

        foreach (var word in dictionary.Words)
        {
            ProcessEntry(word);
        }

        _isLoaded = true;
    }

    /// <summary>
    /// Loads the dictionary from an embedded Avalonia resource.
    /// </summary>
    /// <param name="resourceUri">The avares:// URI to the JMdict-simplified JSON file.</param>
    public void LoadFromAvaloniaResource(string resourceUri)
    {
        if (_isLoaded)
            return;

        try
        {
            var uri = new Uri(resourceUri);
            using var stream = Avalonia.Platform.AssetLoader.Open(uri);

            var dictionary = JsonSerializer.Deserialize(stream, SourceGenerationContext.Default.JmdictDictionary);

            if (dictionary?.Words == null)
                return;

            foreach (var word in dictionary.Words)
            {
                ProcessEntry(word);
            }

            _isLoaded = true;
        }
        catch (Exception)
        {
            // Failed to load dictionary - service will return empty results
        }
    }

    /// <summary>
    /// Loads the dictionary asynchronously from a file path.
    /// </summary>
    public Task LoadFromJsonAsync(string jsonPath)
    {
        return Task.Run(() => LoadFromJson(jsonPath));
    }

    /// <summary>
    /// Loads the dictionary asynchronously from an Avalonia resource.
    /// </summary>
    public Task LoadFromAvaloniaResourceAsync(string resourceUri)
    {
        return Task.Run(() => LoadFromAvaloniaResource(resourceUri));
    }

    /// <summary>
    /// Processes a single JMdict entry and adds it to the lookup dictionary.
    /// </summary>
    private void ProcessEntry(JmdictWord entry)
    {
        if (entry.Kana == null || entry.Kana.Count == 0)
            return;

        // Get all kana readings
        var readings = entry.Kana
            .Where(k => !string.IsNullOrEmpty(k.Text))
            .ToList();

        // Get all kanji representations
        var kanjiList = entry.Kanji?
            .Where(k => !string.IsNullOrEmpty(k.Text))
            .ToList() ?? new List<JmdictKanji>();

        foreach (var kanaEntry in readings)
        {
            var kana = kanaEntry.Text;
            var words = new List<JapaneseWord>();

            // Calculate base priority from kana commonness
            var basePriority = kanaEntry.Common ? 10 : 500;

            if (kanjiList.Count > 0)
            {
                // Filter kanji based on appliesToKanji
                var applicableKanji = kanjiList;
                if (kanaEntry.AppliesToKanji != null && kanaEntry.AppliesToKanji.Count > 0)
                {
                    if (!kanaEntry.AppliesToKanji.Contains("*"))
                    {
                        applicableKanji = kanjiList
                            .Where(k => kanaEntry.AppliesToKanji.Contains(k.Text))
                            .ToList();
                    }
                }

                if (applicableKanji.Count > 0)
                {
                    // Add each kanji representation
                    foreach (var kanji in applicableKanji)
                    {
                        // Use the more favorable priority if kanji is also common
                        var priority = (kanaEntry.Common || kanji.Common) ? 10 : 500;
                        words.Add(new JapaneseWord(kana, kanji.Text, priority));
                    }
                }
                else
                {
                    // No applicable kanji - use kana as both reading and output
                    words.Add(new JapaneseWord(kana, kana, basePriority));
                }
            }
            else
            {
                // No kanji - use kana as both reading and output
                words.Add(new JapaneseWord(kana, kana, basePriority));
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
