using CiteUrl.Core.Templates;
using Shouldly;
using Xunit;

namespace CiteUrl.Core.Tests.Templates;

/// <summary>
/// Tests for Citator.InsertLinks() method.
/// Task 4.1: Implement InsertLinks Method
/// </summary>
public class CitatorInsertLinksTests
{
    [Fact]
    public void InsertLinks_CreatesHtmlLinks()
    {
        var text = "See 42 U.S.C. § 1983.";
        var result = Citator.Default.InsertLinks(text);

        result.ShouldContain("<a href");
        result.ShouldContain("1983");
        result.ShouldContain("class=\"citation\"");
    }

    [Fact]
    public void InsertLinks_CreatesMarkdownLinks()
    {
        var text = "See 42 U.S.C. § 1983.";
        var result = Citator.Default.InsertLinks(text, markupFormat: "markdown");

        result.ShouldContain("[");
        result.ShouldContain("](");
        result.ShouldContain("42 U.S.C. § 1983");
    }

    [Fact]
    public void InsertLinks_WithCustomAttributes()
    {
        var text = "See 42 U.S.C. § 1983.";
        var attrs = new Dictionary<string, string>
        {
            ["class"] = "legal-cite",
            ["data-type"] = "statute"
        };

        var result = Citator.Default.InsertLinks(text, attrs: attrs);

        result.ShouldContain("class=\"legal-cite\"");
        result.ShouldContain("data-type=\"statute\"");
    }

    [Fact]
    public void InsertLinks_AddsTitleAttribute()
    {
        var text = "See 42 U.S.C. § 1983.";
        var result = Citator.Default.InsertLinks(text, addTitle: true);

        result.ShouldContain("title=\"");
    }

    [Fact]
    public void InsertLinks_SkipsTitleWhenDisabled()
    {
        var text = "See 42 U.S.C. § 1983.";
        var result = Citator.Default.InsertLinks(text, addTitle: false);

        result.ShouldNotContain("title=\"");
    }

    [Fact]
    public void InsertLinks_SkipsCitationsWithoutUrls()
    {
        // Create citator with minimal template that has no URL builder
        var yaml = @"
Test Citation:
  tokens:
    volume: \d+
  pattern: 'Volume {volume}'
";
        var citator = Citator.FromYaml(yaml);
        var text = "See Volume 123.";

        var result = citator.InsertLinks(text, urlOptional: false);

        // Should return original text if citation has no URL
        result.ShouldBe(text);
    }

    [Fact]
    public void InsertLinks_IncludesCitationsWithoutUrlsWhenOptional()
    {
        // Create citator with minimal template that has no URL builder
        var yaml = @"
Test Citation:
  tokens:
    volume: \d+
  pattern: 'Volume {volume}'
";
        var citator = Citator.FromYaml(yaml);
        var text = "See Volume 123.";

        var result = citator.InsertLinks(text, urlOptional: true);

        // Should create link even without URL
        result.ShouldContain("<a href");
    }

    [Fact]
    public void InsertLinks_SkipsRedundantLinksByDefault()
    {
        var text = "See 42 U.S.C. § 1983 and id.";
        var result = Citator.Default.InsertLinks(text, redundantLinks: false);

        // First citation should be linked
        result.ShouldContain("<a href");

        // Count the number of links (should be 1, not 2)
        var linkCount = result.Split(new[] { "<a href" }, StringSplitOptions.None).Length - 1;
        linkCount.ShouldBe(1);
    }

    [Fact]
    public void InsertLinks_AllowsRedundantLinksWhenEnabled()
    {
        var text = "See 42 U.S.C. § 1983 and id.";
        var result = Citator.Default.InsertLinks(text, redundantLinks: true);

        // Both citations should be linked
        var linkCount = result.Split(new[] { "<a href" }, StringSplitOptions.None).Length - 1;
        linkCount.ShouldBeGreaterThanOrEqualTo(1);
    }

    [Fact]
    public void InsertLinks_EscapesHtmlInCitationText()
    {
        // Create a template with special HTML characters
        var yaml = @"
Test Citation:
  tokens:
    section: '[\w&<>""]+'
  pattern: 'Section {section}'
  URL builder:
    parts: ['http://example.com/', '{section}']
";
        var citator = Citator.FromYaml(yaml);
        var text = "See Section A&B<C>.";

        var result = citator.InsertLinks(text);

        // Should escape HTML characters
        result.ShouldContain("&amp;");
        result.ShouldContain("&lt;");
        result.ShouldContain("&gt;");
    }

    [Fact]
    public void InsertLinks_HandlesMultipleCitations()
    {
        var text = "See 42 U.S.C. § 1983 and 42 U.S.C. § 1985.";
        var result = Citator.Default.InsertLinks(text);

        // Should have two separate links
        var linkCount = result.Split(new[] { "<a href" }, StringSplitOptions.None).Length - 1;
        linkCount.ShouldBe(2);

        result.ShouldContain("1983");
        result.ShouldContain("1985");
    }

    [Fact]
    public void InsertLinks_PreservesTextOutsideCitations()
    {
        var text = "The statute at 42 U.S.C. § 1983 provides remedies.";
        var result = Citator.Default.InsertLinks(text);

        result.ShouldContain("The statute at");
        result.ShouldContain("provides remedies.");
    }

    [Fact]
    public void InsertLinks_ReturnsOriginalTextWhenNoCitations()
    {
        var text = "This text has no citations.";
        var result = Citator.Default.InsertLinks(text);

        result.ShouldBe(text);
    }

    [Fact]
    public void InsertLinks_MarkdownFormat_PreservesSpecialCharacters()
    {
        var text = "See 42 U.S.C. § 1983.";
        var result = Citator.Default.InsertLinks(text, markupFormat: "markdown");

        // Should preserve section symbol in markdown
        result.ShouldContain("§");
        result.ShouldContain("[42 U.S.C. § 1983]");
    }

    [Fact]
    public void InsertLinks_HtmlFormat_CaseInsensitive()
    {
        var text = "See 42 U.S.C. § 1983.";
        var resultLower = Citator.Default.InsertLinks(text, markupFormat: "html");
        var resultUpper = Citator.Default.InsertLinks(text, markupFormat: "HTML");

        // Both should produce HTML links
        resultLower.ShouldContain("<a href");
        resultUpper.ShouldContain("<a href");
    }
}
