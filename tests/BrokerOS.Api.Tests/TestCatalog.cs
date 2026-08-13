namespace BrokerOS.Api.Tests;

public sealed class TestCatalog
{
    public required Guid OrgAClientAssignedPublicId { get; init; }

    public required Guid OrgAClientOtherPublicId { get; init; }

    public required Guid OrgAPolicyNearPublicId { get; init; }

    public required Guid OrgAPolicyFarPublicId { get; init; }

    public required Guid OrgARenewalNearPublicId { get; init; }

    public required long OrgARenewalNearId { get; init; }

    public required Guid OrgATaskPublicId { get; init; }

    public required Guid OrgAInsurerPublicId { get; init; }

    public required Guid OrgAEmployeePublicId { get; init; }

    public required Guid OrgBClientPublicId { get; init; }

    public required Guid OrgBPolicyPublicId { get; init; }

    public required Guid OrgBRenewalPublicId { get; init; }

    public required decimal NearPremium { get; init; }

    public required decimal FarPremium { get; init; }
}
