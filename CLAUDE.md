# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

CiteUrl.NET is a C# port of the Python [citeurl](https://github.com/raindrum/citeurl) library for parsing and hyperlinking legal citations. The library supports 130+ citation formats including US federal and state case law, statutes, and regulations using YAML-based extensible templates.

**Technology Stack**: .NET 9, C# 13, xUnit, YamlDotNet, Serilog

**Python Reference**: The original Python implementation is located at `C:\Users\tlewers\source\repos\citeurl` and can be used as a reference for understanding expected behavior, YAML template formats, and bug fixes. When encountering ambiguities or bugs, always consult the Python version first.

## Build and Test Commands

### Building
```bash
# Build entire solution
dotnet build

# Build specific project
dotnet build src/CiteUrl.Core/CiteUrl.Core.csproj
```

### Testing

**IMPORTANT**: Due to a performance issue in `Citator.ListCitations()` idform chain resolution, tests containing `ListCitations` calls will timeout when run together:

```bash
# Run tests individually or in small groups (RECOMMENDED)
dotnet test --filter "FullyQualifiedName~TokenOperationTests"  # ✅ Fast
dotnet test --filter "FullyQualifiedName~TemplateTests"  # ✅ Fast
dotnet test --filter "FullyQualifiedName~YamlLoaderTests"  # ✅ Fast

# AVOID running these together (will timeout):
dotnet test --filter "FullyQualifiedName~CitatorTests.ListCitations"  # ⚠️ Timeout
dotnet test --filter "FullyQualifiedName~CitatorTests"  # ⚠️ Timeout (contains ListCitations tests)
dotnet test  # ⚠️ Timeout (runs all tests)
```

**Known Issue - `ListCitations` Performance Bug** (Citator.cs:126-147):
- **Root Cause**: The `while (true)` loop in `ListCitations` searching for idform chains causes extreme slowdown with 90+ templates
- **Impact**: Any test calling `ListCitations()` will timeout (>30 seconds) when run with other tests
- **Workaround**: Run tests individually or in small groups that don't include `ListCitations` tests
- **Individual Test Status**: All tests pass when run separately (including ListCitations tests)
- **Code Functionality**: The actual citation parsing works correctly; only test execution is affected

**Test Suite Summary** (127+ tests total):
- ✅ TokenOperationTests: 17/17 pass (fast, <100ms)
- ✅ TemplateTests: 5/5 pass (fast, <200ms)
- ✅ TokenTypeTests: All pass (fast)
- ✅ StringBuilderTests: All pass (fast)
- ✅ InsertLinksTests: All pass (3s)
- ✅ YamlLoaderTests: All pass (fast)
- ⚠️ CitatorTests: 10 tests, pass individually, timeout together
- ⚠️ CitationTests: Pass individually, timeout together
- ✅ AuthorityTests: Pass
- ✅ RealWorldTests: Pass
- ✅ ResourceLoaderTests: Pass

