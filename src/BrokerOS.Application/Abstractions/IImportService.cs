using BrokerOS.Application.Import;

namespace BrokerOS.Application.Abstractions;

/// <summary>
/// Bulk-loads the existing Excel/CSV book of business. Preview validates without writing;
/// confirm inserts only valid rows for the current JWT OrganizationId.
/// </summary>
public interface IImportService
{
    Task<ImportPreviewDto<ClientImportRowDto>> PreviewClientsAsync(
        ImportFileContent file,
        CancellationToken cancellationToken);

    Task<ImportCommitResultDto> ConfirmClientsAsync(
        Guid? previewToken,
        ImportFileContent? file,
        CancellationToken cancellationToken);

    Task<ImportPreviewDto<PolicyImportRowDto>> PreviewPoliciesAsync(
        ImportFileContent file,
        ClientMatchStrategy matchBy,
        CancellationToken cancellationToken);

    Task<ImportCommitResultDto> ConfirmPoliciesAsync(
        Guid? previewToken,
        ImportFileContent? file,
        ClientMatchStrategy matchBy,
        CancellationToken cancellationToken);

    ImportTemplateFile GetClientTemplate();

    ImportTemplateFile GetPolicyTemplate();
}
