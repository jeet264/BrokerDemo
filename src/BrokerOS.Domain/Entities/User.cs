using BrokerOS.Domain.Common;
using BrokerOS.Domain.Enums;

namespace BrokerOS.Domain.Entities;

public class User : Entity, ITenantOwned, IAudited, ISoftDeletable
{
    public long OrganizationId { get; set; }

    public string Email { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public UserRole Role { get; set; } = UserRole.Employee;

    public string PasswordHash { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public DateTime? LastLoginAtUtc { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? ModifiedAtUtc { get; set; }

    public string? CreatedBy { get; set; }

    public string? ModifiedBy { get; set; }

    public bool IsDeleted { get; set; }

    public Organization Organization { get; set; } = null!;
}
