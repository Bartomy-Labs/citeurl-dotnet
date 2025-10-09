using System;
using System.Collections.Immutable;
using System.Text.RegularExpressions;
using CiteUrl.Core.Templates;
using CiteUrl.Core.Tokens;
using Shouldly;
using Xunit;

namespace CiteUrl.Core.Tests.Templates;

public class TemplateTests
{
    [Fact]
    public void Constructor_CompilesRegexesWithTimeout()
    {
        // Arrange & Act
        var template = new Template(
            name: "Test Template",
            tokens: ImmutableDictionary<string, TokenType>.Empty,
            metadata: ImmutableDictionary<string, string>.Empty,
            patterns: new[] { @"\d+" },
            broadPatterns: Array.Empty<string>(),
            shortformPatterns: Array.Empty<string>(),
            idformPatterns: Array.Empty<string>(),
            urlBuilder: null,
            nameBuilder: null,
            regexTimeout: TimeSpan.FromSeconds(1)
        );

        // Assert
        template.Regexes.Count.ShouldBe(1);
        template.RegexTimeout.ShouldBe(TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Constructor_ProcessesTokenReplacements()
    {
        // Arrange
        var tokens = ImmutableDictionary<string, TokenType>.Empty
            .Add("title", new TokenType { Regex = @"\d+" })
            .Add("section", new TokenType { Regex = @"\d+\w*" });

        // Act
        var template = new Template(
            name: "USC",
            tokens: tokens,
            metadata: ImmutableDictionary<string, string>.Empty,
            patterns: new[] { @"{title} U\.S\.C\. § {section}" },
            broadPatterns: Array.Empty<string>(),
            shortformPatterns: Array.Empty<string>(),
            idformPatterns: Array.Empty<string>(),
            urlBuilder: null,
            nameBuilder: null
        );

        // Assert
        template.Regexes.Count.ShouldBe(1);

        // Test the regex matches expected input
        var match = template.Regexes[0].Match("42 U.S.C. § 1983");
        match.Success.ShouldBeTrue();
        match.Groups["title"].Value.ShouldBe("42");
        match.Groups["section"].Value.ShouldBe("1983");
    }

    [Fact]
    public void BroadRegexes_AreCaseInsensitive()
    {
        // Arrange & Act
        var template = new Template(
            name: "Test",
            tokens: ImmutableDictionary<string, TokenType>.Empty,
            metadata: ImmutableDictionary<string, string>.Empty,
            patterns: Array.Empty<string>(),
            broadPatterns: new[] { "section" },
            shortformPatterns: Array.Empty<string>(),
            idformPatterns: Array.Empty<string>(),
            urlBuilder: null,
            nameBuilder: null
        );

        // Assert
        var regex = template.BroadRegexes[0];
        regex.IsMatch("Section").ShouldBeTrue();
        regex.IsMatch("SECTION").ShouldBeTrue();
        regex.IsMatch("section").ShouldBeTrue();
    }

    [Fact]
    public void Inherit_MergesParentAndChildTokens()
    {
        // Arrange
        var parentTokens = ImmutableDictionary<string, TokenType>.Empty
            .Add("token1", new TokenType { Regex = "parent1" })
            .Add("token2", new TokenType { Regex = "parent2" });

        var parent = new Template(
            name: "Parent",
            tokens: parentTokens,
            metadata: ImmutableDictionary<string, string>.Empty,
            patterns: Array.Empty<string>(),
            broadPatterns: Array.Empty<string>(),
            shortformPatterns: Array.Empty<string>(),
            idformPatterns: Array.Empty<string>(),
            urlBuilder: null,
            nameBuilder: null
        );

        var childTokens = ImmutableDictionary<string, TokenType>.Empty
            .Add("token2", new TokenType { Regex = "child2" })  // Override
            .Add("token3", new TokenType { Regex = "child3" }); // New

        // Act
        var child = Template.Inherit(
            parent,
            name: "Child",
            tokens: childTokens
        );

        // Assert
        child.Tokens.Count.ShouldBe(3);
        child.Tokens["token1"].Regex.ShouldBe("parent1"); // Inherited
        child.Tokens["token2"].Regex.ShouldBe("child2"); // Overridden
        child.Tokens["token3"].Regex.ShouldBe("child3"); // New
    }

    [Fact]
    public void Inherit_MergesMetadata()
    {
        // Arrange
        var parentMeta = ImmutableDictionary<string, string>.Empty
            .Add("base_url", "https://parent.com")
            .Add("key1", "parent");

        var parent = new Template(
            name: "Parent",
            tokens: ImmutableDictionary<string, TokenType>.Empty,
            metadata: parentMeta,
            patterns: Array.Empty<string>(),
            broadPatterns: Array.Empty<string>(),
            shortformPatterns: Array.Empty<string>(),
            idformPatterns: Array.Empty<string>(),
            urlBuilder: null,
            nameBuilder: null
        );

        var childMeta = ImmutableDictionary<string, string>.Empty
            .Add("key1", "child");  // Override

        // Act
        var child = Template.Inherit(parent, metadata: childMeta);

        // Assert
        child.Metadata["base_url"].ShouldBe("https://parent.com"); // Inherited
        child.Metadata["key1"].ShouldBe("child"); // Overridden
    }
}
