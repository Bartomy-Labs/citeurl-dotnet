using CiteUrl.Core.Templates;
using CiteUrl.Core.Models;
using Shouldly;
using Xunit;
using System.Collections.Immutable;
using System.Text.RegularExpressions;

namespace CiteUrl.Core.Tests.Templates;

public class CitatorTests
{
    [Fact]
    public void Default_LoadsAllTemplates()
    {
        // Act
        var citator = Citator.Default;

        // Assert
        citator.Templates.Count.ShouldBeGreaterThan(50);
        citator.Templates.ShouldNotBeEmpty();
    }

    [Fact]
    public void Default_IsThreadSafe()
    {
        // Act - access from multiple tasks
        var tasks = Enumerable.Range(0, 10)
            .Select(_ => Task.Run(() => Citator.Default))
            .ToArray();

        Task.WaitAll(tasks);

        // Assert - all should return same instance
        var first = tasks[0].Result;
        tasks.All(t => ReferenceEquals(t.Result, first)).ShouldBeTrue();
    }

    [Fact]
    public void Cite_FindsFirstCitation()
    {
        // Arrange
        var text = "See 42 U.S.C. § 1983 for details.";

        // Act
        var citation = Citator.Cite(text);

        // Assert
        citation.ShouldNotBeNull();
        citation!.Text.ShouldContain("1983");
        citation.Template.ShouldNotBeNull();
    }

    [Fact]
    public void Cite_ReturnsNullWhenNoCitationFound()
    {
        // Arrange
        var text = "This text has no legal citations.";

        // Act
        var citation = Citator.Cite(text);

        // Assert
        citation.ShouldBeNull();
    }

    [Fact]
    public void ListCitations_ReturnsStreamingEnumerable()
    {
        // Arrange
        var text = "See 42 U.S.C. § 1983 and 29 C.F.R. § 1630.2.";

        // Act
        var citations = Citator.ListCitations(text);

        // Assert
        citations.ShouldBeAssignableTo<IEnumerable<Citation>>();
        citations.Count().ShouldBeGreaterThanOrEqualTo(1);
    }

    [Fact]
    public void ListCitations_RemovesOverlappingCitations()
    {
        // Arrange - create text with overlapping patterns
        var text = "42 U.S.C. § 1983";
        var citator = Citator.Default;

        // Act
        var citations = citator.ListCitations(text).ToList();

        // Assert - should not have overlapping spans
        for (int i = 0; i < citations.Count - 1; i++)
        {
            var current = citations[i];
            var next = citations[i + 1];
            next.Span.Start.ShouldBeGreaterThanOrEqualTo(current.Span.End,
                $"Citation '{next.Text}' overlaps with '{current.Text}'");
        }
    }

    [Fact]
    public void ListCitations_SortsByPosition()
    {
        // Arrange
        var text = "See 29 C.F.R. § 1630.2 and 42 U.S.C. § 1983.";

        // Act
        var citations = Citator.ListCitations(text).ToList();

        // Assert - citations should be in order of appearance
        for (int i = 0; i < citations.Count - 1; i++)
        {
            citations[i].Span.Start.ShouldBeLessThan(citations[i + 1].Span.Start);
        }
    }

    [Fact]
    public void FromYaml_CreatesCustomCitator()
    {
        // Arrange
        var yaml = @"
Test Citation:
  tokens:
    volume: \d+
    page: \d+
  patterns:
    - '{volume} Test {page}'
";

        // Act
        var citator = Citator.FromYaml(yaml, "test.yaml");

        // Assert
        citator.Templates.Count.ShouldBe(1);
        citator.Templates.ShouldContainKey("Test Citation");
    }

    [Fact]
    public void Templates_IsImmutable()
    {
        // Arrange
        var citator = Citator.Default;

        // Assert
        citator.Templates.ShouldBeOfType<ImmutableDictionary<string, Template>>();
    }

    [Fact]
    public void StaticConvenienceMethods_UseDefaultInstance()
    {
        // Arrange
        var text = "See 42 U.S.C. § 1983.";

        // Act
        var citationStatic = Citator.Cite(text);
        var citationInstance = Citator.Default.Cite(text);

        // Assert
        citationStatic.ShouldNotBeNull();
        citationInstance.ShouldNotBeNull();
        citationStatic!.Text.ShouldBe(citationInstance!.Text);
    }
}
