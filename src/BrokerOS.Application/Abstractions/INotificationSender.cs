using BrokerOS.Domain.Entities;

namespace BrokerOS.Application.Abstractions;

/// <summary>
/// Outbound delivery for one <see cref="Notification"/>. The renewal worker and any future
/// "send now" actions call this — they do not talk to Twilio / Gupshup / Interakt directly.
/// </summary>
/// <remarks>
/// Swap the DI registration of <c>INotificationSender</c> for a real provider implementation
/// (e.g. <c>WhatsAppBusinessApiSender</c>) when ready to go live — no other code should need to change.
/// A live sender should call the provider, then set <c>Status</c> to Sent (or Failed) on the same entity
/// before it is persisted. This demo registers <c>SimulatedNotificationSender</c>, which only records
/// the row and never hits a network.
/// </remarks>
public interface INotificationSender
{
    Task SendAsync(Notification notification, CancellationToken cancellationToken);
}
