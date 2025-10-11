using System.Collections.Immutable;
using System.Text.RegularExpressions;
using CiteUrl.Core.Models;

namespace CiteUrl.Core.Templates;

/// <summary>
/// Interface for citation extraction and link insertion.
/// Provides DI abstraction for Citator implementations.
/// </summary>
public interface ICitator
{
    /// <summary>
    /// Immutable dictionary of templates keyed by name.
    /// Thread-safe for concurrent access.
    /// </summary>
    ImmutableDictionary<string, Template> Templates { get; }

    /// <summary>
    /// Finds the first citation in the text.
    /// </summary>
    /// <param name="text">Text to search for citations.</param>
    /// <param name="broad">If true, uses both broad and normal regexes. Default true.</param>
    /// <returns>First citation found, or null if none.</returns>
    Citation? Cite(string text, bool broad = true);

    /// <summary>
    /// Finds all citations in the text, including shortforms and idforms.
    /// Returns a streaming enumerable for memory efficiency (Gap Decision #6).
    /// </summary>
    /// <param name="text">Text to search for citations.</param>
    /// <param name="idBreaks">Optional regex to break idform chains.</param>
    /// <returns>Streaming enumerable of all citations found.</returns>
    IEnumerable<Citation> ListCitations(string text, Regex? idBreaks = null);

    /// <summary>
    /// Inserts hyperlinks for all citations in the text.
    /// </summary>
    /// <param name="text">Text containing citations.</param>
    /// <param name="attrs">Optional HTML attributes for links.</param>
    /// <param name="addTitle">If true, adds title attribute with citation name.</param>
    /// <param name="urlOptional">If true, allows citations without URLs.</param>
    /// <param name="redundantLinks">If true, creates links for idforms even if parent has same URL.</param>
    /// <param name="idBreaks">Optional regex to break idform chains.</param>
    /// <param name="ignoreMarkup">If true, ignores existing markup.</param>
    /// <param name="markupFormat">Markup format: "html", "markdown", or "plaintext".</param>
    /// <returns>Text with hyperlinks inserted.</returns>
    string InsertLinks(
        string text,
        Dictionary<string, string>? attrs = null,
        bool addTitle = true,
        bool urlOptional = false,
        bool redundantLinks = true,
        Regex? idBreaks = null,
        bool ignoreMarkup = true,
        string markupFormat = "html");

    /// <summary>
    /// Groups citations by their core tokens, creating Authority records.
    /// Returns a streaming enumerable for memory efficiency.
    /// </summary>
    /// <param name="citations">Citations to group into authorities.</param>
    /// <param name="ignoredTokens">Token names to ignore when grouping.</param>
    /// <param name="sortByCites">If true, sorts by citation count (most cited first).</param>
    /// <returns>Streaming enumerable of Authority records.</returns>
    IEnumerable<Authority> ListAuthorities(IEnumerable<Citation> citations,
        IEnumerable<string>? ignoredTokens = null, bool sortByCites = true);
}
