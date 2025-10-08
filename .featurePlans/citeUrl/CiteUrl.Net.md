# CiteUrl.NET - Legal Citation Parsing Library for .NET

**Status**: PLANNING - Placeholder for future planning session
**Created**: 2025-10-06
**Source**: Port of Python `citeurl` library to .NET C#
**Python Repository**: https://github.com/raindrum/citeurl
**License**: MIT (allows porting)

---

## Project Mission

Port the Python `citeurl` library to .NET C# as a standalone, reusable NuGet package that provides legal citation parsing and URL generation for the .NET ecosystem.

---

## Key Information for Planning Session

### What is CiteURL?

CiteURL is an open-source tool that automatically parses legal citations and generates hyperlinks to free online legal resources. It supports over 130 sources of U.S. law with Bluebook-style citation support.

### Python Implementation Analysis

**Codebase Size**: ~2,188 lines of Python across 19 files

**Core Architecture**:
- **Template Class**: Represents citation templates, handles pattern matching and parsing
- **Citator Class**: Manages multiple templates, provides high-level extraction API
- **YAML Templates**: 5 template files with regex patterns for different legal sources
- **Multi-stage regex compilation**: Token-based extraction with nested matching

**YAML Templates** (5 files in `citeurl/templates/`):
1. `caselaw.yaml` - U.S. case law citations (reporters, volumes, pages)
2. `general federal law.yaml` - Federal statutes, CFR
3. `specific federal laws.yaml` - Specific federal rules
4. `state law.yaml` - State statutes and constitutions
5. `secondary sources.yaml` - Law reviews, etc.

**Core API Methods**:
- `cite()` - Find first citation in text
- `list_cites()` - Extract all citations from text
- `insert_links()` - Convert citations to HTML hyperlinks
- `list_authorities()` - Aggregate citations by source

**Python-Specific Features to Port**:
- Type hints → C# strong typing (natural fit)
- YAML deserialization → Use YamlDotNet
- Regex patterns → System.Text.RegularExpressions (mostly compatible)
- Dynamic method generation → C# Reflection or source generators
- Nested functions → C# local functions or private methods

### Porting Feasibility Assessment

✅ **Straightforward**:
- YAML templates are language-agnostic
- Regex patterns mostly compatible
- Class structure maps cleanly to C#
- Type system is actually better in C#

⚠️ **Moderate Complexity**:
- Multi-stage regex compilation logic
- Token-based matching system
- Context-aware citation extraction

**Estimated Effort**: 4-6 weeks (core port + comprehensive testing + documentation)

### Proposed .NET Solution Structure

```
citeurl-dotnet/
├── .featurePlans/
│   └── citeUrl/
│       └── CiteUrl.Net.md (this file)
├── src/
│   └── CiteUrl.Core/
│       ├── CiteUrl.Core.csproj
│       ├── Citator.cs
│       ├── Template.cs
│       ├── Citation.cs
│       ├── Authority.cs
│       ├── Tokens.cs
│       └── Templates/
│           ├── caselaw.yaml
│           ├── general-federal-law.yaml
│           ├── specific-federal-laws.yaml
│           ├── state-law.yaml
│           └── secondary-sources.yaml
├── tests/
│   └── CiteUrl.Core.Tests/
│       └── CiteUrl.Core.Tests.csproj
├── samples/
│   └── CiteUrl.Cli/ (optional - command-line tool)
├── CiteUrl.sln
└── README.md
```

### Target Framework

**Recommendation**: .NET Standard 2.1 for maximum compatibility (works with .NET 5+, .NET Framework 4.7.2+)
**Alternative**: .NET 9 if we only need modern .NET support

### Key Dependencies (NuGet Packages)

**Required**:
- `YamlDotNet` - YAML template parsing
- `System.Text.RegularExpressions` - Built-in, no package needed

**Testing**:
- `xunit` - Testing framework
- `FluentAssertions` - Readable test assertions
- `Moq` (if needed for mocking)

### Design Decisions Needed

1. **Target Framework**: .NET Standard 2.1 vs .NET 9?
2. **API Design**: Match Python API exactly or make it more C#-idiomatic?
3. **Template Loading**: Embedded resources vs file-based vs both?
4. **Async/Await**: Should parsing be async for large texts?
5. **Immutability**: Should Citation/Authority be records or classes?
6. **Null Handling**: Nullable reference types enabled?
7. **Performance**: Regex compilation caching strategy?
8. **Extensibility**: Allow custom YAML templates from users?

### Success Criteria

✅ **Functional Parity**: All core Python features ported
✅ **Test Coverage**: >80% code coverage with comprehensive test suite
✅ **Performance**: Comparable or better than Python version
✅ **Documentation**: XML comments, README, usage examples
✅ **NuGet Package**: Published and consumable
✅ **License**: MIT license maintained with proper attribution

### Integration with CourtListener MCP Server

**MCP Server Location**: `C:\Users\tlewers\source\repos\court-listener-mcp\`

**Integration Method**:
- MCP server references `CiteUrl.Core` NuGet package (local or published)
- Used by 6 citation tools: LookupCitation, BatchLookupCitations, VerifyCitationFormat, ParseCitation, ExtractCitationsFromText, EnhancedCitationLookup

**Fallback Strategy**:
- MCP server starts with minimal validation (Option 3)
- Swap in CiteUrl.NET when ready (no architecture changes needed)

### Reference Material

**Python Repository**: https://github.com/raindrum/citeurl
**License**: MIT (Copyright 2020 Simon Raindrum Sherred)
**Live Demo**: https://citation.link
**Documentation**: See Python repo README and docs/

### Next Steps for Planning Session

When ready to plan this project:

1. **Load this plan** with `/leash` and reference this file
2. **Review Python codebase** in detail (core files: citator.py, citation.py, templates/*.yaml)
3. **Define C# class structure** (Template, Citator, Citation, Authority, Tokens)
4. **Plan YAML template porting** (verify regex compatibility)
5. **Design test strategy** (port Python tests + add .NET-specific tests)
6. **Decide on API style** (match Python vs C# idiomatic)
7. **Create implementation phases** (likely 3-4 phases for iterative development)
8. **Define NuGet packaging** strategy

---

## Notes

- This is a **separate project** from the CourtListener MCP server
- Primary goal: Create reusable .NET library for the entire .NET ecosystem
- Secondary goal: Provide citation parsing for MCP server
- Community impact: First .NET legal citation parsing library
- Python reference implementation cloned at: `C:\Users\tlewers\source\repos\citeurl\`

---

**Project Phase**: Pre-planning (placeholder created)
**Next Action**: Complete CourtListener MCP server first, then return for full planning session
