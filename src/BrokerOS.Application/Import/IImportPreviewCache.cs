namespace BrokerOS.Application.Import;

/// <summary>Short-lived store for preview rows. Implementations must not share tokens across organizations.</summary>
public interface IImportPreviewCache
{
    void Set(ImportPreviewSession session);

    ImportPreviewSession? Get(Guid token);

    void Remove(Guid token);
}
