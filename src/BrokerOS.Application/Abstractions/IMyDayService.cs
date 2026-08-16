using BrokerOS.Application.MyDay;

namespace BrokerOS.Application.Abstractions;

/// <summary>
/// Builds the morning briefing and records the inline actions taken from it.
/// Tenant + assignment scope apply: employees only see work assigned to them.
/// </summary>
public interface IMyDayService
{
    /// <summary>Capped overdue / due-today / upcoming lists for the signed-in user (IST "today").</summary>
    Task<MyDayDto> GetAsync(CancellationToken cancellationToken);

    /// <summary>Completes a task, or clears a renewal's next chase without rolling over the policy term.</summary>
    Task CompleteAsync(MyDayActionRequest request, CancellationToken cancellationToken);

    /// <summary>Writes a Call activity (and stamps LastFollowUpAtUtc on renewals).</summary>
    Task LogCallAsync(MyDayActionRequest request, CancellationToken cancellationToken);

    /// <summary>Writes a WhatsApp activity and pushes the next chase two IST calendar days.</summary>
    Task SendFollowUpAsync(MyDayActionRequest request, CancellationToken cancellationToken);
}
