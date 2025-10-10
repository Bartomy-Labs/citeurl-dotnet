using CiteUrl.Core.Utilities;
using CiteUrl.Core.Exceptions;
using Shouldly;
using Xunit;

namespace CiteUrl.Core.Tests.Utilities;

public class YamlLoaderTests
{
    [Fact]
    public void LoadYaml_ParsesSimpleTemplate()
    {
        // Arrange
        var yaml = @"
Test Template:
  tokens:
    title:
      regex: \d+
  pattern: '{title} U.S.C.'
";

        // Act
        var templates = YamlLoader.LoadYaml(yaml);

        // Assert
        templates.Count.ShouldBe(1);
        templates.ContainsKey("Test Template").ShouldBeTrue();
        var template = templates["Test Template"];
        template.Tokens.Count.ShouldBe(1);
        template.Regexes.Count.ShouldBe(1);
    }

    [Fact]
    public void LoadYaml_SupportsTemplateInheritance()
    {
        // Arrange
        var yaml = @"
Parent:
  tokens:
    token1:
      regex: parent1
  pattern: 'parent pattern'

Child:
  inherit: Parent
  tokens:
    token2:
      regex: child2
  pattern: 'child pattern'
";

        // Act
        var templates = YamlLoader.LoadYaml(yaml);

        // Assert
        templates.Count.ShouldBe(2);
        var child = templates["Child"];
        child.Tokens.Count.ShouldBe(2); // Inherited + new
        child.Tokens.ContainsKey("token1").ShouldBeTrue();
        child.Tokens.ContainsKey("token2").ShouldBeTrue();
    }

    [Fact]
    public void LoadYaml_ThrowsOnInvalidYaml()
    {
        // Arrange
        var invalidYaml = @"
Bad YAML:
  - this is
    invalid: [syntax
";

        // Act & Assert
        Should.Throw<CiteUrlYamlException>(() => YamlLoader.LoadYaml(invalidYaml));
    }

    [Fact]
    public void LoadYaml_ThrowsOnUnknownParent()
    {
        // Arrange
        var yaml = @"
Child:
  inherit: NonExistent
  pattern: 'test'
";

        // Act & Assert
        var ex = Should.Throw<CiteUrlYamlException>(() => YamlLoader.LoadYaml(yaml));
        ex.Message.ShouldContain("NonExistent");
    }

    [Fact]
    public void LoadYaml_SupportsSingularAndPluralKeys()
    {
        // Arrange
        var yaml = @"
Template1:
  pattern: 'single pattern'

Template2:
  patterns:
    - 'pattern 1'
    - 'pattern 2'
";

        // Act
        var templates = YamlLoader.LoadYaml(yaml);

        // Assert
        templates["Template1"].Regexes.Count.ShouldBe(1);
        templates["Template2"].Regexes.Count.ShouldBe(2);
    }
}
