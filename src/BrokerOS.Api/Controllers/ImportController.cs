using BrokerOS.Application.Abstractions;
using BrokerOS.Application.Common;
using BrokerOS.Application.Import;
using BrokerOS.Application.Security;
using BrokerOS.Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BrokerOS.Api.Controllers;

/// <summary>
/// Bulk import for an existing Excel/CSV book of business.
/// Preview never writes; confirm inserts only valid rows. That two-step exists so a broker can
/// catch missing phones and duplicate policy numbers before 300 rows land in the database.
/// </summary>
[ApiController]
[Authorize(Policy = AuthPolicies.CanManageOperations)]
[Route("api/import")]
[RequestSizeLimit(10 * 1024 * 1024)]
[RequestFormLimits(MultipartBodyLengthLimit = 10 * 1024 * 1024)]
public sealed class ImportController : ControllerBase
{
    private readonly IImportService _importService;

    public ImportController(IImportService importService)
    {
        _importService = importService;
    }

    /// <summary>Downloads an Excel template with the expected client columns and one example row.</summary>
    /// <remarks>
    /// Auth: BrokerAdmin or BrokerManager (CanManageOperations).
    /// Tenant scope: template has no tenant data. Imported rows later use JWT OrganizationId.
    /// </remarks>
    [HttpGet("clients/template")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    public IActionResult GetClientTemplate()
    {
        var template = _importService.GetClientTemplate();
        return File(template.Content, template.ContentType, template.DownloadName);
    }

    /// <summary>Parses a client CSV/XLSX and returns per-row validation without saving.</summary>
    /// <remarks>
    /// Auth: BrokerAdmin or BrokerManager.
    /// Tenant scope: duplicate ClientCode is checked only inside the current JWT organization.
    /// An OrganizationId column in the file is ignored.
    /// </remarks>
    [HttpPost("clients/preview")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ApiResponse<ImportPreviewDto<ClientImportRowDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<ImportPreviewDto<ClientImportRowDto>>>> PreviewClients(
        [FromForm] IFormFile file,
        CancellationToken cancellationToken)
    {
        await using var buffer = await CopyUploadAsync(file, cancellationToken);
        var importFile = new ImportFileContent { Content = buffer, FileName = file.FileName };
        var result = await _importService.PreviewClientsAsync(importFile, cancellationToken);
        return Ok(ApiResponse<ImportPreviewDto<ClientImportRowDto>>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    /// <summary>Commits valid client rows from a previous preview token.</summary>
    /// <remarks>
    /// Auth: BrokerAdmin or BrokerManager.
    /// Tenant scope: session token is bound to JWT OrganizationId; another brokerage's token is treated as expired.
    /// Only valid rows are inserted. Invalid rows are listed under skipped.
    /// </remarks>
    [HttpPost("clients/confirm")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(ApiResponse<ImportCommitResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<ImportCommitResultDto>>> ConfirmClients(
        [FromBody] ImportConfirmRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _importService.ConfirmClientsAsync(request.PreviewToken, file: null, cancellationToken);
        return Ok(ApiResponse<ImportCommitResultDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    /// <summary>Commits valid client rows by re-uploading the file (same rules as preview + confirm).</summary>
    /// <remarks>
    /// Auth: BrokerAdmin or BrokerManager.
    /// Tenant scope: OrganizationId comes from the JWT, never from the file.
    /// Use this when the preview token expired; invalid rows are still skipped.
    /// </remarks>
    [HttpPost("clients/confirm")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ApiResponse<ImportCommitResultDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<ImportCommitResultDto>>> ConfirmClientsFromFile(
        [FromForm] IFormFile file,
        CancellationToken cancellationToken)
    {
        await using var buffer = await CopyUploadAsync(file, cancellationToken);
        var importFile = new ImportFileContent { Content = buffer, FileName = file.FileName };
        var result = await _importService.ConfirmClientsAsync(previewToken: null, importFile, cancellationToken);
        return Ok(ApiResponse<ImportCommitResultDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    /// <summary>Downloads an Excel template with the expected policy columns and one example row.</summary>
    /// <remarks>
    /// Auth: BrokerAdmin or BrokerManager.
    /// Tenant scope: template has no tenant data.
    /// </remarks>
    [HttpGet("policies/template")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    public IActionResult GetPolicyTemplate()
    {
        var template = _importService.GetPolicyTemplate();
        return File(template.Content, template.ContentType, template.DownloadName);
    }

    /// <summary>Parses a policy CSV/XLSX, matches each row to an existing client, and returns validation without saving.</summary>
    /// <remarks>
    /// Auth: BrokerAdmin or BrokerManager.
    /// Tenant scope: clients and insurers are loaded through EF query filters for the JWT org (plus global insurers).
    /// Match strategy: ClientCode (default) or NameAndPhone — pick on the query string, not a column in the file.
    /// </remarks>
    [HttpPost("policies/preview")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ApiResponse<ImportPreviewDto<PolicyImportRowDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<ImportPreviewDto<PolicyImportRowDto>>>> PreviewPolicies(
        [FromForm] IFormFile file,
        [FromQuery] ClientMatchStrategy matchBy = ClientMatchStrategy.ClientCode,
        CancellationToken cancellationToken = default)
    {
        await using var buffer = await CopyUploadAsync(file, cancellationToken);
        var importFile = new ImportFileContent { Content = buffer, FileName = file.FileName };
        var result = await _importService.PreviewPoliciesAsync(importFile, matchBy, cancellationToken);
        return Ok(ApiResponse<ImportPreviewDto<PolicyImportRowDto>>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    /// <summary>Commits valid policy rows from a previous preview token.</summary>
    /// <remarks>
    /// Auth: BrokerAdmin or BrokerManager.
    /// Tenant scope: token is bound to JWT OrganizationId. Client/insurer ids are re-checked at commit.
    /// </remarks>
    [HttpPost("policies/confirm")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(ApiResponse<ImportCommitResultDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<ImportCommitResultDto>>> ConfirmPolicies(
        [FromBody] ImportConfirmRequest request,
        [FromQuery] ClientMatchStrategy matchBy = ClientMatchStrategy.ClientCode,
        CancellationToken cancellationToken = default)
    {
        var result = await _importService.ConfirmPoliciesAsync(request.PreviewToken, file: null, matchBy, cancellationToken);
        return Ok(ApiResponse<ImportCommitResultDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    /// <summary>Commits valid policy rows by re-uploading the file.</summary>
    /// <remarks>
    /// Auth: BrokerAdmin or BrokerManager.
    /// Tenant scope: OrganizationId from JWT. Match strategy from the query string.
    /// </remarks>
    [HttpPost("policies/confirm")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ApiResponse<ImportCommitResultDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<ImportCommitResultDto>>> ConfirmPoliciesFromFile(
        [FromForm] IFormFile file,
        [FromQuery] ClientMatchStrategy matchBy = ClientMatchStrategy.ClientCode,
        CancellationToken cancellationToken = default)
    {
        await using var buffer = await CopyUploadAsync(file, cancellationToken);
        var importFile = new ImportFileContent { Content = buffer, FileName = file.FileName };
        var result = await _importService.ConfirmPoliciesAsync(previewToken: null, importFile, matchBy, cancellationToken);
        return Ok(ApiResponse<ImportCommitResultDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    private static async Task<MemoryStream> CopyUploadAsync(IFormFile? file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
        {
            throw new BusinessRuleException("Choose a CSV or Excel file.");
        }

        if (file.Length > 10 * 1024 * 1024)
        {
            throw new BusinessRuleException("The file is larger than 10 MB.");
        }

        var buffer = new MemoryStream();
        await file.CopyToAsync(buffer, cancellationToken);
        buffer.Position = 0;
        return buffer;
    }
}
