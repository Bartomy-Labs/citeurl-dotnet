using System;
using System.Collections.Generic;
using CiteUrl.Core.Tokens;
using Shouldly;
using Xunit;

namespace CiteUrl.Core.Tests.Tokens;

public class TokenTypeTests
{
    [Fact]
    public void Normalize_AppliesEditPipelineInOrder()
    {
        // Arrange
        var tokenType = new TokenType
        {
            Regex = @".*",
            Edits = new[]
            {
                new TokenOperation { Action = TokenOperationAction.Case, Data = "lower" },
                new TokenOperation { Action = TokenOperationAction.Sub, Data = (@"[.()]", "") },
                new TokenOperation
                {
                    Action = TokenOperationAction.Lookup,
                    Data = new Dictionary<string, string> { ["f 2d"] = "f2d" },
                    IsMandatory = false
                }
            }
        };

        // Act
        var result = tokenType.Normalize("F. 2d");

        // Assert
        result.ShouldBe("f2d");
    }

    [Fact]
    public void Normalize_ReturnsDefaultWhenInputIsNull()
    {
        // Arrange
        var tokenType = new TokenType
        {
            Default = "default-value",
            Edits = Array.Empty<TokenOperation>()
        };

        // Act
        var result = tokenType.Normalize(null);

        // Assert
        result.ShouldBe("default-value");
    }

    [Fact]
    public void Normalize_PropagatesExceptionsFromMandatoryOperations()
    {
        // Arrange
        var tokenType = new TokenType
        {
            Edits = new[]
            {
                new TokenOperation
                {
                    Action = TokenOperationAction.Lookup,
                    Data = new Dictionary<string, string> { ["known"] = "value" },
                    IsMandatory = true
                }
            }
        };

        // Act & Assert
        Should.Throw<InvalidOperationException>(() => tokenType.Normalize("unknown"));
    }
}
