using BrokerOS.Domain.Entities;
using BrokerOS.Domain.Enums;
using BrokerOS.Domain.Notifications;

namespace BrokerOS.Domain.Tests;

public sealed class SimulatedNotificationFactoryTests
{
    [Theory]
    [InlineData(30)]
    [InlineData(7)]
    [InlineData(1)]
    public void Client_milestones_use_whatsapp(int milestoneDays)
    {
        var draft = SimulatedNotificationFactory.CreateForMilestone(RenewalFixture(), milestoneDays);

        Assert.Equal(NotificationRecipientType.Client, draft.RecipientType);
        Assert.Equal(NotificationChannel.WhatsApp, draft.Channel);
        Assert.DoesNotContain("Dear ", draft.Body);
        Assert.Contains("Hi ", draft.Body);
    }

    [Theory]
    [InlineData(90)]
    [InlineData(60)]
    [InlineData(45)]
    public void Internal_milestones_stay_on_email(int milestoneDays)
    {
        var draft = SimulatedNotificationFactory.CreateForMilestone(RenewalFixture(), milestoneDays);

        Assert.Equal(NotificationRecipientType.InternalUser, draft.RecipientType);
        Assert.Equal(NotificationChannel.Email, draft.Channel);
    }

    [Fact]
    public void Insurer_quotation_request_stays_on_email()
    {
        var draft = SimulatedNotificationFactory.CreateForMilestone(RenewalFixture(), 15);

        Assert.Equal(NotificationRecipientType.Insurer, draft.RecipientType);
        Assert.Equal(NotificationChannel.Email, draft.Channel);
        Assert.Contains("quotation", draft.Body, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Fallback_milestone_is_client_whatsapp()
    {
        var draft = SimulatedNotificationFactory.CreateForMilestone(RenewalFixture(), 3);

        Assert.Equal(NotificationRecipientType.Client, draft.RecipientType);
        Assert.Equal(NotificationChannel.WhatsApp, draft.Channel);
        Assert.Contains("Hi ", draft.Body);
    }

    [Fact]
    public void Notification_channel_defaults_to_whatsapp()
    {
        Assert.Equal(NotificationChannel.WhatsApp, new Notification().Channel);
    }

    private static Renewal RenewalFixture() =>
        new()
        {
            Organization = new Organization { Name = "Apex Insurance Brokers" },
            AssignedUser = new User { FullName = "Apex Employee" },
            Policy = new Policy
            {
                PolicyNumber = "POL-A100",
                ExpiryDate = new DateOnly(2026, 9, 12),
                ClientId = 33,
                Client = new Client { Id = 33, CompanyName = "Alpha Logistics" },
                Insurer = new Insurer { Name = "Test New India" }
            }
        };
}
