using CiteUrl.Core.Tokens;
using Shouldly;
using Xunit;
using System.Collections.Generic;

namespace CiteUrl.Core.Tests.Tokens;

public class StringBuilderTests
{
    [Fact]
    public void Build_ConcatenatesAllPartsWithTokens()
    {
        // Arrange
        var builder = new CiteUrl.Core.Tokens.StringBuilder
        {
            Parts = new[]
            {
                "https://example.com/",
                "{section}/",
                "{subsection}"
            },
            UrlEncode = false
        };

        var tokens = new Dictionary<string, string>
        {
            ["section"] = "42",
            ["subsection"] = "1983"
        };

        // Act
        var result = builder.Build(tokens);

        // Assert
        result.ShouldBe("https://example.com/42/1983");
    }

    [Fact]
    public void Build_SkipsPartsWithMissingTokens()
    {
        // Arrange
        var builder = new CiteUrl.Core.Tokens.StringBuilder
        {
            Parts = new[]
            {
                "https://example.com/",
                "{section}",
                "/{subsection}"  // subsection is missing
            },
            UrlEncode = false
        };

        var tokens = new Dictionary<string, string>
        {
            ["section"] = "42"
            // subsection missing
        };

        // Act
        var result = builder.Build(tokens);

        // Assert
        result.ShouldBe("https://example.com/42");
    }

    [Fact]
    public void Build_ReturnsNullWhenAllPartsSkipped()
    {
        // Arrange
        var builder = new CiteUrl.Core.Tokens.StringBuilder
        {
            Parts = new[]
            {
                "{missing_token}"
            }
        };

        var tokens = new Dictionary<string, string>();

        // Act
        var result = builder.Build(tokens);

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public void Build_AppliesEditsBeforeBuilding()
    {
        // Arrange
        var builder = new CiteUrl.Core.Tokens.StringBuilder
        {
            Parts = new[] { "https://example.com/{section}" },
            Edits = new[]
            {
                new TokenOperation
                {
                    Action = TokenOperationAction.Sub,
                    Data = (@"[()]", ""),
                    Token = "section"
                }
            },
            UrlEncode = false
        };

        var tokens = new Dictionary<string, string>
        {
            ["section"] = "1983(b)"
        };

        // Act
        var result = builder.Build(tokens);

        // Assert
        result.ShouldBe("https://example.com/1983b");
    }

    [Fact]
    public void Build_SupportsOutputTokens()
    {
        // Arrange
        var builder = new CiteUrl.Core.Tokens.StringBuilder
        {
            Parts = new[] { "{base_url}/{normalized}" },
            Edits = new[]
            {
                new TokenOperation
                {
                    Action = TokenOperationAction.Case,
                    Data = "lower",
                    Token = "section",
                    Output = "normalized"
                }
            },
            Defaults = new Dictionary<string, string>
            {
                ["base_url"] = "https://example.com"
            },
            UrlEncode = false
        };

        var tokens = new Dictionary<string, string>
        {
            ["section"] = "HELLO"
        };

        // Act
        var result = builder.Build(tokens);

        // Assert
        result.ShouldBe("https://example.com/hello");
    }

    [Fact]
    public void Build_MergesDefaultsWithProvidedTokens()
    {
        // Arrange
        var builder = new CiteUrl.Core.Tokens.StringBuilder
        {
            Parts = new[] { "{base}/{path}" },
            Defaults = new Dictionary<string, string>
            {
                ["base"] = "default-base",
                ["path"] = "default-path"
            },
            UrlEncode = false
        };

        var tokens = new Dictionary<string, string>
        {
            ["path"] = "custom-path"
            // base will use default
        };

        // Act
        var result = builder.Build(tokens);

        // Assert
        result.ShouldBe("default-base/custom-path");
    }

    [Fact]
    public void Build_AppliesUrlEncoding()
    {
        // Arrange
        var builder = new CiteUrl.Core.Tokens.StringBuilder
        {
            Parts = new[] { "https://example.com/{section}" },
            UrlEncode = true
        };

        var tokens = new Dictionary<string, string>
        {
            ["section"] = "test (with parens)"
        };

        // Act
        var result = builder.Build(tokens);

        // Assert
        result.ShouldBe("https://example.com/test%20%28with%20parens%29");
    }

    [Fact]
    public void Build_SkipsUrlEncodingWhenDisabled()
    {
        // Arrange
        var builder = new CiteUrl.Core.Tokens.StringBuilder
        {
            Parts = new[] { "{title} U.S.C. § {section}" },
            UrlEncode = false
        };

        var tokens = new Dictionary<string, string>
        {
            ["title"] = "42",
            ["section"] = "1983"
        };

        // Act
        var result = builder.Build(tokens);

        // Assert
        result.ShouldBe("42 U.S.C. § 1983");
    }
}