**To disable parallel test execution** (doesn't fix the issue but helps isolate):
Create `tests/CiteUrl.Core.Tests/xunit.runner.json`:
```json
{
  "parallelizeAssembly": false,
  "parallelizeTestCollections": false,
  "maxParallelThreads": 1
}
```

### Useful Test Filters
- `--filter "FullyQualifiedName~CitatorTests.Cite_FindsFirstCitation"` - Single test method
- `--filter "FullyQualifiedName~TemplateTests"` - All Template tests
- `--filter "FullyQualifiedName~YamlLoaderTests"` - All YamlLoader tests

## Architecture

### Core Components

**Citator** (`src/CiteUrl.Core/Templates/Citator.cs`)
- Main orchestrator for citation extraction
- Thread-safe singleton pattern using `Lazy<T>`
- Default instance loads embedded YAML templates via `ResourceLoader`
- Implements `ICitator` interface for dependency injection support
- Key methods:
  - `Cite(text)` - Find first citation
  - `ListCitations(text)` - Stream all citations (returns `IEnumerable<Citation>`)
  - `ListAuthorities(citations)` - Group citations by core tokens

**Template** (`src/CiteUrl.Core/Templates/Template.cs`)
- Immutable citation pattern definition with compiled regexes
- Supports template inheritance via `Template.Inherit()`
- Processes YAML patterns by replacing `{token_name}` with regex groups
- Normalizes token names (spaces → underscores) for .NET regex compatibility
- Contains:
  - `Regexes` - Narrow/precise patterns (case-sensitive)
  - `BroadRegexes` - Lenient patterns (case-insensitive)
  - `ProcessedShortformPatterns` - Compiled per-citation instance
  - `ProcessedIdformPatterns` - Compiled per-citation instance

**Citation** (`src/CiteUrl.Core/Models/Citation.cs`)
- Immutable record representing a found citation
- Lazily compiles shortform/idform regexes with parent token substitution
- Computed properties: `Url`, `Name` (built from `StringBuilder`)
- Methods:
  - `FromMatch()` - Create from regex match with token normalization
  - `GetShortformCitations()` - Find subsequent shortforms
  - `GetIdformCitation()` - Find next "Id." reference

**Authority** (`src/CiteUrl.Core/Models/Authority.cs`)
- Groups multiple citations referring to the same legal authority
- Uses core tokens (non-severable) for identity matching
- Tracks all citation instances for a given authority

### YAML Processing

**YamlLoader** (`src/CiteUrl.Core/Utilities/YamlLoader.cs`)
- Deserializes YAML templates using YamlDotNet
- Custom `TokenTypeYamlConverter` handles two token syntax forms:
  - Simple: `volume: \d+` (string → `TokenTypeYaml{Regex="\d+"}`)
  - Complex: `volume: {regex: \d+, edits: [...]}` (full object)
- Handles YAML boolean values (`yes/no/true/false/on/off`)
- Supports template inheritance via `inherit:` property
- Normalizes metadata keys (spaces → underscores)

**TokenType** (`src/CiteUrl.Core/Tokens/TokenType.cs`)
- Defines token regex pattern and normalization rules
- `IsSeverable` flag determines if token affects authority identity
- `Edits` list applies transformations (sub, lookup, case, lpad, number_style)

**StringBuilder** (`src/CiteUrl.Core/Tokens/StringBuilder.cs`)
- Builds URLs and display names from citation tokens
- Concatenates parts with token placeholder substitution
- Applies edits to transform token values
- `UrlEncode` flag for URL generation

### Embedded Resources

Default YAML templates are embedded as resources:
- `src/CiteUrl.Core/Templates/Resources/*.yaml`
- Loaded by `ResourceLoader.LoadAllDefaultYaml()`
- Combined into single YAML string for parsing

## Design Patterns

### Thread Safety
- **Immutable Design**: All models use `ImmutableDictionary`, `ImmutableList`, records
- **Lazy Singleton**: `Citator.Default` uses `Lazy<T>` with `isThreadSafe: true`
- **No Shared Mutable State**: Regex compilation happens at construction

### Streaming Enumeration
- `ListCitations()` returns `IEnumerable<Citation>` for memory efficiency
- Uses `yield return` to stream results instead of buffering
- Enables processing large documents without loading all citations into memory

### Dependency Injection
- `ICitator` interface abstracts citation operations
- Extension project: `CiteUrl.Extensions.DependencyInjection`
- Static convenience methods accept optional `ICitator` parameter

## YAML Template Format

```yaml
Template Name:
  tokens:
    volume: \d+              # Simple syntax (auto-wrapped in TokenTypeYaml)
    page:                     # Complex syntax
      regex: \d+
      severable: yes          # YAML boolean (yes/no/true/false/on/off)
  patterns:
    - '{volume} Test {page}'  # {tokens} replaced with named regex groups
  broad patterns:             # Case-insensitive patterns
    - '{volume} test {page}'
  URL builder:
    parts: ['https://example.com/v', '{volume}', '/p', '{page}']
  name builder: '{volume} Test § {page}'
```

**Important**: Token names with spaces are normalized to underscores in regex groups (e.g., `{reporter id}` becomes `(?<reporter_id>...)`)

## Project Structure

```
src/
  CiteUrl.Core/                         # Main library
    Templates/
      Citator.cs, ICitator.cs           # Citation orchestration
      Template.cs                        # Pattern definitions
      Resources/*.yaml                   # Embedded templates (130+)
    Models/
      Citation.cs                        # Immutable citation record
      Authority.cs                       # Grouped citations
    Tokens/
      TokenType.cs, TokenOperation.cs   # Token normalization
      StringBuilder.cs                   # URL/name generation
    Utilities/
      YamlLoader.cs, YamlModels.cs      # YAML deserialization
      ResourceLoader.cs                  # Embedded resource access
    Exceptions/
      CiteUrlException.cs               # Custom exceptions

  CiteUrl.Extensions.DependencyInjection/ # Optional DI support

tests/
  CiteUrl.Core.Tests/                   # xUnit tests with Shouldly assertions
```

## Common Workflows

### Adding a New Citation Template
1. Edit appropriate YAML file in `src/CiteUrl.Core/Templates/Resources/`
2. Rebuild to embed updated resource: `dotnet build`
3. Test with `Citator.Default.Cite("your test text")`

### Debugging YAML Parsing Issues
- Check `YamlLoader.LoadYaml()` for deserialization errors
- Use `Citator.FromYaml(yamlString)` to test custom templates
- Verify token names don't contain spaces in regex groups (auto-normalized)
- Ensure boolean values use YAML format (`yes/no` not just `true/false`)

### Working with Citation Matching
1. **Longform matching**: `Template.Regexes` and `Template.BroadRegexes`
2. **Shortform matching**: `Citation.GetShortformCitations()` compiles patterns per-citation
3. **Idform matching**: `Citation.GetIdformCitation()` looks for "Id." patterns
4. **Overlap removal**: `Citator.RemoveOverlaps()` prefers longer matches

### Token Normalization Flow
1. Regex captures raw token value → `Citation.RawTokens`
2. `TokenType.Normalize()` applies edits → `Citation.Tokens`
3. `StringBuilder.Build()` uses normalized tokens → `Citation.Url`, `Citation.Name`

## Key Constraints

- **ReDoS Protection**: All regexes use 1-second timeout (configurable)
- **Immutability**: Never mutate `ImmutableDictionary`/`ImmutableList` - use `.Add()`, `.SetItems()` to create new instances
- **Nullable Reference Types**: Enabled (`<Nullable>enable</Nullable>`)
- **Documentation**: XML doc comments required (`GenerateDocumentationFile: true`)
