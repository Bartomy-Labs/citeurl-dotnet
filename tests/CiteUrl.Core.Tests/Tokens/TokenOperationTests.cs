using System;
using System.Collections.Generic;
using CiteUrl.Core.Tokens;
using Shouldly;
using Xunit;

namespace CiteUrl.Core.Tests.Tokens;

public class TokenOperationTests
{
    [Fact]
    public void Sub_ReplacesPatternWithReplacement()
    {
        // Arrange
        var op = new TokenOperation
        {
            Action = TokenOperationAction.Sub,
            Data = (@"[.()]", "")
        };

        // Act
        var result = op.Apply("F. 2d (Test)");

        // Assert
        result.ShouldBe("F 2d Test");
    }

    [Fact]
    public void Lookup_FindsKeysCaseInsensitively()
    {
        // Arrange
        var dict = new Dictionary<string, string>
        {
            ["f-2d"] = "f2d",
            ["a-2d"] = "a2d"
        };
        var op = new TokenOperation
        {
            Action = TokenOperationAction.Lookup,
            Data = dict
        };

        // Act
        var result = op.Apply("F-2D");

        // Assert
        result.ShouldBe("f2d");
    }

    [Fact]
    public void Lookup_Mandatory_ThrowsWhenKeyNotFound()
    {
        // Arrange
        var dict = new Dictionary<string, string> { ["known"] = "value" };
        var op = new TokenOperation
        {
            Action = TokenOperationAction.Lookup,
            Data = dict,
            IsMandatory = true
        };

        // Act & Assert
        Should.Throw<InvalidOperationException>(() => op.Apply("unknown"));
    }

    [Fact]
    public void Lookup_Optional_ReturnsInputWhenKeyNotFound()
    {
        // Arrange
        var dict = new Dictionary<string, string> { ["known"] = "value" };
        var op = new TokenOperation
        {
            Action = TokenOperationAction.Lookup,
            Data = dict,
            IsMandatory = false
        };

        // Act
        var result = op.Apply("unknown");

        // Assert
        result.ShouldBe("unknown");
    }

    [Theory]
    [InlineData("upper", "hello world", "HELLO WORLD")]
    [InlineData("lower", "HELLO WORLD", "hello world")]
    [InlineData("title", "hello world", "Hello World")]
    public void Case_TransformsCorrectly(string caseType, string input, string expected)
    {
        // Arrange
        var op = new TokenOperation
        {
            Action = TokenOperationAction.Case,
            Data = caseType
        };

        // Act
        var result = op.Apply(input);

        // Assert
        result.ShouldBe(expected);
    }

    [Fact]
    public void LeftPad_PadsToCorrectWidth()
    {
        // Arrange
        var op = new TokenOperation
        {
            Action = TokenOperationAction.LeftPad,
            Data = ('0', 5)
        };

        // Act
        var result = op.Apply("42");

        // Assert
        result.ShouldBe("00042");
    }

    [Theory]
    [InlineData("digit", "roman", "5", "V")]
    [InlineData("digit", "roman", "42", "XLII")]
    [InlineData("roman", "digit", "X", "10")]
    [InlineData("roman", "digit", "XCIX", "99")]
    [InlineData("digit", "cardinal", "5", "five")]
    [InlineData("cardinal", "digit", "forty-two", "42")]
    [InlineData("digit", "ordinal", "5", "fifth")]
    [InlineData("ordinal", "digit", "forty-second", "42")]
    public void NumberStyle_ConvertsCorrectly(string from, string to, string input, string expected)
    {
        // Arrange
        var op = new TokenOperation
        {
            Action = TokenOperationAction.NumberStyle,
            Data = (from, to)
        };

        // Act
        var result = op.Apply(input);

        // Assert
        result.ShouldBe(expected, StringCompareShould.IgnoreCase);
    }

    [Fact]
    public void NumberStyle_SupportsRange1To100()
    {
        // Arrange
        var op = new TokenOperation
        {
            Action = TokenOperationAction.NumberStyle,
            Data = ("digit", "roman")
        };

        // Act & Assert
        op.Apply("1").ShouldBe("I");
        op.Apply("100").ShouldBe("C");
    }
}
