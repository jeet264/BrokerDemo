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
                $"Internal reminder: {clientName} policy {policyNumber} with {insurerName} expires on {expiry} (90 days).\n\nOpen the renewal file and plan the first client conversation so there is no last-minute scramble."),
            60 => InternalEmail(
                clientId,
                $"Review renewal — {policyNumber} (60 days)",
                $"Internal reminder: {clientName} policy {policyNumber} with {insurerName} expires on {expiry} (60 days).\n\nReview the current cover, note any claims or sum-insured changes, and schedule a call with the client."),
            45 => InternalEmail(
                clientId,
                $"Prepare quotation request — {policyNumber} (45 days)",
                $"Internal reminder: {clientName} policy {policyNumber} with {insurerName} expires on {expiry} (45 days).\n\nPrepare the quotation request pack so we can go to market with enough time for the client to decide."),
            30 => new SimulatedNotificationDraft(
                NotificationRecipientType.Client,
                NotificationChannel.Email,
                "Your policy is due for renewal — action needed",
                $"Dear {clientName},\n\nThis is a courtesy reminder that your {insurerName} policy {policyNumber} is due for renewal on {expiry}.\n\nPlease contact us to review your cover and complete renewal so there is no gap in protection.\n\nWarm regards,\n{assignedName}\n{orgName}",
                clientId),
            15 => new SimulatedNotificationDraft(
                NotificationRecipientType.Insurer,
                NotificationChannel.Email,
                $"Quotation required — {policyNumber} renews {expiry}",
                $"Dear {insurerName},\n\nWe request a renewal quotation for policy {policyNumber} ({clientName}). The current term expires on {expiry}.\n\nPlease share terms and premium at the earliest so we can present options to the client.\n\nRegards,\n{assignedName}\n{orgName}",
                clientId),
            7 => new SimulatedNotificationDraft(
                NotificationRecipientType.Client,
                NotificationChannel.WhatsApp,
                $"Reminder: {policyNumber} renews in 7 days",
                $"Hi {clientName}, this is {orgName}. Your policy {policyNumber} with {insurerName} expires on {expiry} — 7 days left. Please share any changes in cover so we can close renewal on time. Reply here or call {assignedName}.",
                clientId),
            1 => new SimulatedNotificationDraft(
                NotificationRecipientType.Client,
                NotificationChannel.SMS,
                $"Urgent: {policyNumber} expires tomorrow",
                $"{orgName}: Policy {policyNumber} for {clientName} expires tomorrow ({expiry}). Contact {assignedName} today to avoid a break in cover.",
                clientId),
            _ => new SimulatedNotificationDraft(
                NotificationRecipientType.Client,
                NotificationChannel.Email,
                $"Renewal reminder — {policyNumber}",
                $"Dear {clientName},\n\nYour {insurerName} policy {policyNumber} is due for renewal on {expiry}. Please contact {assignedName} at {orgName} to complete renewal.\n\nWarm regards,\n{orgName}",
                clientId)
        };
    }

    private static SimulatedNotificationDraft InternalEmail(long? clientId, string subject, string body) =>
        new(NotificationRecipientType.InternalUser, NotificationChannel.Email, subject, body, clientId);
}
