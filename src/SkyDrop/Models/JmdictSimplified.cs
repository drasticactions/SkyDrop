// <copyright file="JmdictSimplified.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using System.Text.Json.Serialization;

namespace SkyDrop.Models;

/// <summary>
/// JMdict simplified dictionary root object.
/// </summary>
public class JmdictDictionary
{
    /// <summary>
    /// Gets or sets the semantic version of the jmdict-simplified project.
    /// </summary>
    [JsonPropertyName("version")]
    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the list of languages in this file.
    /// </summary>
    [JsonPropertyName("languages")]
    public List<string> Languages { get; set; } = new();

    /// <summary>
    /// Gets or sets a value indicating whether this file contains only common entries.
    /// </summary>
    [JsonPropertyName("commonOnly")]
    public bool CommonOnly { get; set; }

    /// <summary>
    /// Gets or sets the creation date of the JMdict file.
    /// </summary>
    [JsonPropertyName("dictDate")]
    public string DictDate { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the list of JMdict file revisions.
    /// </summary>
    [JsonPropertyName("dictRevisions")]
    public List<string> DictRevisions { get; set; } = new();

    /// <summary>
    /// Gets or sets the tags dictionary (tag code -> description).
    /// </summary>
    [JsonPropertyName("tags")]
    public Dictionary<string, string> Tags { get; set; } = new();

    /// <summary>
    /// Gets or sets the list of dictionary entries/words.
    /// </summary>
    [JsonPropertyName("words")]
    public List<JmdictWord> Words { get; set; } = new();
}

/// <summary>
/// A JMdict dictionary entry/word.
/// </summary>
public class JmdictWord
{
    /// <summary>
    /// Gets or sets the unique identifier of an entry.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the kanji (and other non-kana) writings.
    /// </summary>
    [JsonPropertyName("kanji")]
    public List<JmdictKanji> Kanji { get; set; } = new();

    /// <summary>
    /// Gets or sets the kana-only writings of words.
    /// </summary>
    [JsonPropertyName("kana")]
    public List<JmdictKana> Kana { get; set; } = new();

    /// <summary>
    /// Gets or sets the senses (translations + related data).
    /// </summary>
    [JsonPropertyName("sense")]
    public List<JmdictSense> Sense { get; set; } = new();
}

/// <summary>
/// Kanji writing of a word.
/// </summary>
public class JmdictKanji
{
    /// <summary>
    /// Gets or sets a value indicating whether this word is considered common.
    /// </summary>
    [JsonPropertyName("common")]
    public bool Common { get; set; }

    /// <summary>
    /// Gets or sets the word itself as spelled with non-kana-only writing.
    /// </summary>
    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the tags applicable to this writing.
    /// </summary>
    [JsonPropertyName("tags")]
    public List<string> Tags { get; set; } = new();
}

/// <summary>
/// Kana writing of a word.
/// </summary>
public class JmdictKana
{
    /// <summary>
    /// Gets or sets a value indicating whether this kana reading is considered common.
    /// </summary>
    [JsonPropertyName("common")]
    public bool Common { get; set; }

    /// <summary>
    /// Gets or sets the kana-only writing.
    /// </summary>
    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the tags applicable to this writing.
    /// </summary>
    [JsonPropertyName("tags")]
    public List<string> Tags { get; set; } = new();

    /// <summary>
    /// Gets or sets the list of kanji spellings this kana applies to.
    /// "*" means all, empty means none.
    /// </summary>
    [JsonPropertyName("appliesToKanji")]
    public List<string> AppliesToKanji { get; set; } = new();
}

/// <summary>
/// Sense of a word (translation + related data).
/// </summary>
public class JmdictSense
{
    /// <summary>
    /// Gets or sets the parts of speech for this sense.
    /// </summary>
    [JsonPropertyName("partOfSpeech")]
    public List<string> PartOfSpeech { get; set; } = new();

    /// <summary>
    /// Gets or sets the list of kanji writings this sense applies to.
    /// "*" means all.
    /// </summary>
    [JsonPropertyName("appliesToKanji")]
    public List<string> AppliesToKanji { get; set; } = new();

    /// <summary>
    /// Gets or sets the list of kana writings this sense applies to.
    /// "*" means all.
    /// </summary>
    [JsonPropertyName("appliesToKana")]
    public List<string> AppliesToKana { get; set; } = new();

    /// <summary>
    /// Gets or sets the references to related words.
    /// </summary>
    [JsonPropertyName("related")]
    public List<List<object>> Related { get; set; } = new();

    /// <summary>
    /// Gets or sets the references to antonyms.
    /// </summary>
    [JsonPropertyName("antonym")]
    public List<List<object>> Antonym { get; set; } = new();

    /// <summary>
    /// Gets or sets the list of fields of application.
    /// </summary>
    [JsonPropertyName("field")]
    public List<string> Field { get; set; } = new();

    /// <summary>
    /// Gets or sets the list of dialects where this word is used.
    /// </summary>
    [JsonPropertyName("dialect")]
    public List<string> Dialect { get; set; } = new();

    /// <summary>
    /// Gets or sets miscellaneous tags.
    /// </summary>
    [JsonPropertyName("misc")]
    public List<string> Misc { get; set; } = new();

    /// <summary>
    /// Gets or sets other information about this word.
    /// </summary>
    [JsonPropertyName("info")]
    public List<string> Info { get; set; } = new();

    /// <summary>
    /// Gets or sets source language information for borrowed words.
    /// </summary>
    [JsonPropertyName("languageSource")]
    public List<JmdictLanguageSource> LanguageSource { get; set; } = new();

    /// <summary>
    /// Gets or sets the translations of this word.
    /// </summary>
    [JsonPropertyName("gloss")]
    public List<JmdictGloss> Gloss { get; set; } = new();
}

/// <summary>
/// Source language information for borrowed words.
/// </summary>
public class JmdictLanguageSource
{
    /// <summary>
    /// Gets or sets the language code (ISO 639-2, 3 letters).
    /// </summary>
    [JsonPropertyName("lang")]
    public string Lang { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether this fully describes the source.
    /// </summary>
    [JsonPropertyName("full")]
    public bool Full { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the word is wasei-eigo.
    /// </summary>
    [JsonPropertyName("wasei")]
    public bool Wasei { get; set; }

    /// <summary>
    /// Gets or sets the text in the source language, or null.
    /// </summary>
    [JsonPropertyName("text")]
    public string? Text { get; set; }
}

/// <summary>
/// Translation of a word.
/// </summary>
public class JmdictGloss
{
    /// <summary>
    /// Gets or sets the language code (ISO 639-2, 3 letters).
    /// </summary>
    [JsonPropertyName("lang")]
    public string Lang { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the gender (masculine, feminine, neuter) or null.
    /// </summary>
    [JsonPropertyName("gender")]
    public string? Gender { get; set; }

    /// <summary>
    /// Gets or sets the type of translation (literal, figurative, explanation, trademark) or null.
    /// </summary>
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    /// <summary>
    /// Gets or sets the translation word/phrase.
    /// </summary>
    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;
}
