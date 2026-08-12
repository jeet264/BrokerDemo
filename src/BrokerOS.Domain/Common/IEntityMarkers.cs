namespace BrokerOS.Domain.Common;

public interface IHasPublicId
{
    Guid PublicId { get; set; }
}

public interface ITenantOwned
{
    long OrganizationId { get; set; }
}

public interface ISoftDeletable
{
    bool IsDeleted { get; set; }
}

public interface IAudited
{
    DateTime CreatedAtUtc { get; set; }

    DateTime? ModifiedAtUtc { get; set; }

    string? CreatedBy { get; set; }

    string? ModifiedBy { get; set; }
}
