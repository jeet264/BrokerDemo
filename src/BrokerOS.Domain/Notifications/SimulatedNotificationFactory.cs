using System.Globalization;
using BrokerOS.Domain.Entities;
using BrokerOS.Domain.Enums;

namespace BrokerOS.Domain.Notifications;

public sealed record SimulatedNotificationDraft(
    NotificationRecipientType RecipientType,
    NotificationChannel Channel,
    string Subject,
    string Body,
    long? ClientId);

/// <summary>
/// Milestone copy for the renewal reminder worker. Client-facing lines are WhatsApp:
/// short, informal-but-professional, as an Indian broker would type on the phone.
/// Email is kept for internal desk notes and insurer quotation requests, where WhatsApp does not fit.
/// </summary>
public static class SimulatedNotificationFactory
{
    public static SimulatedNotificationDraft CreateForMilestone(Renewal renewal, int milestoneDays)
    {
        var policy = renewal.Policy;
        var client = policy.Client;
        var insurer = policy.Insurer;
        var assigned = renewal.AssignedUser ?? policy.AssignedUser;
        var orgName = string.IsNullOrWhiteSpace(renewal.Organization?.Name)
            ? "your broker"
            : renewal.Organization.Name;
        var clientName = string.IsNullOrWhiteSpace(client?.CompanyName)
            ? "Client"
            : client.CompanyName;
        var insurerName = string.IsNullOrWhiteSpace(insurer?.Name)
            ? "the insurer"
            : insurer.Name;
        var assignedName = string.IsNullOrWhiteSpace(assigned?.FullName)
            ? orgName
            : assigned.FullName;
        var policyNumber = policy.PolicyNumber;
        var expiry = policy.ExpiryDate.ToString("dd MMM yyyy", CultureInfo.InvariantCulture);
        var clientId = client?.Id ?? policy.ClientId;

        return milestoneDays switch
        {
            90 => InternalEmail(
                clientId,
                $"Start renewal planning — {policyNumber} (90 days)",
                $"Internal: {clientName} / {policyNumber} ({insurerName}) expires {expiry} — 90 days.\n\nOpen the renewal file and plan the first client WhatsApp so this does not become a last-week scramble."),
            60 => InternalEmail(
                clientId,
                $"Review renewal — {policyNumber} (60 days)",
                $"Internal: {clientName} / {policyNumber} ({insurerName}) expires {expiry} — 60 days.\n\nReview cover, claims, and sum insured, then message the client on WhatsApp to schedule a call."),
            45 => InternalEmail(
                clientId,
                $"Prepare quotation request — {policyNumber} (45 days)",
                $"Internal: {clientName} / {policyNumber} ({insurerName}) expires {expiry} — 45 days.\n\nPrepare the quotation pack so we can go to market with time for the client to decide."),
            30 => ClientWhatsApp(
                clientId,
                $"Policy {policyNumber} is due for renewal",
                $"Hi {clientName}, {orgName} here. Your {insurerName} policy {policyNumber} comes up for renewal on {expiry}. Shall we review the cover this week so there is no gap? Reply here or call {assignedName}."),
            15 => InsurerEmail(
                clientId,
                $"Quotation required — {policyNumber} renews {expiry}",
                $"Dear {insurerName},\n\nPlease share a renewal quotation for policy {policyNumber} ({clientName}). Current term expires {expiry}.\n\nWe need terms and premium in time to present options to the client.\n\nRegards,\n{assignedName}\n{orgName}"),
            7 => ClientWhatsApp(
                clientId,
                $"7 days left on {policyNumber}",
                $"Hi {clientName}, reminder from {orgName} — {policyNumber} with {insurerName} expires on {expiry} (7 days). Any change in cover? Reply here and we will close it. — {assignedName}"),
            1 => ClientWhatsApp(
                clientId,
                $"{policyNumber} expires tomorrow",
                $"Hi {clientName}, {orgName}: {policyNumber} expires tomorrow ({expiry}). Please confirm today so cover does not lapse. {assignedName} is on this."),
            _ => ClientWhatsApp(
                clientId,
                $"Renewal reminder — {policyNumber}",
                $"Hi {clientName}, {orgName} here. Your {insurerName} policy {policyNumber} is due for renewal on {expiry}. Reply here or call {assignedName} to complete it.")
        };
    }

    /// <summary>Client-facing default: WhatsApp, not email or SMS.</summary>
    private static SimulatedNotificationDraft ClientWhatsApp(long? clientId, string subject, string body) =>
        new(NotificationRecipientType.Client, NotificationChannel.WhatsApp, subject, body, clientId);

    /// <summary>Insurer quotation chase stays on email — a company inbox, not a personal WhatsApp.</summary>
    private static SimulatedNotificationDraft InsurerEmail(long? clientId, string subject, string body) =>
        new(NotificationRecipientType.Insurer, NotificationChannel.Email, subject, body, clientId);

    /// <summary>Internal desk reminders stay on email so they land in the broker's work inbox.</summary>
    private static SimulatedNotificationDraft InternalEmail(long? clientId, string subject, string body) =>
        new(NotificationRecipientType.InternalUser, NotificationChannel.Email, subject, body, clientId);
}
