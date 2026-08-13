namespace BrokerOS.Domain.Renewals;

public static class RenewalCalendar
{
    public static int DaysRemaining(DateOnly expiryOrRenewalDate, DateOnly today) =>
        expiryOrRenewalDate.DayNumber - today.DayNumber;
}
