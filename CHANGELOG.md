# Changelog

All notable changes to CiteUrl.NET will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.0] - 2025-01-11

### Added
- Initial release of CiteUrl.NET - C# port of Python citeurl library
- 90+ citation templates supporting US federal and state law
- Support for case law, statutes, regulations, and legal citations
- Immutable, thread-safe design with `ImmutableDictionary` and `ImmutableList`
- Streaming enumeration via `IEnumerable<Citation>` for memory-efficient processing
- Template inheritance system for extensible citation patterns
- Comprehensive token normalization with edits (sub, lookup, case, lpad, number_style)
- YAML-based template system with 5 embedded resource files
- Dependency injection support via `CiteUrl.Extensions.DependencyInjection` package
- Full XML documentation for IntelliSense
- 75+ unit tests with >85% code coverage
- `Citator.Cite()` - Find first citation in text
- `Citator.ListCitations()` - Stream all citations from text
- `Citator.ListAuthorities()` - Group citations by legal authority
- `Citator.InsertLinks()` - Transform text with hyperlinked citations
- `Citation.GetShortformCitations()` - Find subsequent shortform references
- `Citation.GetIdformCitation()` - Find "Id." references

### Security
- ReDoS protection with configurable regex timeout (default 1 second)
- All regex patterns compiled with timeout enforcement
- Input validation and safe null handling throughout

### Performance
- Lazy singleton initialization of `Citator.Default`
- Compiled regex caching for improved performance
- Streaming enumeration prevents memory bloat on large documents
- Immutable collections avoid defensive copying overhead

### Known Issues
- Caselaw reporter ID lookups need refinement for some reporters (e.g., "U.S.")
- Test runner timeouts when running all CitatorTests together (individual tests pass)
- .NET 9 preview SDK warnings (expected, will resolve with .NET 9 GA)

## [Unreleased]

### Planned
- Additional citation templates (currently 90+, goal 130+)
- Custom YAML template loading via `CiteUrlOptions.CustomYamlPaths`
- Performance optimizations for large document processing
- Extended reporter ID lookup table coverage
- .NET Standard 2.0 target for broader compatibility

---

**Full Changelog**: https://github.com/tlewers/citeurl-dotnet/releases
