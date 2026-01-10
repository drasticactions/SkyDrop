using System.Text.RegularExpressions;

namespace SkyDrop.Services;

/// <summary>
/// Utility class for processing post text, including URL detection and markdown conversion.
/// </summary>
public static partial class PostTextProcessor
{
    // Regex to match URLs (http, https, or www.)
    // Allow most URL characters including query strings, fragments, and encoded characters
    [GeneratedRegex(@"(https?://[^\s\[\]<>""]+|www\.[^\s\[\]<>""]+)", RegexOptions.IgnoreCase)]
    private static partial Regex UrlRegex();

    // Regex to detect if a URL is already part of a markdown link: [text](url) or preceded by ](
    [GeneratedRegex(@"\]\(\s*$")]
    private static partial Regex MarkdownLinkPrefixRegex();

    // Regex to match existing markdown links to skip them
    [GeneratedRegex(@"\[([^\]]*)\]\(([^\)]+)\)")]
    private static partial Regex ExistingMarkdownLinkRegex();

    /// <summary>
    /// Converts plain URLs in text to markdown links, while preserving existing markdown links.
    /// URLs starting with www. will have https:// prepended in the link target.
    /// </summary>
    /// <param name="text">The input text that may contain plain URLs and/or markdown links.</param>
    /// <returns>Text with plain URLs converted to markdown format [displayUrl](fullUrl).</returns>
    /// <example>
    /// Input: "Check out https://example.com for more"
    /// Output: "Check out [https://example.com](https://example.com) for more"
    ///
    /// Input: "Visit www.google.com today"
    /// Output: "Visit [www.google.com](https://www.google.com) today"
    ///
    /// Input: "Visit [my site](https://example.com)"
    /// Output: "Visit [my site](https://example.com)" (unchanged)
    /// </example>
    public static string ConvertUrlsToMarkdown(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        // First, find all existing markdown links and their positions to avoid modifying them
        var existingLinks = new HashSet<(int Start, int End)>();
        foreach (Match match in ExistingMarkdownLinkRegex().Matches(text))
        {
            existingLinks.Add((match.Index, match.Index + match.Length));
        }

        // Process URLs from end to start to preserve indices during replacement
        var urlMatches = UrlRegex().Matches(text).Cast<Match>().OrderByDescending(m => m.Index).ToList();

        foreach (var match in urlMatches)
        {
            var url = match.Value;
            var startIndex = match.Index;
            var endIndex = startIndex + match.Length;

            // Check if this URL is inside an existing markdown link
            bool isInsideMarkdownLink = existingLinks.Any(link =>
                startIndex >= link.Start && endIndex <= link.End);

            if (isInsideMarkdownLink)
                continue;

            // Check if preceded by ]( which indicates it's part of a markdown link URL
            if (startIndex >= 2)
            {
                var prefixStart = Math.Max(0, startIndex - 10);
                var prefixLength = Math.Min(10, startIndex);
                var prefix = text.AsSpan(prefixStart, prefixLength);
                if (MarkdownLinkPrefixRegex().IsMatch(prefix))
                    continue;
            }

            // Clean up the URL - remove trailing punctuation that's likely not part of the URL
            // Note: Don't trim ':' as it can be part of port numbers or URL-encoded values
            var cleanUrl = url.TrimEnd('.', ',', '!', '?', ';', '\'', '"');
            var trailingChars = url.AsSpan(cleanUrl.Length);

            // Determine the display URL and the actual link URL
            // For www. URLs without protocol, add https:// to the link
            var displayUrl = cleanUrl;
            var linkUrl = cleanUrl.StartsWith("www.", StringComparison.OrdinalIgnoreCase)
                ? $"https://{cleanUrl}"
                : cleanUrl;

            // Escape parentheses in the URL for markdown compatibility
            var escapedLinkUrl = linkUrl.Replace("(", "%28").Replace(")", "%29");

            // Convert to markdown link
            var markdownLink = $"[{displayUrl}]({escapedLinkUrl}){trailingChars}";
            text = string.Concat(text.AsSpan(0, startIndex), markdownLink, text.AsSpan(endIndex));
        }

        return text;
    }

    /// <summary>
    /// Processes post text by converting plain URLs to markdown links.
    /// This prepares the text for use with MarkdownPost.Parse which will extract facets.
    /// </summary>
    /// <param name="text">The raw post text.</param>
    /// <returns>Text with URLs converted to markdown format.</returns>
    public static string PreparePostText(string text)
    {
        return ConvertUrlsToMarkdown(text);
    }
}
