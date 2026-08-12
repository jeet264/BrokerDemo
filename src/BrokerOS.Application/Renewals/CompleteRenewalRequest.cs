namespace BrokerOS.Application.Renewals;

public sealed class CompleteRenewalRequest
{
    public DateOnly? NewExpiryDate { get; set; }

    public decimal? Premium { get; set; }

    public decimal? SumInsured { get; set; }

    public decimal? CommissionPercentage { get; set; }

    public string? Notes { get; set; }
}
