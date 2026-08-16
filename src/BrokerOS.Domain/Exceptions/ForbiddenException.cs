namespace BrokerOS.Domain.Exceptions;

/// <summary>
/// Mapped to HTTP 403. Use when the caller is authenticated in the right tenant but the action
/// is not allowed (e.g. mutating a system insurer). Do not use this for "row not in your book" — that is 404.
/// </summary>
public sealed class ForbiddenException : Exception
{
    public ForbiddenException(string message)
        : base(message)
    {
    }
}
