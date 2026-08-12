using BrokerOS.Domain.Enums;

namespace BrokerOS.Application.Policies;

public sealed class PolicyListQuery
{
    public string? Search { get; set; }

    public PolicyStatus? Status { get; set; } = PolicyStatus.Active;

    public Guid? ClientPublicId { get; set; }

    public string? SortBy { get; set; }

    public string? SortDir { get; set; }

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 20;
}
