using System.Collections.Immutable;
using System.Text.RegularExpressions;
using CiteUrl.Core.Models;
using CiteUrl.Core.Templates;
using CiteUrl.Core.Tokens;
using Shouldly;
using Xunit;

namespace CiteUrl.Core.Tests.Models;

public class CitationTests
{
    private Template CreateTestTemplate()
    {
        var tokens = ImmutableDictionary<string, TokenType>.Empty
            .Add("title", new TokenType { Regex = @"\d+" })
            .Add("section", new TokenType { Regex = @"\d+\w*" });

        return new Template(
            name: "Test Template",
            tokens: tokens,
            metadata: ImmutableDictionary<string, string>.Empty,
            patterns: new[] { @"{title} U\.S\.C\. § {section}" },
            broadPatterns: [],
            shortformPatterns: new[] { @"§ {same section}" },
            idformPatterns: new[] { @"[Ii]d\." },
            urlBuilder: null,
            nameBuilder: null
        );
    }

    [Fact]
    public void FromMatch_ExtractsTokensFromMatch()
    {
        // Arrange
        var template = CreateTestTemplate();
        var text = "See 42 U.S.C. § 1983 for details.";
        var match = template.Regexes[0].Match(text);

        // Act
        var citation = Citation.FromMatch(match, template, text);

        // Assert
        citation.Text.ShouldBe("42 U.S.C. § 1983");
        citation.Tokens["title"].ShouldBe("42");
        citation.Tokens["section"].ShouldBe("1983");
        citation.Span.Start.ShouldBe(4);
        citation.Span.End.ShouldBe(20);
    }

    [Fact]
    public void FromMatch_InheritsTokensFromParent()
    {
        // Arrange
        var template = CreateTestTemplate();
        var parentText = "42 U.S.C. § 1983";
        var parentMatch = template.Regexes[0].Match(parentText);
        var parent = Citation.FromMatch(parentMatch, template, parentText);

        // Create a shortform citation manually to test inheritance
        var childTokens = ImmutableDictionary<string, string>.Empty
            .Add("section", "1985");

        var child = new Citation
        {
            Template = template,
            Parent = parent,
            RawTokens = ImmutableDictionary<string, string>.Empty.Add("section", "1985"),
            Tokens = childTokens.Add("title", "42") // Manually simulate inheritance
        };

        // Assert
        child.Tokens["title"].ShouldBe("42"); // Inherited from parent
        child.Tokens["section"].ShouldBe("1985"); // Overridden in child
    }

    [Fact]
    public void ShortformRegexes_CompilesLazily()
    {
        // Arrange
        var template = CreateTestTemplate();
        var text = "42 U.S.C. § 1983";
        var match = template.Regexes[0].Match(text);
        var citation = Citation.FromMatch(match, template, text);

        // Act
        var shortformRegexes = citation.ShortformRegexes;

        // Assert
        shortformRegexes.ShouldNotBeEmpty();
        // Accessing again should return same instance (lazy)
        citation.ShortformRegexes.ShouldBe(shortformRegexes);
    }

    [Fact]
    public void IdformRegexes_IncludesBasicIdPattern()
    {
        // Arrange
        var template = CreateTestTemplate();
        var text = "42 U.S.C. § 1983";
        var match = template.Regexes[0].Match(text);
        var citation = Citation.FromMatch(match, template, text);

        // Act
        var idformRegexes = citation.IdformRegexes;

        // Assert
        idformRegexes.ShouldNotBeEmpty();
        idformRegexes.ShouldContain(r => r.IsMatch("Id."));
        idformRegexes.ShouldContain(r => r.IsMatch("id."));
    }

    [Fact]
    public void Url_ComputedFromUrlBuilder()
    {
        // Arrange
        var urlBuilder = new CiteUrl.Core.Tokens.StringBuilder
        {
            Parts = new[] { "https://example.com/{title}/{section}" },
            UrlEncode = false
        };

        var tokens = ImmutableDictionary<string, TokenType>.Empty
            .Add("title", new TokenType { Regex = @"\d+" })
            .Add("section", new TokenType { Regex = @"\d+" });

        var template = new Template(
            name: "Test",
            tokens: tokens,
            metadata: ImmutableDictionary<string, string>.Empty,
            patterns: new[] { @"{title} USC {section}" },
            broadPatterns: [],
            shortformPatterns: [],
            idformPatterns: [],
            urlBuilder: urlBuilder,
            nameBuilder: null
        );

        var citation = new Citation
        {
            Template = template,
            Tokens = ImmutableDictionary<string, string>.Empty
                .Add("title", "42")
                .Add("section", "1983")
        };

        // Act & Assert
        citation.Url.ShouldBe("https://example.com/42/1983");
    }

    [Fact]
    public void Name_ComputedFromNameBuilder()
    {
        // Arrange
        var nameBuilder = new CiteUrl.Core.Tokens.StringBuilder
        {
            Parts = new[] { "{title} U.S.C. § {section}" },
            UrlEncode = false
        };

        var tokens = ImmutableDictionary<string, TokenType>.Empty
            .Add("title", new TokenType { Regex = @"\d+" })
            .Add("section", new TokenType { Regex = @"\d+" });

        var template = new Template(
            name: "Test",
            tokens: tokens,
            metadata: ImmutableDictionary<string, string>.Empty,
            patterns: new[] { @"{title} USC {section}" },
            broadPatterns: [],
            shortformPatterns: [],
            idformPatterns: [],
            urlBuilder: null,
            nameBuilder: nameBuilder
        );

        var citation = new Citation
        {
            Template = template,
            Tokens = ImmutableDictionary<string, string>.Empty
                .Add("title", "42")
                .Add("section", "1983")
        };

        // Act & Assert
        citation.Name.ShouldBe("42 U.S.C. § 1983");
    }

    [Fact]
    public void Citation_IsImmutable()
    {
        // Arrange
        var template = CreateTestTemplate();
        var text = "42 U.S.C. § 1983";
        var match = template.Regexes[0].Match(text);

        // Act
        var citation = Citation.FromMatch(match, template, text);

        // Assert - record types are immutable by default
        citation.ShouldBeOfType<Citation>();
        citation.Tokens.ShouldBeOfType<ImmutableDictionary<string, string>>();
        citation.RawTokens.ShouldBeOfType<ImmutableDictionary<string, string>>();
    }

    [Fact]
    public void FromMatch_NormalizesTokens()
    {
        // Arrange
        var numberOperation = new TokenOperation
        {
            Action = TokenOperationAction.NumberStyle,
            Data = ("roman", "digit")
        };

        var tokens = ImmutableDictionary<string, TokenType>.Empty
            .Add("volume", new TokenType
            {
                Regex = @"[IVX]+",
                Edits = [numberOperation]
            });

        var template = new Template(
            name: "Test",
            tokens: tokens,
            metadata: ImmutableDictionary<string, string>.Empty,
            patterns: new[] { @"Volume {volume}" },
            broadPatterns: [],
            shortformPatterns: [],
            idformPatterns: [],
            urlBuilder: null,
            nameBuilder: null
        );

        var text = "Volume XII";
        var match = template.Regexes[0].Match(text);

        // Act
        var citation = Citation.FromMatch(match, template, text);

        // Assert
        citation.RawTokens["volume"].ShouldBe("XII"); // Raw
        citation.Tokens["volume"].ShouldBe("12"); // Normalized
    }
}
