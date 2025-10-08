# CiteUrl.NET - Complete Implementation Plan
## L.E.A.S.H. Orchestrated Development Plan

**Status**: READY FOR APPROVAL
**Created**: 2025-10-06
**Plan Version**: 1.0
**Methodology**: L.E.A.S.H. v13.0.1 (Semantic Intent Edition with Approval Guards)

---

## Executive Summary

This plan details the complete port of the Python `citeurl` library to .NET C# as a production-ready NuGet package. The implementation is divided into 4 phases over an estimated 4-6 weeks, with systematic testing and documentation throughout.

**Key Outcomes:**
- Production NuGet package `CiteUrl.Core` (.NET 9)
- 100% feature parity with Python version
- 130+ legal citation sources supported via YAML templates
- >80% test coverage with comprehensive test suite
- Full XML documentation and usage examples
- Integration ready for CourtListener MCP server

---

## Design Decisions (RESOLVED)

### 1. **Target Framework: .NET 9** ✅
**Decision**: Use .NET 9 for modern features and performance
**Rationale**:
- Latest .NET LTS release with best performance
- Full access to C# 13 features
- Modern collections, LINQ optimizations, and regex improvements
- Native AOT support if needed
- Enables use in Blazor, ASP.NET Core, Console apps, and MCP servers
- Can be consumed by any .NET 9+ application

### 2. **API Design: C#-Idiomatic with Python Compatibility** ✅
**Decision**: Design idiomatic C# API while maintaining conceptual compatibility with Python
**Examples**:
- `cite()` → `Cite()` (PascalCase)
- `list_cites()` → `ListCitations()` (descriptive name)
- `insert_links()` → `InsertLinks()` (match Python)
- Return `IEnumerable<T>` instead of Python lists
- Use C# properties instead of Python `@property` decorators
- Nullable reference types enabled throughout

### 3. **Template Loading: Embedded Resources + File Support** ✅
**Decision**: Embed default YAML templates as resources, support external file loading
**Implementation**:
- Default 5 YAML files embedded in assembly
- `Citator` constructor loads embedded resources by default
- `Citator.FromYaml()` for custom YAML strings
- `Citator(yamlPaths: ["path/to/custom.yaml"])` for file-based loading
- Supports template inheritance and overrides

### 4. **Async/Await: Synchronous API** ✅
**Decision**: Keep API synchronous (no async)
**Rationale**:
- Citation parsing is CPU-bound regex work, not I/O-bound
- Python version is synchronous
- Adding async adds complexity without performance benefit
- Can be added later if needed without breaking changes

### 5. **Immutability: Records for Data, Classes for Behavior** ✅
**Decision**: Use `record` types for immutable data, `class` for stateful objects
**Mapping**:
- `Citation` → `record class` (immutable with computed properties)
- `Authority` → `record class` (immutable with citation list)
- `Template` → `class` (has compiled regexes, caches)
- `Citator` → `class` (manages template dictionary)
- `TokenType` → `record` (immutable token definition)
- `StringBuilder` → `class` (builds strings from tokens)

### 6. **Null Handling: Nullable Reference Types Enabled** ✅
**Decision**: Enable nullable reference types across entire project
**Benefits**:
- Compile-time null safety
- Explicit intent in API surface
- Modern C# best practice
- Better IDE support and warnings

### 7. **Performance: Aggressive Regex Compilation + Caching** ✅
**Decision**: Use `RegexOptions.Compiled` + caching strategy
**Strategy**:
- All template regexes compiled at Template construction
- Citator caches compiled templates by name
- Regex patterns compiled once, reused for all matches
- No runtime regex compilation overhead

### 8. **Extensibility: Full Custom Template Support** ✅
**Decision**: Allow users to provide custom YAML templates
**Capabilities**:
- Load custom YAML files via constructor
- Inherit from built-in templates
- Override built-in templates with same name
- Template inheritance supported (like Python)

---

## C# Class Structure (DEFINED)

### Core Namespace: `CiteUrl.Core`

```csharp
// Main API Classes
public class Citator
public class Template
public record class Citation
public record class Authority

// Token Processing
public record TokenType
public class TokenOperation
public class StringBuilder

// Utilities
internal static class RegexUtilities
internal static class YamlLoader
```

### Class Responsibilities

**`Citator`** - Main entry point, template manager
- Loads and manages Template collection
- Provides `Cite()`, `ListCitations()`, `InsertLinks()`, `ListAuthorities()`
- Singleton pattern for default instance

**`Template`** - Citation pattern matcher
- Holds compiled regexes for long/short/id-form patterns
- Token dictionary with normalization rules
- URL builder and name builder
- Template inheritance support

**`Citation`** - Immutable citation found in text
- Tokens dictionary (normalized values)
- Raw tokens (as captured)
- URL and Name (computed properties)
- Span (start/end position in text)
- Parent citation (for shortforms/idforms)
- Methods to find child citations

**`Authority`** - Grouped citations to same source
- Template reference
- Core tokens (excludes pincites/subsections)
- List of citations
- URL and Name (computed)

**`TokenType`** - Token definition and normalization
- Regex pattern
- Edit operations (lookup tables, case transforms, etc.)
- Default value
- Severable flag (for hierarchical tokens)

**`TokenOperation`** - String transformation
- Actions: sub, lookup, case, lpad, number_style
- Mandatory flag (throw on failure or skip)
- Used in TokenType normalization and StringBuilder

**`StringBuilder`** - Builds URLs/names from tokens
- Parts list (string templates with {token} placeholders)
- Edits to apply before building
- Defaults from template metadata

---

## YAML Template Porting Strategy

### Template Files (Copy + Minor Adjustments)

1. **caselaw.yaml** (82 lines)
   - Copy verbatim from Python
   - Verify C# regex compatibility
   - Test reporter normalization lookup tables

2. **general-federal-law.yaml** (226 lines)
   - USC, CFR, Constitution patterns
   - Copy verbatim
   - Verify URL builders work with Cornell LII

3. **specific-federal-laws.yaml** (178 lines)
   - Federal rules, regulations
   - Copy verbatim
   - Test pattern matching

