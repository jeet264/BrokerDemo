using BrokerOS.Domain.Enums;

namespace BrokerOS.Application.Clients;

public sealed class UpdateClientRequest
{
    public string ClientCode { get; set; } = string.Empty;

    public string CompanyName { get; set; } = string.Empty;

    public ClientType ClientType { get; set; }

    public string? Industry { get; set; }

    public string Email { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;

    public string? AlternatePhone { get; set; }

    public string AddressLine1 { get; set; } = string.Empty;

    public string? AddressLine2 { get; set; }

    public string City { get; set; } = string.Empty;

    public string State { get; set; } = string.Empty;

    public string PostalCode { get; set; } = string.Empty;

    public string Country { get; set; } = "India";

    /// <summary>PublicId of the assignee. Null = unassigned. Must belong to this org (resolved under the tenant filter).</summary>
    public Guid? AssignedUserPublicId { get; set; }

    public string? Notes { get; set; }

    public bool IsActive { get; set; } = true;
}
