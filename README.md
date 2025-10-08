# CiteUrl.NET

A .NET 9 library for parsing and hyperlinking legal citations. This is a C# port of the Python [citeurl](https://github.com/raindrum/citeurl) library by Simon Raindrum Sherred.

## Features

- **130+ Citation Formats**: US federal and state case law, statutes, regulations, and more
- **YAML-Based Templates**: Extensible citation patterns via YAML configuration
- **Thread-Safe**: Fully immutable design for concurrent use
- **Memory Efficient**: Streaming enumeration for large documents
- **DI-Friendly**: Optional dependency injection extensions
- **Regex Timeout Protection**: Built-in ReDoS protection

## Installation

```bash
dotnet add package CiteUrl.Core
```

For dependency injection support:
```bash
dotnet add package CiteUrl.Extensions.DependencyInjection
```

## Quick Start

```csharp
using CiteUrl.Core;

// Find first citation
var citation = Citator.Cite("See 42 U.S.C. § 1983 for details.");
Console.WriteLine(citation?.Url); // https://www.law.cornell.edu/uscode/text/42/1983

// Find all citations
var text = "Federal law, 42 U.S.C. § 1983, provides remedies.";
foreach (var cite in Citator.Default.ListCitations(text))
{
    Console.WriteLine($"{cite.Name} -> {cite.Url}");
}
```

## License

MIT License. Original Python library by Simon Raindrum Sherred.

## Status

🚧 **Alpha** - In active development