4. **state-law.yaml** (2,120 lines)
   - All 50 states + territories
   - Copy verbatim
   - Verify state code URL builders

5. **secondary-sources.yaml** (34 lines)
   - Law reviews, restatements
   - Copy verbatim

### Regex Compatibility

**Compatible** (no changes needed):
- Named groups: `(?P<name>...)` (same in C#)
- Lookahead/lookbehind: `(?<=...)` and `(?!...)`
- Character classes: `\d`, `\w`, `\s`
- Quantifiers: `+`, `*`, `?`, `{n,m}`

**Incompatible** (requires adjustment):
- None identified in YAML analysis
- C# `System.Text.RegularExpressions` is feature-compatible

### YamlDotNet Integration

```csharp
// Deserialization models
public class TemplateYaml
{
    public Dictionary<string, TokenTypeYaml>? Tokens { get; set; }
    public Dictionary<string, string>? Meta { get; set; }
    public List<string>? Patterns { get; set; }
    public List<string>? BroadPatterns { get; set; }
    public List<string>? ShortformPatterns { get; set; }
    public List<string>? IdformPatterns { get; set; }
    public StringBuilderYaml? UrlBuilder { get; set; }
    public StringBuilderYaml? NameBuilder { get; set; }
    public string? Inherit { get; set; }
}
```

---

## Test Strategy

### Test Framework: xUnit + Shouldly

**Test Coverage Goals:**
- Unit tests: 80%+ coverage
- Integration tests: All public API methods
- YAML template tests: Sample citations for each template

### Test Categories

1. **Unit Tests** (`CiteUrl.Core.Tests`)
   - TokenType normalization
   - TokenOperation transformations
   - StringBuilder URL/name building
   - Regex pattern processing
   - Template inheritance

2. **Integration Tests**
   - Citator.Cite() - find single citation
   - Citator.ListCitations() - find all citations
   - Citator.InsertLinks() - hyperlink insertion
   - Citator.ListAuthorities() - authority grouping
   - Shortform citation resolution
   - Id. citation chains

3. **YAML Template Tests**
   - Parse each of the 5 YAML files
   - Test sample citations for major templates:
     - Federal case law (e.g., "477 U.S. 561")
     - State case law (e.g., "123 Cal.App.4th 456")
     - USC (e.g., "42 U.S.C. § 1983")
     - CFR (e.g., "29 C.F.R. § 1630.2")
     - State codes (e.g., "Cal. Civ. Code § 1234")

4. **Real-World Text Tests**
   - Legal opinion paragraphs with multiple citations
   - Mixed citation types
   - Shortform and id. citation chains
   - Edge cases from Python test suite

### Python Test Port

Port existing Python tests from `tests/` directory:
- `test_citation.py` → `CitationTests.cs`
- `test_citator.py` → `CitatorTests.cs`
- `test_authority.py` → `AuthorityTests.cs`
- `test_tokens.py` → `TokenTests.cs`

---

## Implementation Phases

### PHASE 1: Foundation & Token System
**Duration**: 1 week
**Focus**: Core data structures and token processing

#### Task 1.1: Project Setup and Solution Structure
**Objective**: Create .NET solution with projects, dependencies, and build configuration

**Critical Anchors** (immutable - never change):
- .NET 9 target framework
- MIT license with attribution to Simon Raindrum Sherred
- YamlDotNet for YAML parsing
- Nullable reference types enabled
- C# 13 language version

**Agent Instructions**:
1. Create solution file `CiteUrl.sln`
2. Create class library `src/CiteUrl.Core/CiteUrl.Core.csproj`
   - TargetFramework: net9.0
   - LangVersion: 13
   - Nullable: enable
   - GeneratePackageOnBuild: true
   - PackageId: CiteUrl.Core
   - Version: 1.0.0-alpha
   - Authors: [User's name]
   - Description: "Legal citation parser for .NET - port of Python citeurl library"
   - PackageLicenseExpression: MIT
   - RepositoryUrl: [User's GitHub URL]
3. Add NuGet package: `YamlDotNet` (latest stable)
4. Create test project `tests/CiteUrl.Core.Tests/CiteUrl.Core.Tests.csproj`
   - TargetFramework: net9.0
   - Add packages: xUnit, xUnit.runner.visualstudio, Shouldly, coverlet.collector
5. Create directory structure:
   ```
   src/CiteUrl.Core/
   ├── Models/        (Citation, Authority records)
   ├── Templates/     (Template, Citator classes)
   ├── Tokens/        (TokenType, TokenOperation, StringBuilder)
   ├── Utilities/     (RegexUtilities, YamlLoader)
   └── Resources/     (embedded YAML files)
   ```
6. Add `.editorconfig` with C# style rules
7. Add README.md with project description

**Success Criteria**:
- Solution builds without errors
- Projects reference each other correctly
- NuGet packages restore successfully
- Test project runs (0 tests initially)

**Risk Factors**:
- YamlDotNet version compatibility issues
- .NET 9 SDK availability on build environment

---

#### Task 1.2: Implement TokenType and TokenOperation
**Objective**: Build token normalization system (lookup tables, regex substitution, case transforms)

**Critical Anchors**:
- Immutable `record` for TokenType
- Functional transformations (no side effects)
- Mandatory flag controls exception vs. skip behavior

**Agent Instructions**:
1. Create `TokenOperation.cs` in `Tokens/` folder
   - Enum `TokenOperationAction`: Sub, Lookup, Case, LeftPad, NumberStyle
   - Record `TokenOperation` with:
     - `Action` property
     - `Data` property (object type, stores pattern/table/value)
     - `IsMandatory` property (default true)
     - `Token` property (for StringBuilder edits)
     - `Output` property (for StringBuilder derived tokens)
   - Method `string Apply(string input)` - execute transformation
   - Static factory `FromDictionary(Dictionary<string, object> dict)` for YAML deserialization
   - Implement each action:
     - **Sub**: Regex.Replace(input, pattern, replacement)
     - **Lookup**: Match input against dictionary keys (case-insensitive), return value
     - **Case**: ToUpper/ToLower/ToTitleCase
     - **LeftPad**: PadLeft with specified character and length
     - **NumberStyle**: Convert between roman/cardinal/ordinal/digit (support 1-100)

2. Create `TokenType.cs` in `Tokens/` folder
   - Record `TokenType` with:
     - `string Regex` property
     - `IReadOnlyList<TokenOperation> Edits` property
     - `string? Default` property
     - `bool IsSeverable` property
   - Method `string Normalize(string? rawValue)` - apply edits chain
   - Static factory `FromDictionary(string name, Dictionary<string, object> dict)`

3. Create unit tests in `TokenTests.cs`:
   - Test each TokenOperation action
   - Test TokenType normalization chains
   - Test mandatory vs. optional operations
   - Test number_style conversions (1-100)
   - Test lookup table matching (case-insensitive)

**Associated Artifacts**:
```csharp
// Example: Reporter token normalization
var reporterToken = new TokenType(
    Regex: @"Abb\. Ct\. App\.|...",
    Edits: [
        new TokenOperation(Action: Case, Data: "lower"),
        new TokenOperation(Action: Sub, Data: (@"[.()&,']", "")),
        new TokenOperation(Action: Lookup, Data: new Dictionary<string, string> {
            ["a-2d"] = "a2d",
            ["f-2d"] = "f2d",
            // ... 100+ mappings
        }, IsMandatory: false)
    ],
    Default: null,
    IsSeverable: false
);

var normalized = reporterToken.Normalize("F. 2d");
// Returns: "f2d"
```

**Success Criteria**:
- All 6 TokenOperation actions implemented
- Number style conversion works for 1-100
- Lookup tables support case-insensitive matching
- Mandatory operations throw on failure
- Optional operations return input unchanged on failure
- 100% test coverage for token operations

**Pause and Ask Scenarios**:
- If YAML deserialization format is ambiguous
- If number word conversion patterns are unclear beyond 40

---

#### Task 1.3: Implement StringBuilder (URL and Name Builder)
**Objective**: Build string construction system from token dictionaries

**Critical Anchors**:
- Parts-based concatenation (skip parts with missing tokens)
- Edits executed before string building
- Default values from template metadata

**Agent Instructions**:
1. Create `StringBuilder.cs` in `Tokens/` folder
   - Class `StringBuilder` with:
     - `IReadOnlyList<string> Parts` - template strings with {token} placeholders
     - `IReadOnlyList<TokenOperation> Edits` - transformations before building
     - `IReadOnlyDictionary<string, string> Defaults` - metadata defaults
   - Method `string? Build(Dictionary<string, string> tokens)` - construct string
     - Merge defaults with tokens
     - Execute each edit operation
     - For each part, try string.Format with tokens
     - Skip parts that reference missing tokens (KeyNotFoundException)
     - Return joined parts or null if all parts skipped
   - Static factory `FromDictionary(Dictionary<string, object> dict)`

2. Implement string interpolation logic:
   - Use C# string interpolation or string.Format
   - Handle missing tokens gracefully (skip part)
   - URL encoding for URL builders (space → %20)

3. Create unit tests in `StringBuilderTests.cs`:
   - Test URL building with all tokens present
   - Test URL building with optional tokens missing
   - Test name building with metadata defaults
   - Test edit operations modifying tokens before building
   - Test output tokens (edits creating new tokens)
   - Test parts skipping when tokens missing

**Associated Artifacts**:
```csharp
// Example: USC URL builder
var urlBuilder = new StringBuilder(
    Parts: [
        "https://www.law.cornell.edu/uscode/text/",
        "{title}/{section}"
    ],
    Edits: [
        new TokenOperation(
            Action: Sub,
            Data: (@"[()]", ""),
            Token: "section"
        )
    ],
    Defaults: new Dictionary<string, string>()
);

var tokens = new Dictionary<string, string> {
    ["title"] = "42",
    ["section"] = "1983(b)"
};

var url = urlBuilder.Build(tokens);
// Returns: "https://www.law.cornell.edu/uscode/text/42/1983b"
```

**Success Criteria**:
- Parts concatenate correctly
- Missing optional tokens skip parts
- Edits execute before building
- URL encoding applied
- Metadata defaults work
- 100% test coverage

---

**PHASE 1 BOUNDARY**: This is the final task of Phase 1. After completion, generate a comprehensive summary:
- All classes implemented (TokenType, TokenOperation, StringBuilder)
- Test coverage percentage
- Any deviations from plan
- Readiness checklist for Phase 2

---

### PHASE 2: Template System & Regex Compilation
**Duration**: 1.5 weeks
**Focus**: Template class, YAML loading, regex compilation

**CONTEXT REQUIRED**: This is the first task of Phase 2. If starting fresh, provide Phase 1 summary.

#### Task 2.1: Implement Template Class
**Objective**: Create Template class with regex compilation and token management

**Critical Anchors**:
- Regexes compiled once at construction (RegexOptions.Compiled)
- Template inheritance supported (override-based)
- Separate regex sets for broad vs. normal matching

**Agent Instructions**:
1. Create `Template.cs` in `Templates/` folder
   - Class `Template` with:
     - `string Name` property
     - `IReadOnlyDictionary<string, TokenType> Tokens` property (ordered)
     - `IReadOnlyDictionary<string, string> Metadata` property
     - `IReadOnlyList<Regex> Regexes` - compiled normal patterns
     - `IReadOnlyList<Regex> BroadRegexes` - compiled broad patterns (case-insensitive)
     - `IReadOnlyList<string> ProcessedShortformPatterns` - with replacements applied
     - `IReadOnlyList<string> ProcessedIdformPatterns` - with replacements applied
     - `StringBuilder? UrlBuilder` property
     - `StringBuilder? NameBuilder` property
   - Constructor that:
     - Accepts all properties
     - Builds replacement dictionary from Metadata + Tokens
     - Processes each pattern with replacements
     - Compiles regexes with RegexOptions.Compiled
     - Stores processed shortform/idform patterns (not compiled yet)
   - Method `Citation? Cite(string text, bool broad = true, (int, int?) span = default)`
   - Method `List<Citation> ListLongformCitations(string text, bool broad = false, (int, int?) span = default)`
   - Static factory `FromDictionary(string name, Dictionary<string, object> dict, Dictionary<string, Template> inheritables)`

2. Implement pattern processing (port from Python `regex_mods.py`):
   - Replace `{token_name}` with `(?P<token_name>REGEX)(?!\w)`
   - Support metadata replacements (e.g., `{reporter_base}`)
   - Add word boundaries: `(?<!\w)PATTERN(?!=\w)`

3. Implement template inheritance:
   - Copy properties from parent template
   - Override with child template's values
   - Update StringBuilder defaults with child metadata

4. Create unit tests in `TemplateTests.cs`:
   - Test regex compilation
   - Test pattern replacement logic
   - Test template inheritance
   - Test broad vs. normal regex matching
   - Test token ordering preservation

**Associated Artifacts**:
```csharp
// Example: USC Template
var uscTemplate = new Template(
    Name: "U.S. Code",
    Tokens: new Dictionary<string, TokenType> {
        ["title"] = new TokenType(Regex: @"\d+"),
        ["section"] = new TokenType(Regex: @"\d+\w*"),
        ["subsection"] = new TokenType(Regex: @"\([a-z0-9]+\)", IsSeverable: true)
    },
    Metadata: new Dictionary<string, string>(),
    Patterns: [
        @"{title} U\.? ?S\.? ?C\.? (§|&#167;|&sect;) {section}(\({subsection}\))?"
    ],
    BroadPatterns: [],
    ShortformPatterns: [
        @"(§|&#167;|&sect;) {same section}(\({subsection}\))?"
    ],
    IdformPatterns: [
        @"[Ii]d\. (\({subsection}\))?"
    ],
    UrlBuilder: uscUrlBuilder,
    NameBuilder: uscNameBuilder
);
```

**Success Criteria**:
- Template regexes compile without errors
- Pattern replacement logic works
- Template inheritance functional
- Can find citations in sample text
- 100% unit test coverage

---

#### Task 2.2: YAML Template Deserialization
**Objective**: Load templates from YAML files using YamlDotNet

**Critical Anchors**:
- YamlDotNet deserializer with custom type converters
- Support for YAML anchors and aliases
- Template inheritance via `inherit` key

**Agent Instructions**:
1. Create `YamlModels.cs` in `Utilities/` folder with YAML deserialization models:
   - `TemplateYaml` class
   - `TokenTypeYaml` class
   - `TokenOperationYaml` class
   - `StringBuilderYaml` class
   - All properties nullable to support inheritance

2. Create `YamlLoader.cs` in `Utilities/` folder:
   - Static method `Dictionary<string, Template> LoadYaml(string yamlContent)`
   - Use YamlDotNet `DeserializerBuilder`
   - Handle YAML key normalization (spaces → underscores)
   - Support singular/plural key forms (pattern vs. patterns)
   - Resolve template inheritance order

3. Implement YAML-to-Template conversion:
   - Convert `TemplateYaml` → `Template`
   - Convert nested objects (TokenType, StringBuilder, etc.)
   - Preserve template order for inheritance

4. Create unit tests in `YamlLoaderTests.cs`:
   - Test loading simple template
   - Test template with inheritance
   - Test all token operation types in YAML
   - Test StringBuilder deserialization
   - Test pattern singular/plural handling

**Associated Artifacts**:
```yaml
# Example YAML template
U.S. Code:
  tokens:
    title: {regex: \d+}
    section: {regex: \d+\w*}
    subsection:
      regex: \([a-z0-9]+\)
      severable: yes
  pattern: '{title} U\.? ?S\.? ?C\.? (§|&#167;|&sect;) {section}(\({subsection}\))?'
  shortform pattern: '(§|&#167;|&sect;) {same section}(\({subsection}\))?'
  idform pattern: '[Ii]d\. (\({subsection}\))?'
  URL builder:
    parts:
      - 'https://www.law.cornell.edu/uscode/text/'
      - '{title}/{section}'
    edits:
      - token: section
        sub: ['[()]', '']
  name builder:
    parts:
      - '{title} U.S.C. § {section}'
      - '({subsection})'
```

**Success Criteria**:
- YamlDotNet deserializes all template structures
- Template inheritance resolves correctly
- Can load all 5 default YAML files
- YAML anchors/aliases work
- 100% test coverage

---

#### Task 2.3: Embed Default YAML Templates as Resources
**Objective**: Copy 5 YAML files and embed in assembly

**Critical Anchors**:
- Files must be embedded resources (Build Action: EmbeddedResource)
- Preserve YAML structure exactly from Python source
- File naming convention: lowercase with hyphens

**Agent Instructions**:
1. Create `Templates/Resources/` folder in CiteUrl.Core project
2. Copy YAML files from Python codebase to C# project:
   - `caselaw.yaml` → `caselaw.yaml`
   - `general federal law.yaml` → `general-federal-law.yaml`
   - `specific federal laws.yaml` → `specific-federal-laws.yaml`
   - `state law.yaml` → `state-law.yaml`
   - `secondary sources.yaml` → `secondary-sources.yaml`
3. Set Build Action to `EmbeddedResource` for all 5 files in .csproj:
   ```xml
   <ItemGroup>
     <EmbeddedResource Include="Templates\Resources\*.yaml" />
   </ItemGroup>
   ```
4. Create `ResourceLoader.cs` in `Utilities/` folder:
   - Method `string LoadEmbeddedYaml(string resourceName)`
   - Use `Assembly.GetManifestResourceStream()`
   - Read stream to string
5. Create unit tests:
   - Test loading each of 5 embedded resources
   - Verify YAML parses without errors
   - Count templates in each file (caselaw: ~3, state law: ~60+)

**Success Criteria**:
- All 5 YAML files embedded
- ResourceLoader can read all files
- YAML content matches Python source
- No parsing errors

---

**PHASE 2 BOUNDARY**: This is the final task of Phase 2. After completion, generate a comprehensive summary:
- Template system fully functional
- All 5 YAML files loaded and parsed
- Template count per file
- Readiness for Phase 3

---

### PHASE 3: Citation Parsing & Citator
**Duration**: 1.5 weeks
**Focus**: Citation class, Citator orchestrator, citation finding logic

**CONTEXT REQUIRED**: This is the first task of Phase 3. If starting fresh, provide Phase 2 summary including template counts.

#### Task 3.1: Implement Citation Record
**Objective**: Create immutable Citation record with parent/child relationships

**Critical Anchors**:
- Immutable `record class` for Citation
- Parent-child linkage for shortforms and idforms
- Lazy regex compilation for shortform/idform patterns (per citation instance)

**Agent Instructions**:
1. Create `Citation.cs` in `Models/` folder
   - Record class `Citation` with:
     - `string Text` - matched text
     - `(int Start, int End) Span` - position in source
     - `string SourceText` - full text searched
     - `Template Template` - template that matched
     - `Citation? Parent` - parent citation (for shortforms/ids)
     - `IReadOnlyDictionary<string, string> Tokens` - normalized values
     - `IReadOnlyDictionary<string, string> RawTokens` - captured values
     - Lazy-initialized `List<Regex> ShortformRegexes`
     - Lazy-initialized `List<Regex> IdformRegexes`
   - Properties (computed):
     - `string? Url` - build from Template.UrlBuilder
     - `string? Name` - build from Template.NameBuilder
   - Constructor:
     - Accept Match, Template, Parent?
     - Extract tokens from Match.Groups
     - Merge parent's raw tokens (inheritance)
     - Normalize tokens via Template.Tokens
     - Compile shortform/idform regexes with {same token} replacements
   - Methods:
     - `IEnumerable<Citation> GetShortformCitations()` - scan text after this citation
     - `Citation? GetIdformCitation(int? untilIndex = null)` - find next "Id." reference
     - `Citation? GetNextChild((int, int?) span = default)` - find next short or id form

2. Implement token inheritance logic:
   - Child citation copies parent's tokens
   - Stop copying at first token child overwrites
   - Maintains token order from template

3. Implement regex compilation for shortforms/idforms:
   - Replace `{same token_name}` with parent's raw token value
   - Compile as Regex with RegexOptions.Compiled
   - Cache on Citation instance (not Template)
   - Add basic "Id." regex to idform list

4. Create unit tests in `CitationTests.cs`:
   - Test citation creation from regex match
   - Test token extraction and normalization
   - Test URL building
   - Test name building
   - Test parent-child token inheritance
   - Test shortform regex compilation
   - Test finding shortform citations
   - Test id-form citation chains

**Associated Artifacts**:
```csharp
// Example: USC citation with shortform child
var parentCitation = new Citation(
    Match: regexMatch, // "42 U.S.C. § 1983"
    Template: uscTemplate,
    Parent: null
);
// parentCitation.Text = "42 U.S.C. § 1983"
// parentCitation.Tokens = { ["title"] = "42", ["section"] = "1983" }
// parentCitation.Url = "https://www.law.cornell.edu/uscode/text/42/1983"

// Later in text: "§ 1985" (shortform - same title, different section)
var childCitation = new Citation(
    Match: shortformMatch,
    Template: uscTemplate,
    Parent: parentCitation
);
// childCitation.Tokens = { ["title"] = "42", ["section"] = "1985" }
// (title inherited from parent)
```

**Success Criteria**:
- Citation records are immutable
- Token inheritance works correctly
- Shortform regex compilation successful
- Can find child citations
- URL and Name properties compute correctly
- 100% test coverage

---

#### Task 3.2: Implement Citator Class
**Objective**: Create main orchestrator for citation finding across all templates

**Critical Anchors**:
- Singleton pattern for default Citator instance
- Template dictionary (name → Template)
- Loads embedded YAML resources by default

**Agent Instructions**:
1. Create `Citator.cs` in `Templates/` folder
   - Class `Citator` with:
     - `Dictionary<string, Template> Templates` property
     - Constructor with parameters:
       - `IEnumerable<string>? defaults` - embedded resource names
       - `IEnumerable<string>? yamlPaths` - external file paths
       - `Dictionary<string, Template>? templates` - runtime templates
     - Load order: defaults → yamlPaths → templates (later overrides earlier)
   - Method `Citation? Cite(string text, bool broad = true)` - find first citation
   - Method `List<Citation> ListCitations(string text, Regex? idBreaks = null)` - find all
   - Method `string InsertLinks(string text, ...)` - hyperlink all citations
   - Method `List<Authority> ListAuthorities(string text, ...)` - group by source
   - Static method `Citator FromYaml(string yaml)` - load from YAML string
   - Method `string ToYaml()` - export templates as YAML

2. Implement default Citator singleton:
   - Static property `Citator Default { get; }` with lazy initialization
   - Loads all 5 default embedded templates
   - Thread-safe initialization

3. Implement static convenience methods (delegate to Default):
   - `static Citation? Cite(string text, bool broad = true, Citator? citator = null)`
   - `static List<Citation> ListCitations(string text, Citator? citator = null, Regex? idBreaks = null)`
   - `static string InsertLinks(string text, ...)`

4. Implement ListCitations logic (port from Python citator.py):
   - Find all longform citations from all templates
   - For each longform, find shortforms
   - Sort and remove overlapping citations (prefer longer match)
   - For each citation, find id-form chains until next citation or id-break
   - Return sorted list

5. Create unit tests in `CitatorTests.cs`:
   - Test default Citator loads all templates
   - Test finding citation from specific template
   - Test finding multiple citations in text
   - Test shortform resolution
   - Test id-form chain resolution
   - Test overlapping citation removal
   - Test custom YAML loading
   - Test template override

**Associated Artifacts**:
```csharp
// Example usage
var citator = Citator.Default;

var text = @"
  Federal law provides remedies, 42 U.S.C. § 1983, and
  also fee awards, id. at (b), which are important.
";

var citations = citator.ListCitations(text);
// citations[0]: "42 U.S.C. § 1983" (longform)
// citations[1]: "id. at (b)" (idform, parent = citations[0])

// Static convenience method
var firstCite = Citator.Cite("See 42 U.S.C. § 1983");
// Returns Citation for "42 U.S.C. § 1983"
```

**Success Criteria**:
- Citator loads all default templates
- Can find citations from multiple templates
- Shortform resolution works
- Id-form chains work
- Overlapping citation removal works
- Static methods delegate correctly
- 100% test coverage

---

#### Task 3.3: Implement Authority Grouping
**Objective**: Create Authority record and grouping logic

**Critical Anchors**:
- Immutable `record class` for Authority
- Authorities group by core tokens (ignore pincites/subsections)
- Sorted by citation count (most cited first)

**Agent Instructions**:
1. Create `Authority.cs` in `Models/` folder
   - Record class `Authority` with:
     - `Template Template`
     - `IReadOnlyDictionary<string, string> Tokens` - core tokens only
     - `List<Citation> Citations` - all citations to this authority
     - `IReadOnlyList<string> IgnoredTokens` - tokens to exclude
   - Properties (computed):
     - `string? Url` - build from Template.UrlBuilder
     - `string? Name` - build from Template.NameBuilder or derive from first longform
   - Method `bool Contains(Citation citation)` - check if citation references this authority
     - Match template name
     - Match all core tokens
     - Support severable token matching (prefix match)

2. Implement name derivation (when NameBuilder is null):
   - Find first longform citation (trace back through parents)
   - Use regex to extract text of core tokens
   - Return that text portion

3. Implement `ListAuthorities` in Citator:
   - Accept list of citations
   - Group into authorities
   - Sort by citation count (descending)
   - Return list

4. Create static convenience function:
   - `static List<Authority> ListAuthorities(IEnumerable<Citation> citations, IEnumerable<string>? ignoredTokens = null, IEnumerable<Authority>? knownAuthorities = null, bool sortByCites = true)`

5. Create unit tests in `AuthorityTests.cs`:
   - Test authority creation
   - Test citation matching (same authority)
   - Test severable token matching
   - Test citation grouping
   - Test sorting by citation count
   - Test name derivation without NameBuilder

**Associated Artifacts**:
```csharp
// Example: Group citations by authority
var text = @"
  See 42 U.S.C. § 1983 and § 1985. The statute, 42 U.S.C. § 1983,
  provides remedies. Section 1985 also applies.
";

var citations = citator.ListCitations(text);
// citations[0]: "42 U.S.C. § 1983"
// citations[1]: "§ 1985" (shortform)
// citations[2]: "42 U.S.C. § 1983" (duplicate longform)
// citations[3]: "Section 1985" (shortform)

var authorities = citator.ListAuthorities(text);
// authorities[0]: 42 U.S.C. § 1983 (3 citations)
// authorities[1]: 42 U.S.C. § 1985 (2 citations)
```

**Success Criteria**:
- Authorities group citations correctly
- Severable token matching works
- Sorting by citation count works
- Name derivation works without NameBuilder
- 100% test coverage

---

**PHASE 3 BOUNDARY**: This is the final task of Phase 3. After completion, generate a comprehensive summary:
- Citation finding fully functional
- Shortform and id-form resolution working
- Authority grouping working
- Real-world text parsing examples
- Readiness for Phase 4

---

### PHASE 4: Link Insertion, Testing & Documentation
**Duration**: 1 week
**Focus**: HTML link insertion, comprehensive testing, XML docs, NuGet packaging

**CONTEXT REQUIRED**: This is the first task of Phase 4. If starting fresh, provide Phase 3 summary with citation finding examples.

#### Task 4.1: Implement InsertLinks Method
**Objective**: Replace citations in text with HTML hyperlinks

**Critical Anchors**:
- Preserve original text positions while inserting links
- Support HTML and Markdown output formats
- Handle inline markup (strip temporarily, restore after)
- Optional: skip citations without URLs, skip redundant links

**Agent Instructions**:
1. Add `InsertLinks` method to Citator class:
   - Parameters:
     - `string text` - source text
     - `Dictionary<string, string>? attrs` - HTML attributes (default: `class="citation"`)
     - `bool addTitle` - include title attribute (default: true)
     - `bool urlOptional` - link even without URL (default: false)
     - `bool redundantLinks` - link same URL repeatedly (default: true)
     - `Regex? idBreaks` - break id-form chains
     - `bool ignoreMarkup` - strip inline tags temporarily (default: true)
     - `string markupFormat` - "html" or "markdown" (default: "html")
   - Algorithm:
     - If ignoreMarkup, strip inline tags and track positions
     - Find all citations
     - For each citation:
       - Skip if no URL and not urlOptional
       - Skip if same URL as previous and not redundantLinks
       - Build `<a href="..." class="citation" title="...">text</a>`
       - Or build `[text](url)` for markdown
     - Replace citation text with link (track running offset)
     - If ignoreMarkup, restore inline tags

2. Implement inline tag stripping (port from Python `_strip_inline_tags`):
   - Regex for HTML inline tags: `</?([inline-tag-list])( .+?>|>)`
   - Regex for Markdown bold/italic: `(?<=\s)[_*]{1,3}(?=\S)|(?<=\S)[_*]{1,3}(?=\s)`
   - Store tag positions and text
   - Remove tags from text
   - After link insertion, restore tags at adjusted positions

3. Create unit tests in `CitatorTests.cs` (InsertLinks):
   - Test HTML link insertion
   - Test Markdown link insertion
   - Test custom attributes
   - Test title attribute
   - Test skipping citations without URLs
   - Test redundant link skipping
   - Test inline tag preservation (HTML <i>, <b>)
   - Test inline tag preservation (Markdown **bold**, *italic*)
   - Test complex text with multiple citations

**Associated Artifacts**:
```csharp
// Example: Insert HTML links
var text = "See 42 U.S.C. § 1983 and Cal. Civ. Code § 1234.";

var linked = citator.InsertLinks(text);
// Output:
// See <a href="https://www.law.cornell.edu/uscode/text/42/1983"
//       class="citation"
//       title="42 U.S.C. § 1983">42 U.S.C. § 1983</a>
// and <a href="https://leginfo.legislature.ca.gov/..."
//        class="citation"
//        title="Cal. Civ. Code § 1234">Cal. Civ. Code § 1234</a>.

// Example: Markdown format
var markdown = citator.InsertLinks(text, markupFormat: "markdown");
// Output:
// See [42 U.S.C. § 1983](https://www.law.cornell.edu/uscode/text/42/1983)
// and [Cal. Civ. Code § 1234](https://leginfo.legislature.ca.gov/...).
```

**Success Criteria**:
- HTML links inserted correctly
- Markdown links inserted correctly
- Inline markup preserved
- Attributes configurable
- Edge cases handled (no URL, redundant links)
- 100% test coverage

---

#### Task 4.2: Comprehensive Integration Testing with Real Citations
**Objective**: Test end-to-end citation finding with real legal text

**Critical Anchors**:
- Test samples from each of 5 YAML template files
- Test actual legal opinion paragraphs
- Verify URL generation for all major citation types
- Port Python test cases

**Agent Instructions**:
1. Create `IntegrationTests.cs` test class
2. Add test methods for each major template category:
   - **U.S. Caselaw**: "477 U.S. 561", "123 F.3d 456", "789 Cal.App.4th 101"
   - **Federal Statutes**: "42 U.S.C. § 1983", "29 C.F.R. § 1630.2"
   - **State Statutes**: "Cal. Civ. Code § 1234", "N.Y. Penal Law § 123.45"
   - **Constitutions**: "U.S. Const. amend. XIV, § 1", "Cal. Const. art. I, § 7"
   - **Federal Rules**: "Fed. R. Civ. P. 12(b)(6)", "Fed. R. Evid. 702"
3. Add real-world text tests:
   - Full legal opinion paragraph with 5+ citations
   - Mixed citation types
   - Shortform chains
   - Id. citation chains
   - Edge cases: overlapping matches, ambiguous citations
4. Port tests from Python `tests/` directory:
   - Identify key test cases in `test_citator.py`
   - Convert to C# xUnit tests
   - Verify same results

**Test Data Sources**:
- Python test suite: `/c/Users/tlewers/source/repos/citeurl/tests/`
- Sample legal texts from citation.link website
- CourtListener opinions

**Success Criteria**:
- 20+ real citation tests across all categories
- 5+ real-world text paragraph tests
- All Python tests ported
- 100% pass rate
- URLs verified against Python version

---

#### Task 4.3: XML Documentation and README
**Objective**: Add complete XML docs and comprehensive README

**Critical Anchors**:
- XML docs on all public APIs
- README with quick start, examples, and integration guide
- NuGet package description
- Attribution to Python source

**Agent Instructions**:
1. Add XML documentation comments to all public members:
   - Classes: Citator, Template, Citation, Authority, TokenType, StringBuilder
   - Methods: All public methods
   - Properties: All public properties
   - Include `<summary>`, `<param>`, `<returns>`, `<example>` tags

2. Update README.md with:
   - Project description and goals
   - Features list
   - Installation (NuGet command)
   - Quick start example
   - Usage examples:
     - Find single citation
     - Find all citations
     - Insert hyperlinks
     - Group authorities
     - Custom templates
   - API documentation link
   - Attribution to Python citeurl library
   - License (MIT)
   - Contributing guidelines

3. Create `USAGE.md` with detailed examples:
   - Basic citation finding
   - Shortform and id-form resolution
   - Custom YAML templates
   - Integration with web apps
   - Integration with MCP servers

4. Update NuGet package metadata in .csproj:
   - Description (detailed)
   - Tags: legal, citation, parser, bluebook, yaml
   - Package README (embed README.md)
   - License: MIT
   - Repository URL and type

**Associated Artifacts**:
```xml
<!-- Example XML docs -->
/// <summary>
/// Finds the first legal citation in the given text.
/// </summary>
/// <param name="text">The text to search for citations.</param>
/// <param name="broad">
/// If true, uses case-insensitive matching and broad regex patterns
/// for better recall at the cost of potential false positives.
/// </param>
/// <returns>
/// The first <see cref="Citation"/> found in the text, or null if
/// no citations are found.
/// </returns>
/// <example>
/// <code>
/// var citation = Citator.Cite("See 42 U.S.C. § 1983");
/// Console.WriteLine(citation.Name); // "42 U.S.C. § 1983"
/// Console.WriteLine(citation.Url);  // "https://www.law.cornell.edu/..."
/// </code>
/// </example>
public static Citation? Cite(string text, bool broad = true)
```

**Success Criteria**:
- All public APIs have XML docs
- README has installation and usage examples
- NuGet metadata complete
- Documentation builds without warnings

---

#### Task 4.4: NuGet Packaging and Release Preparation
**Objective**: Package as NuGet, test local installation, prepare for publishing

**Critical Anchors**:
- Version 1.0.0 for initial release
- Include XML docs in package
- Include YAML templates as embedded resources
- README.md displayed on NuGet.org

**Agent Instructions**:
1. Update `CiteUrl.Core.csproj` package properties:
   - Version: 1.0.0
   - PackageReadmeFile: README.md
   - GenerateDocumentationFile: true
   - IncludeSymbols: true
   - SymbolPackageFormat: snupkg
   - PackageIcon: (if icon created)
   - PackageProjectUrl
   - PackageTags: legal, citation, parser, bluebook, yaml, law
   - PackageLicenseExpression: MIT

2. Create local NuGet package:
   - Run `dotnet pack -c Release`
   - Verify .nupkg file created
   - Inspect package contents (XML docs, YAML files, README)

3. Test local package installation:
   - Create test console app project
   - Add local NuGet source
   - Install CiteUrl.Core package
   - Write test code using Citator
   - Verify XML docs show in IntelliSense

4. Create CHANGELOG.md:
   - Version 1.0.0 initial release notes
   - Features list
   - Known limitations
   - Future roadmap

5. Prepare for NuGet.org publication:
   - Create API key (if publishing)
   - Document publish command: `dotnet nuget push CiteUrl.Core.1.0.0.nupkg -s https://api.nuget.org/v3/index.json -k <API_KEY>`
   - Test in sandbox.nuget.org first

**Success Criteria**:
- NuGet package builds successfully
- Package contents correct (DLL, XML, YAML, README)
- Local installation works
- IntelliSense shows XML docs
- Package metadata complete

---

**PHASE 4 BOUNDARY**: Final task complete. Generate project completion summary:
- Total implementation time
- Test coverage achieved
- NuGet package status
- Integration checklist for CourtListener MCP
- Post-launch tasks (publish to NuGet.org, GitHub release)

---

## Execution Controls

### Scope and Boundaries

**Strict Boundaries** (DO NOT modify):
- Python citeurl source code (read-only reference)
- YAML template regex patterns (copy verbatim)
- MIT license terms
- .NET 9 target framework

**Flexible Areas**:
- C# naming conventions (PascalCase vs. snake_case)
- Error handling patterns (exceptions vs. null returns)
- Test organization
- Additional utility methods

### Risk Factors

1. **YAML Template Compatibility**
   - **Risk**: Regex patterns incompatible with C# engine
   - **Mitigation**: Test each template incrementally, verify Python regex matches C# matches

2. **Regex Performance**
   - **Risk**: Compiled regexes consume excessive memory
   - **Mitigation**: Monitor memory usage, consider lazy compilation if needed

3. **Token Normalization Complexity**
   - **Risk**: Lookup tables and number conversion edge cases
   - **Mitigation**: Port Python tests verbatim, add C#-specific edge cases

4. **YamlDotNet Deserialization**
   - **Risk**: YAML anchors or complex structures fail to deserialize
   - **Mitigation**: Test each YAML file independently, add custom type converters if needed

### Decision Intelligence

**Prefer**:
- Immutability (records over mutable classes)
- Compiled regexes (performance)
- Explicit nullability (compile-time safety)
- C# idioms (LINQ, properties, nullable types)

**Avoid**:
- Dynamic typing (use generics)
- Reflection (use source generators if needed)
- Mutable shared state
- Complex inheritance hierarchies

**Pause and Ask When**:
- YAML deserialization format unclear
- Regex compatibility issue discovered
- Performance bottleneck detected
- Breaking API change needed

### Quality and Validation

**Test Requirements**:
- Minimum 80% code coverage
- All public APIs have tests
- Integration tests with real citations
- Performance tests (1000+ citations in <1 second)

**Code Quality**:
- No compiler warnings
- StyleCop / EditorConfig compliance
- XML docs on all public members
- Nullable reference types enabled

**Validation Steps** (each phase):
1. Run all tests (100% pass)
2. Check test coverage (>80%)
3. Build NuGet package (no errors)
4. Verify XML docs generated
5. Test local package installation

---

## Success Criteria (Complete Project)

✅ **Functional Parity**: All Python features ported
✅ **Test Coverage**: >80% code coverage
✅ **Performance**: Comparable or better than Python
✅ **Documentation**: XML comments, README, usage guide
✅ **NuGet Package**: Published and consumable
✅ **License**: MIT with attribution
✅ **Integration**: Works with CourtListener MCP server

---

## Post-Implementation Tasks

1. **Publish to NuGet.org**
   - Create NuGet account
   - Generate API key
   - Push package
   - Verify listing on NuGet.org

2. **GitHub Repository**
   - Create public repository
   - Push source code
   - Add CI/CD (GitHub Actions)
   - Create release tag v1.0.0

3. **Documentation Site** (optional)
   - Generate API docs with DocFX
   - Host on GitHub Pages
   - Link from NuGet package

4. **CourtListener MCP Integration**
   - Add CiteUrl.Core NuGet reference to MCP server
   - Update 6 citation tools to use CiteUrl.NET
   - Remove placeholder validation code
   - Test end-to-end

5. **Community**
   - Submit to .NET Foundation (optional)
   - Post on legal tech forums
   - Contact original Python author (Simon Raindrum Sherred) to inform of port

---

## Context Management Instructions

This plan is designed for long-running implementation across multiple sessions. Each phase boundary includes instructions for context preservation:

**After each phase**:
1. Generate summary of completed tasks
2. List any deviations from plan
3. Provide key metrics (test coverage, LOC, template count)
4. Create checklist for next phase

**At the start of each phase**:
1. Review previous phase summary
2. Confirm critical anchors still hold
3. Verify dependencies ready
4. Begin with first task

**If context is lost mid-implementation**:
1. Read latest phase summary
2. Check git log for completed work
3. Run tests to verify state
4. Resume from next pending task

---

## Appendix: Python-to-C# Mapping Reference

| Python Class | C# Class | Type | Notes |
|-------------|----------|------|-------|
| `Citation` | `Citation` | `record class` | Immutable with computed properties |
| `Authority` | `Authority` | `record class` | Immutable with citation list |
| `Template` | `Template` | `class` | Has compiled regexes, stateful |
| `Citator` | `Citator` | `class` | Template dictionary manager |
| `TokenType` | `TokenType` | `record` | Token definition |
| `TokenOperation` | `TokenOperation` | `record` | Normalization operation |
| `StringBuilder` | `StringBuilder` | `class` | URL/name builder |

| Python Method | C# Method | Notes |
|--------------|-----------|-------|
| `cite(text)` | `Cite(string text)` | Static method on Citator |
| `list_cites(text)` | `ListCitations(string text)` | Descriptive name |
| `insert_links(text)` | `InsertLinks(string text)` | Same name |
| `list_authorities(text)` | `ListAuthorities(string text)` | Descriptive name |
| `@property name` | `string? Name { get; }` | Computed property |
| `@property URL` | `string? Url { get; }` | Computed property |

| Python Feature | C# Equivalent | Notes |
|---------------|---------------|-------|
| Type hints | Strong typing | Native to C# |
| `@property` | Properties | `{ get; }` |
| `@classmethod` | Static methods | `static` |
| List comprehensions | LINQ | `.Select()`, `.Where()` |
| `None` | `null` | Nullable types |
| `dict` | `Dictionary<K,V>` | Generic collection |
| `list` | `List<T>` | Generic collection |
| `re.compile()` | `new Regex()` | `RegexOptions.Compiled` |

---

**END OF IMPLEMENTATION PLAN**

This plan is ready for review and approval. Once approved, implementation can begin with Phase 1, Task 1.1.
