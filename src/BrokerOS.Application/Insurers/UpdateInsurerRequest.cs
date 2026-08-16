namespace BrokerOS.Application.Insurers;

public sealed class UpdateInsurerRequest : IInsurerWriteRequest
{
    public string Name { get; set; } = string.Empty;

    public string Code { get; set; } = string.Empty;

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public string? Website { get; set; }

    public bool IsActive { get; set; } = true;
}
