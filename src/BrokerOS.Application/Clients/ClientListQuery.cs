using BrokerOS.Domain.Enums;

namespace BrokerOS.Application.Clients;

public sealed class ClientListQuery
{
    public string? Search { get; set; }

    public ClientType? ClientType { get; set; }

    public string? Industry { get; set; }

    public Guid? AssignedUserPublicId { get; set; }

    public bool? IsActive { get; set; }

    public string? SortBy { get; set; }

    public string? SortDir { get; set; }

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 20;
}
