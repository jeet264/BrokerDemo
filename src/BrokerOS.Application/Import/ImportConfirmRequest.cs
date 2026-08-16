namespace BrokerOS.Application.Import;

/// <summary>
/// Confirms a previous preview. Prefer this over re-uploading the file so the broker imports
/// exactly the rows they just reviewed. Re-upload is still accepted on the multipart confirm action.
/// </summary>
public sealed class ImportConfirmRequest
{
    public Guid PreviewToken { get; set; }
}
