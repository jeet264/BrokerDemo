namespace BrokerOS.Domain.Exceptions;

/// <summary>
/// Mapped to HTTP 404. Used for missing rows and for out-of-scope access (other tenant or
/// employee viewing someone else's assignment) so we do not confirm that the id exists.
/// </summary>
public sealed class NotFoundException : Exception
{
    public NotFoundException(string message)
        : base(message)
    {
    }
}
