# CiteUrl.NET Usage Guide

Comprehensive guide to using CiteUrl.NET for parsing and hyperlinking legal citations.

## Table of Contents

- [Basic Usage](#basic-usage)
- [Citation Types](#citation-types)
- [Link Insertion](#link-insertion)
- [Authority Grouping](#authority-grouping)
- [Advanced Features](#advanced-features)
- [Custom Templates](#custom-templates)

## Basic Usage

### Finding a Single Citation

The simplest way to find a citation is using the static `Citator.Cite()` method:

```csharp
using CiteUrl.Core.Templates;

var text = "See 42 U.S.C. § 1983 for civil rights remedies.";
var citation = Citator.Cite(text);

if (citation != null)
{
    Console.WriteLine($"Found: {citation.Text}");
    Console.WriteLine($"URL: {citation.Url}");
    Console.WriteLine($"Name: {citation.Name}");
    Console.WriteLine($"Position: {citation.Span.Start}-{citation.Span.End}");
}
```

Output:
```
Found: 42 U.S.C. § 1983
URL: https://www.law.cornell.edu/uscode/text/42/1983
Name: 42 U.S.C. § 1983
Position: 4-22
```

### Finding All Citations

Use `ListCitations()` to find all citations in a document:

```csharp
var text = @"
    The statute at 42 U.S.C. § 1983 provides a cause of action.
    Fees are available under § 1988. See also Miranda v. Arizona,
    384 U.S. 436 (1966), and Fed. R. Civ. P. 12(b)(6).
";

foreach (var citation in Citator.ListCitations(text))
{
    Console.WriteLine($"[{citation.Span.Start}] {citation.Name}");
}
```

Output:
```
[23] 42 U.S.C. § 1983
[81] § 1988
[107] Miranda v. Arizona, 384 U.S. 436
[156] Fed. R. Civ. P. 12(b)(6)
```

### Accessing Citation Properties

```csharp
var citation = Citator.Cite("See 42 U.S.C. § 12112(b)(5)(A)");

// Raw text as it appears
Console.WriteLine(citation.Text); // "42 U.S.C. § 12112(b)(5)(A)"

// Canonical citation name
Console.WriteLine(citation.Name); // "42 U.S.C. § 12112(b)(5)(A)"

// Generated URL
Console.WriteLine(citation.Url); // "https://www.law.cornell.edu/uscode/text/42/12112"

// Template used
Console.WriteLine(citation.Template.Name); // "U.S. Code"

// Token values
foreach (var token in citation.Tokens)
{
    Console.WriteLine($"{token.Key}: {token.Value}");
}
// Output:
// title: 42
// section: 12112
// subsection: (b)(5)(A)
```

## Citation Types

### U.S. Code (Federal Statutes)

```csharp
// Standard form
Citator.Cite("42 U.S.C. § 1983");
Citator.Cite("42 U.S.C. § 2000e");

// With subsections
Citator.Cite("42 U.S.C. § 12112(b)(5)(A)");

// Abbreviated form
Citator.Cite("42 USC 1983");

// Multiple sections
Citator.Cite("42 U.S.C. §§ 1981-1983");
```

### Code of Federal Regulations

```csharp
// Standard form
Citator.Cite("29 C.F.R. § 1630.2");
Citator.Cite("29 C.F.R. § 1630.2(h)");

// Abbreviated
Citator.Cite("29 CFR 1630.2");
```

### U.S. Supreme Court Cases

```csharp
Citator.Cite("Brown v. Board of Education, 347 U.S. 483 (1954)");
Citator.Cite("Miranda v. Arizona, 384 U.S. 436, 444 (1966)");
Citator.Cite("477 U.S. 561"); // Volume and page only
```

### Federal Circuit Courts

```csharp
// F.2d (Second Series)
Citator.Cite("Smith v. Jones, 123 F.2d 456 (9th Cir. 1987)");

// F.3d (Third Series)
Citator.Cite("789 F.3d 101 (2d Cir. 2015)");

// F.4th (Fourth Series)
Citator.Cite("456 F.4th 789");
```

### State Case Law

```csharp
// California
Citator.Cite("People v. Smith, 123 Cal. App. 4th 456 (2005)");
Citator.Cite("456 Cal.2d 789");

// New York
Citator.Cite("Smith v. Jones, 123 N.Y.2d 456");

// Texas
Citator.Cite("456 S.W.3d 789 (Tex. 2010)");
```

### State Codes

```csharp
// California
Citator.Cite("Cal. Civ. Code § 1234");
Citator.Cite("Cal. Penal Code § 187");

// New York
Citator.Cite("N.Y. Penal Law § 120.05");
Citator.Cite("N.Y. C.P.L.R. § 301");

// Texas
Citator.Cite("Tex. Gov't Code § 311.005");
Citator.Cite("Tex. Penal Code § 22.01");
```

### Constitutions

```csharp
// U.S. Constitution
Citator.Cite("U.S. Const. art. I, § 8");
Citator.Cite("U.S. Const. amend. XIV, § 1");
Citator.Cite("U.S. Const. amend. I");

// State Constitutions
Citator.Cite("Cal. Const. art. I, § 7");
Citator.Cite("N.Y. Const. art. VI, § 1");
```

### Federal Rules

```csharp
// Federal Rules of Civil Procedure
Citator.Cite("Fed. R. Civ. P. 12(b)(6)");
Citator.Cite("Fed. R. Civ. P. 56");

// Federal Rules of Evidence
Citator.Cite("Fed. R. Evid. 702");
Citator.Cite("Fed. R. Evid. 401");

// Federal Rules of Appellate Procedure
Citator.Cite("Fed. R. App. P. 4");

// Federal Rules of Criminal Procedure
Citator.Cite("Fed. R. Crim. P. 11");
```

## Link Insertion

### HTML Links

Insert HTML hyperlinks for all citations:

```csharp
var text = "See 42 U.S.C. § 1983 and Cal. Civ. Code § 1234.";
var html = Citator.Default.InsertLinks(text);

Console.WriteLine(html);
```

Output:
```html
See <a href="https://www.law.cornell.edu/uscode/text/42/1983" class="citation" title="42 U.S.C. § 1983">42 U.S.C. § 1983</a>
and <a href="https://leginfo.legislature.ca.gov/..." class="citation" title="Cal. Civ. Code § 1234">Cal. Civ. Code § 1234</a>.
```

### Custom HTML Attributes

```csharp
var attrs = new Dictionary<string, string>
{
    ["class"] = "legal-citation",
    ["data-type"] = "statute",
    ["target"] = "_blank"
};

var html = Citator.Default.InsertLinks(text, attrs: attrs);
```

Output:
```html
<a href="..." class="legal-citation" data-type="statute" target="_blank" title="42 U.S.C. § 1983">42 U.S.C. § 1983</a>
```

### Markdown Links

```csharp
var markdown = Citator.Default.InsertLinks(text, markupFormat: "markdown");

Console.WriteLine(markdown);
```

Output:
```markdown
See [42 U.S.C. § 1983](https://www.law.cornell.edu/uscode/text/42/1983)
and [Cal. Civ. Code § 1234](https://leginfo.legislature.ca.gov/...).
```

### Controlling Link Behavior

```csharp
// Skip citations without URLs
var html = Citator.Default.InsertLinks(text, urlOptional: false);

// Skip redundant links (same URL repeated)
var html = Citator.Default.InsertLinks(text, redundantLinks: false);

// Disable title attributes
var html = Citator.Default.InsertLinks(text, addTitle: false);
```

## Authority Grouping

Group multiple citations that refer to the same legal authority:

```csharp
var text = @"
    See 42 U.S.C. § 1983 for civil rights remedies.
    Fees are available under § 1988.
    Both § 1983 and § 1985 provide causes of action.
    The seminal case is 42 U.S.C. § 1983.
";

var authorities = Citator.ListAuthorities(text);

foreach (var authority in authorities)
{
    Console.WriteLine($"\nAuthority: {authority.Name}");
    Console.WriteLine($"Citations: {authority.Citations.Count}");
    Console.WriteLine($"URL: {authority.Url}");

    foreach (var cite in authority.Citations)
    {
        Console.WriteLine($"  - {cite.Text} at position {cite.Span.Start}");
    }
}
```

Output:
```
Authority: 42 U.S.C. § 1983
Citations: 3
URL: https://www.law.cornell.edu/uscode/text/42/1983
  - 42 U.S.C. § 1983 at position 8
  - § 1983 at position 89
  - 42 U.S.C. § 1983 at position 125

Authority: 42 U.S.C. § 1988
Citations: 1
URL: https://www.law.cornell.edu/uscode/text/42/1988
  - § 1988 at position 60

Authority: 42 U.S.C. § 1985
Citations: 1
URL: https://www.law.cornell.edu/uscode/text/42/1985
  - § 1985 at position 97
```

### Ignoring Specific Tokens

When grouping, you can ignore certain tokens (like page numbers):

```csharp
var ignoredTokens = new[] { "page", "pincite" };
var authorities = Citator.Default.ListAuthorities(
    citations,
    ignoredTokens: ignoredTokens
);
```

## Advanced Features

### Shortform Citations

CiteUrl.NET automatically resolves shortform citations:

```csharp
var text = @"
    The statute at 42 U.S.C. § 1983 provides remedies.
    Under § 1985, conspiracy claims are available.
    Section 1988 allows fee awards.
";

var citations = Citator.ListCitations(text).ToList();

// All three citations are found and linked to their parent
Console.WriteLine(citations[0].Text); // "42 U.S.C. § 1983" (longform)
Console.WriteLine(citations[1].Text); // "§ 1985" (shortform of parent)
Console.WriteLine(citations[2].Text); // "Section 1988" (shortform)
```

### Id. Citations

"Id." citations are automatically linked to their parent:

```csharp
var text = @"
    See Miranda v. Arizona, 384 U.S. 436, 444 (1966).
    The Court held that id. at 478-479.
";

var citations = Citator.ListCitations(text).ToList();

Console.WriteLine(citations[0].Text); // "Miranda v. Arizona, 384 U.S. 436, 444"
Console.WriteLine(citations[1].Text); // "id. at 478-479"
Console.WriteLine(citations[1].Parent?.Text); // "Miranda v. Arizona, 384 U.S. 436, 444"
```

### Using the Citator Instance

For better performance when processing many documents, reuse a Citator instance:

```csharp
// Use the default singleton
var citator = Citator.Default;

// Process many documents
foreach (var document in documents)
{
    var citations = citator.ListCitations(document.Text);
    // ...
}
```

### Streaming Large Documents

`ListCitations()` returns `IEnumerable<Citation>` for memory efficiency:

```csharp
// Citations are found lazily as you iterate
foreach (var citation in Citator.ListCitations(veryLargeText))
{
    // Process each citation as found
    await SaveToDatabase(citation);

    // Can break early
    if (citation.Span.Start > 10000)
        break;
}
```

## Custom Templates

### Creating Custom YAML Templates

Define your own citation formats:

```csharp
var yaml = @"
Patent Citation:
  tokens:
    patent_number: '\d{1,2},\d{3},\d{3}'
  pattern: 'U\.S\. Patent No\. {patent_number}'
  URL builder:
    parts:
      - 'https://patents.google.com/patent/US'
      - '{patent_number}'
  name builder: 'U.S. Patent No. {patent_number}'
";

var citator = Citator.FromYaml(yaml);
var citation = citator.Cite("See U.S. Patent No. 5,123,456");

Console.WriteLine(citation?.Url);
// Output: https://patents.google.com/patent/US5,123,456
```

### Template Inheritance

Extend existing templates:

```csharp
var yaml = @"
My State Code:
  inherit: 'California Civil Code'
  URL builder:
    parts: ['https://my-custom-site.com/', '{section}']
";
```

### Token Normalization

Apply transformations to captured tokens:

```csharp
var yaml = @"
Custom Citation:
  tokens:
    section:
      regex: '\d+'
      edits:
        - lpad: [5, '0']  # Left-pad with zeros to 5 digits
  pattern: 'Sec\. {section}'
  URL builder:
    parts: ['https://example.com/', '{section}']
";

var citator = Citator.FromYaml(yaml);
var citation = citator.Cite("Sec. 123");

Console.WriteLine(citation?.Url);
// Output: https://example.com/00123
```

## Best Practices

### 1. Reuse Citator Instances

```csharp
// Good - reuses singleton
var citations = Citator.Default.ListCitations(text);

// Better - for custom templates
var citator = Citator.FromYaml(myYaml);
// Reuse this instance for all documents
```

### 2. Use Streaming for Large Documents

```csharp
// Good - streams results
await foreach (var citation in Citator.ListCitations(largeText).ToAsyncEnumerable())
{
    await ProcessCitation(citation);
}
```

### 3. Handle Nulls Gracefully

```csharp
// Some citations may not have URLs
var citation = Citator.Cite(text);
if (citation?.Url != null)
{
    Console.WriteLine(citation.Url);
}
```

### 4. Dependency Injection

```csharp
// In production apps, use DI
services.AddCiteUrl();

// Then inject ICitator
public MyService(ICitator citator) { }
```

## Performance Considerations

- **Thread Safety**: `Citator.Default` is thread-safe and can be used concurrently
- **Regex Compilation**: All regexes are compiled once at initialization
- **Timeout Protection**: Built-in 1-second regex timeout prevents ReDoS attacks
- **Memory Efficiency**: Streaming enumeration avoids loading all citations into memory

## Error Handling

```csharp
try
{
    var citation = Citator.Cite(text);
}
catch (CiteUrlException ex)
{
    Console.WriteLine($"Citation parsing error: {ex.Message}");
}
catch (RegexMatchTimeoutException ex)
{
    Console.WriteLine("Regex timeout - possible ReDoS attack");
}
```

## Further Reading

- [README.md](README.md) - Quick start guide
- [API Documentation](https://github.com/tlewers/citeurl-dotnet/wiki) - Full API reference
- [Python Original](https://github.com/raindrum/citeurl) - Original Python library documentation
- [Bluebook Citation Guide](https://www.legalbluebook.com/) - Legal citation style guide
