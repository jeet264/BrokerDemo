namespace BrokerOS.Application.Insurers;

public interface IInsurerWriteRequest
{
    string Name { get; }

    string Code { get; }

    string? Email { get; }

    string? Phone { get; }

    string? Website { get; }
}
