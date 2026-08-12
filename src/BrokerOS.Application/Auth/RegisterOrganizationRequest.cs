namespace BrokerOS.Application.Auth;

public sealed class RegisterOrganizationRequest
{
    public string OrganizationName { get; set; } = string.Empty;

    public string OrganizationCode { get; set; } = string.Empty;

    public string AdminFullName { get; set; } = string.Empty;

    public string AdminEmail { get; set; } = string.Empty;

    public string AdminPassword { get; set; } = string.Empty;
}
