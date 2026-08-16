using BrokerOS.Domain.Search;

namespace BrokerOS.Domain.Tests;

public sealed class SearchRelevanceTests
{
    [Fact]
    public void Exact_beats_prefix_beats_partial()
    {
        Assert.True(SearchRelevance.Rank("Alpha Logistics", "Alpha Logistics") < SearchRelevance.Rank("Alpha Logistics", "Alpha"));
        Assert.True(SearchRelevance.Rank("Alpha Logistics", "Alpha") < SearchRelevance.Rank("Alpha Logistics", "Log"));
    }

    [Fact]
    public void Compact_vehicle_numbers_match_without_hyphens()
    {
        Assert.Equal(SearchRelevance.Exact, SearchRelevance.Rank("MH-01-AB-4321", "MH01AB4321"));
        Assert.Equal(SearchRelevance.Prefix, SearchRelevance.Rank("MH-01-AB-4321", "MH-01"));
    }

    [Fact]
    public void Best_picks_the_strongest_field()
    {
        Assert.Equal(
            SearchRelevance.Partial,
            SearchRelevance.Best("90000", "Alpha", "+91 90000 00001"));
    }
}
