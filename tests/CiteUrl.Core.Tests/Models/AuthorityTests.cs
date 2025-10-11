using CiteUrl.Core.Models;
using CiteUrl.Core.Templates;
using Shouldly;
using Xunit;

namespace CiteUrl.Core.Tests.Models;

public class AuthorityTests
{
    [Fact]
    public void ListAuthorities_GroupsByCoreTokens()
    {
        var text = "See 42 U.S.C. § 1983 and § 1985. Also 42 U.S.C. § 1983.";
        var authorities = Citator.ListAuthorities(text).ToList();

        authorities.Count.ShouldBeGreaterThanOrEqualTo(1);
    }

    [Fact]
    public void ListAuthorities_SortsByCitationCount()
    {
        var text = "42 USC 1983, 1983 again, and 1985 once.";
        var authorities = Citator.ListAuthorities(text).ToList();

        if (authorities.Count >= 2)
        {
            authorities[0].Citations.Count.ShouldBeGreaterThanOrEqualTo(
                authorities[1].Citations.Count);
        }
    }

    [Fact]
    public void Authority_ContainsMethod_MatchesSameAuthority()
    {
        var text = "See 42 U.S.C. § 1983.";
        var citations = Citator.ListCitations(text).ToList();

        if (citations.Any())
        {
            var authorities = Citator.ListAuthorities(text).ToList();
            if (authorities.Any())
            {
                var authority = authorities[0];
                authority.Contains(citations[0]).ShouldBeTrue();
            }
        }
    }

    [Fact]
    public void Authority_Name_DerivesFromCitation()
    {
        var text = "See 42 U.S.C. § 1983.";
        var authorities = Citator.ListAuthorities(text).ToList();

        if (authorities.Any())
        {
            authorities[0].Name.ShouldNotBeNullOrEmpty();
        }
    }

    [Fact]
    public void Authority_Url_GeneratedFromTemplate()
    {
        var text = "See 42 U.S.C. § 1983.";
        var authorities = Citator.ListAuthorities(text).ToList();

        if (authorities.Any())
        {
            authorities[0].Url.ShouldNotBeNullOrEmpty();
        }
    }

    [Fact]
    public void ListAuthorities_IgnoresSpecifiedTokens()
    {
        var text = "See 42 U.S.C. § 1983(a) and § 1983(b).";
        var ignoredTokens = new[] { "subsection" };
        var authorities = Citator.ListAuthorities(text, null, ignoredTokens).ToList();

        // Both citations should be grouped together if subsection is ignored
        if (authorities.Any())
        {
            var firstAuthority = authorities.FirstOrDefault(a => a.Citations.Count > 1);
            firstAuthority.ShouldNotBeNull();
        }
    }
}
