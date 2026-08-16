using BrokerOS.Application.Quotations;

namespace BrokerOS.Application.Tests;

public sealed class CreateQuotationRequestValidatorTests
{
    private readonly CreateQuotationRequestValidator _validator = new();

    [Fact]
    public void Existing_insurer_and_premium_are_valid()
    {
        var result = _validator.Validate(new CreateQuotationRequest
        {
            InsurerPublicId = Guid.NewGuid(),
            PremiumAmount = 850000m,
            CoverageSummary = "Fire + burglary"
        });

        Assert.True(result.IsValid);
    }

    [Fact]
    public void New_insurer_name_without_public_id_is_valid()
    {
        var result = _validator.Validate(new CreateQuotationRequest
        {
            NewInsurerName = "Tata AIG",
            PremiumAmount = 1000m
        });

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Both_insurer_sources_are_rejected()
    {
        var result = _validator.Validate(new CreateQuotationRequest
        {
            InsurerPublicId = Guid.NewGuid(),
            NewInsurerName = "Tata AIG",
            PremiumAmount = 1000m
        });

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Neither_insurer_source_is_rejected()
    {
        var result = _validator.Validate(new CreateQuotationRequest { PremiumAmount = 1000m });

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Negative_premium_is_rejected()
    {
        var result = _validator.Validate(new CreateQuotationRequest
        {
            NewInsurerName = "Tata AIG",
            PremiumAmount = -1m
        });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == "PremiumAmount");
    }
}
