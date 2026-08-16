namespace BrokerOS.Application.MyDay;

/// <summary>Body for Call / Mark Done / Send Follow-up from a My Day card.</summary>
public sealed class MyDayActionRequest
{
    public MyDayItemKind Kind { get; set; }

    public Guid PublicId { get; set; }
}
