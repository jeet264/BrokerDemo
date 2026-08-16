using BrokerOS.Application.Abstractions;
using BrokerOS.Application.Import;
using BrokerOS.Domain.Entities;
using BrokerOS.Domain.Enums;
using BrokerOS.Domain.Exceptions;
using BrokerOS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BrokerOS.Infrastructure.Import;

/// <summary>
/// Two-step bulk import for the existing Excel book of business.
/// Preview parses and validates without writing so a broker can catch bad phones, dates, and
/// duplicate policy numbers before anything hits the database. Confirm inserts only valid rows
/// and always stamps OrganizationId from the JWT — a column in the file cannot hop tenants.
/// </summary>
public sealed class ImportService : IImportService
{
    private const int MaxUploadBytes = 10 * 1024 * 1024;
    private const string MissingAddress = "Not provided";

    private readonly BrokerOsDbContext _dbContext;
    private readonly ICurrentUserService _currentUser;
    private readonly IImportPreviewCache _previewCache;

    public ImportService(
        BrokerOsDbContext dbContext,
        ICurrentUserService currentUser,
        IImportPreviewCache previewCache)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
        _previewCache = previewCache;
    }

    public async Task<ImportPreviewDto<ClientImportRowDto>> PreviewClientsAsync(
        ImportFileContent file,
        CancellationToken cancellationToken)
    {
        var table = ReadTable(file);
        EnsureClientHeaders(table.Headers);

        var existingCodes = await LoadExistingClientCodesAsync(cancellationToken);
        var drafts = BuildClientDrafts(table.Rows, existingCodes);
        var token = StoreSession(ImportKind.Clients, matchStrategy: null, drafts, policies: []);
        return ToClientPreview(token, drafts);
    }

    public async Task<ImportCommitResultDto> ConfirmClientsAsync(
        Guid? previewToken,
        ImportFileContent? file,
        CancellationToken cancellationToken)
    {
        var drafts = previewToken.HasValue && previewToken.Value != Guid.Empty
            ? RequireSession(previewToken.Value, ImportKind.Clients).Clients.ToList()
            : file is null
                ? throw new BusinessRuleException("Upload a file or pass the preview token from the previous step.")
                : BuildClientDrafts(ReadTable(file).Rows, await LoadExistingClientCodesAsync(cancellationToken));

        // Re-check uniqueness at commit time: another import (or a manual create) may have taken a code since preview.
        var existingCodes = await LoadExistingClientCodesAsync(cancellationToken);
        var seenInBatch = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var toInsert = new List<Client>();
        var skipped = new List<ImportSkipDto>();

        foreach (var draft in drafts)
        {
            if (!draft.IsValid)
            {
                skipped.Add(new ImportSkipDto { RowNumber = draft.RowNumber, Reason = draft.Error ?? "Invalid row." });
                continue;
            }

            if (existingCodes.Contains(draft.ClientCode) || !seenInBatch.Add(draft.ClientCode))
            {
                skipped.Add(new ImportSkipDto { RowNumber = draft.RowNumber, Reason = "Duplicate client code." });
                continue;
            }

            toInsert.Add(MapClient(draft));
        }

        if (toInsert.Count > 0)
        {
            _dbContext.Clients.AddRange(toInsert);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        if (previewToken.HasValue && previewToken.Value != Guid.Empty)
        {
            _previewCache.Remove(previewToken.Value);
        }

        return new ImportCommitResultDto
        {
            ImportedCount = toInsert.Count,
            SkippedCount = skipped.Count,
            Skipped = skipped
        };
    }

    public async Task<ImportPreviewDto<PolicyImportRowDto>> PreviewPoliciesAsync(
        ImportFileContent file,
        ClientMatchStrategy matchBy,
        CancellationToken cancellationToken)
    {
        var table = ReadTable(file);
        EnsurePolicyHeaders(table.Headers, matchBy);

        var context = await LoadPolicyMatchContextAsync(cancellationToken);
        var drafts = BuildPolicyDrafts(table.Rows, matchBy, context);
        var token = StoreSession(ImportKind.Policies, matchBy, clients: [], drafts);
        return ToPolicyPreview(token, matchBy, drafts);
    }

    public async Task<ImportCommitResultDto> ConfirmPoliciesAsync(
        Guid? previewToken,
        ImportFileContent? file,
        ClientMatchStrategy matchBy,
        CancellationToken cancellationToken)
    {
        List<PolicyImportDraft> drafts;
        if (previewToken.HasValue && previewToken.Value != Guid.Empty)
        {
            var session = RequireSession(previewToken.Value, ImportKind.Policies);
            drafts = session.Policies.ToList();
            matchBy = session.MatchStrategy ?? matchBy;
        }
        else if (file is not null)
        {
            var contextForFile = await LoadPolicyMatchContextAsync(cancellationToken);
            drafts = BuildPolicyDrafts(ReadTable(file).Rows, matchBy, contextForFile);
        }
        else
        {
            throw new BusinessRuleException("Upload a file or pass the preview token from the previous step.");
        }

        var context = await LoadPolicyMatchContextAsync(cancellationToken);
        var seenNumbers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var toInsert = new List<Policy>();
        var skipped = new List<ImportSkipDto>();

        foreach (var draft in drafts)
        {
            if (!draft.IsValid)
            {
                skipped.Add(new ImportSkipDto { RowNumber = draft.RowNumber, Reason = draft.Error ?? "Invalid row." });
                continue;
            }

            if (context.PolicyNumbers.Contains(draft.PolicyNumber) || !seenNumbers.Add(draft.PolicyNumber))
            {
                skipped.Add(new ImportSkipDto { RowNumber = draft.RowNumber, Reason = "Duplicate policy number." });
                continue;
            }

            if (draft.ClientId is null || !context.ClientIds.Contains(draft.ClientId.Value))
            {
                skipped.Add(new ImportSkipDto { RowNumber = draft.RowNumber, Reason = "No matching client found." });
                continue;
            }

            if (draft.InsurerId is null || !context.InsurerIds.Contains(draft.InsurerId.Value))
            {
                skipped.Add(new ImportSkipDto { RowNumber = draft.RowNumber, Reason = "Insurer was not found." });
                continue;
            }

            toInsert.Add(MapPolicy(draft));
        }

        if (toInsert.Count > 0)
        {
            _dbContext.Policies.AddRange(toInsert);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        if (previewToken.HasValue && previewToken.Value != Guid.Empty)
        {
            _previewCache.Remove(previewToken.Value);
        }

        return new ImportCommitResultDto
        {
            ImportedCount = toInsert.Count,
            SkippedCount = skipped.Count,
            Skipped = skipped
        };
    }

    public ImportTemplateFile GetClientTemplate() => ImportTemplateFactory.CreateClientTemplate();

    public ImportTemplateFile GetPolicyTemplate() => ImportTemplateFactory.CreatePolicyTemplate();

    private SpreadsheetTable ReadTable(ImportFileContent file)
    {
        if (file.Content.CanSeek)
        {
            file.Content.Position = 0;
        }

        if (file.Content.CanSeek && file.Content.Length > MaxUploadBytes)
        {
            throw new BusinessRuleException("The file is larger than 10 MB.");
        }

        return SpreadsheetReader.Read(file.Content, file.FileName);
    }

    private static void EnsureClientHeaders(IReadOnlyList<string> headers)
    {
        if (!SpreadsheetReader.HasHeader(headers, "clientcode", "clientexternalid", "externalid"))
        {
            throw new BusinessRuleException("The file is missing a ClientCode column (ClientExternalId is also accepted).");
        }

        if (!SpreadsheetReader.HasHeader(headers, "companyname", "clientname"))
        {
            throw new BusinessRuleException("The file is missing a CompanyName column.");
        }

        if (!SpreadsheetReader.HasHeader(headers, "phone", "mobile", "mobilenumber"))
        {
            throw new BusinessRuleException("The file is missing a Phone column.");
        }
    }

    private static void EnsurePolicyHeaders(IReadOnlyList<string> headers, ClientMatchStrategy matchBy)
    {
        if (!SpreadsheetReader.HasHeader(headers, "policynumber"))
        {
            throw new BusinessRuleException("The file is missing a PolicyNumber column.");
        }

        if (!SpreadsheetReader.HasHeader(headers, "policytype"))
        {
            throw new BusinessRuleException("The file is missing a PolicyType column.");
        }

        if (!SpreadsheetReader.HasHeader(headers, "startdate"))
        {
            throw new BusinessRuleException("The file is missing a StartDate column.");
        }

        if (!SpreadsheetReader.HasHeader(headers, "expirydate", "enddate"))
        {
            throw new BusinessRuleException("The file is missing an ExpiryDate column.");
        }

        if (!SpreadsheetReader.HasHeader(headers, "premium"))
        {
            throw new BusinessRuleException("The file is missing a Premium column.");
        }

        if (!SpreadsheetReader.HasHeader(headers, "insurercode", "insurername", "insurer"))
        {
            throw new BusinessRuleException("The file is missing an InsurerCode or InsurerName column.");
        }

        if (matchBy == ClientMatchStrategy.ClientCode
            && !SpreadsheetReader.HasHeader(headers, "clientcode", "clientexternalid", "externalid"))
        {
            throw new BusinessRuleException("Match by client code requires a ClientCode or ClientExternalId column.");
        }

        if (matchBy == ClientMatchStrategy.NameAndPhone)
        {
            if (!SpreadsheetReader.HasHeader(headers, "clientname", "companyname"))
            {
                throw new BusinessRuleException("Match by name and phone requires a ClientName (or CompanyName) column.");
            }

            if (!SpreadsheetReader.HasHeader(headers, "phone", "mobile", "mobilenumber"))
            {
                throw new BusinessRuleException("Match by name and phone requires a Phone column.");
            }
        }
    }

    private List<ClientImportDraft> BuildClientDrafts(
        IReadOnlyList<IReadOnlyDictionary<string, string>> rows,
        HashSet<string> existingCodes)
    {
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var drafts = new List<ClientImportDraft>(rows.Count);

        for (var index = 0; index < rows.Count; index++)
        {
            var row = rows[index];
            var rowNumber = index + 2;
            var clientCode = SpreadsheetReader.Get(row, "clientcode", "clientexternalid", "externalid");
            var companyName = SpreadsheetReader.Get(row, "companyname", "clientname");
            var phone = SpreadsheetReader.Get(row, "phone", "mobile", "mobilenumber");
            var email = SpreadsheetReader.Get(row, "email");
            var typeRaw = SpreadsheetReader.Get(row, "clienttype", "type");
            var error = (string?)null;

            if (string.IsNullOrWhiteSpace(clientCode))
            {
                error = "Missing client code.";
            }
            else if (string.IsNullOrWhiteSpace(companyName))
            {
                error = "Missing company name.";
            }
            else if (string.IsNullOrWhiteSpace(phone))
            {
                error = "Missing phone number.";
            }
            else if (!ImportValueParser.TryParseClientType(typeRaw, out _))
            {
                error = "Client type must be Corporate, SME, or Individual.";
            }
            else if (!string.IsNullOrWhiteSpace(email) && !email.Contains('@', StringComparison.Ordinal))
            {
                error = "Email is not valid.";
            }
            else if (existingCodes.Contains(clientCode) || !seen.Add(clientCode))
            {
                error = "Duplicate client code.";
            }

            ImportValueParser.TryParseClientType(typeRaw, out var clientType);

            drafts.Add(new ClientImportDraft
            {
                RowNumber = rowNumber,
                IsValid = error is null,
                Error = error,
                Values = new ClientImportRowDto
                {
                    ClientCode = clientCode,
                    CompanyName = companyName,
                    ClientType = string.IsNullOrWhiteSpace(typeRaw) ? ClientType.Corporate.ToString() : typeRaw,
                    Email = email,
                    Phone = phone,
                    City = SpreadsheetReader.Get(row, "city"),
                    State = SpreadsheetReader.Get(row, "state")
                },
                ClientCode = clientCode,
                CompanyName = companyName,
                ClientType = clientType,
                Industry = NullIfEmpty(SpreadsheetReader.Get(row, "industry")),
                Email = email.ToLowerInvariant(),
                Phone = phone,
                AlternatePhone = NullIfEmpty(SpreadsheetReader.Get(row, "alternatephone", "altphone")),
                AddressLine1 = DefaultIfEmpty(SpreadsheetReader.Get(row, "addressline1", "address"), MissingAddress),
                AddressLine2 = NullIfEmpty(SpreadsheetReader.Get(row, "addressline2")),
                City = DefaultIfEmpty(SpreadsheetReader.Get(row, "city"), MissingAddress),
                State = DefaultIfEmpty(SpreadsheetReader.Get(row, "state"), MissingAddress),
                PostalCode = DefaultIfEmpty(SpreadsheetReader.Get(row, "postalcode", "pincode", "zip"), "000000"),
                Country = DefaultIfEmpty(SpreadsheetReader.Get(row, "country"), "India"),
                Notes = NullIfEmpty(SpreadsheetReader.Get(row, "notes"))
            });
        }

        return drafts;
    }

    private List<PolicyImportDraft> BuildPolicyDrafts(
        IReadOnlyList<IReadOnlyDictionary<string, string>> rows,
        ClientMatchStrategy matchBy,
        PolicyMatchContext context)
    {
        var seenNumbers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var drafts = new List<PolicyImportDraft>(rows.Count);

        for (var index = 0; index < rows.Count; index++)
        {
            var row = rows[index];
            var rowNumber = index + 2;
            var policyNumber = SpreadsheetReader.Get(row, "policynumber");
            var clientCode = SpreadsheetReader.Get(row, "clientcode", "clientexternalid", "externalid");
            var clientName = SpreadsheetReader.Get(row, "clientname", "companyname");
            var phone = SpreadsheetReader.Get(row, "phone", "mobile", "mobilenumber");
            var insurerRaw = SpreadsheetReader.Get(row, "insurercode", "insurername", "insurer");
            var typeRaw = SpreadsheetReader.Get(row, "policytype");
            var startRaw = SpreadsheetReader.Get(row, "startdate");
            var expiryRaw = SpreadsheetReader.Get(row, "expirydate", "enddate");
            var premiumRaw = SpreadsheetReader.Get(row, "premium");
            var sumRaw = SpreadsheetReader.Get(row, "suminsured", "suminsuredamount");
            var commissionPctRaw = SpreadsheetReader.Get(row, "commissionpercentage", "commissionpct", "commissionpercent");
            var commissionAmtRaw = SpreadsheetReader.Get(row, "commissionamount");
            var statusRaw = SpreadsheetReader.Get(row, "status", "policystatus");

            string? error = null;
            long? clientId = null;
            string? matchedClientName = null;
            long? insurerId = null;
            var start = default(DateOnly);
            var expiry = default(DateOnly);
            var premium = 0m;
            var sumInsured = 0m;
            var commissionPct = 0m;
            var commissionAmt = 0m;
            var policyType = PolicyType.Other;
            var status = PolicyStatus.Active;

            if (string.IsNullOrWhiteSpace(policyNumber))
            {
                error = "Missing policy number.";
            }
            else if (context.PolicyNumbers.Contains(policyNumber) || !seenNumbers.Add(policyNumber))
            {
                error = "Duplicate policy number.";
            }
            else if (!ImportValueParser.TryParsePolicyType(typeRaw, out policyType))
            {
                error = "Policy type is missing or not recognised.";
            }
            else if (!ImportValueParser.TryParseDate(startRaw, out start))
            {
                error = "Start date is missing or not parseable.";
            }
            else if (!ImportValueParser.TryParseDate(expiryRaw, out expiry))
            {
                error = "Expiry date is missing or not parseable.";
            }
            else if (expiry < start)
            {
                error = "Expiry date is before start date.";
            }
            else if (!ImportValueParser.TryParseMoney(premiumRaw, out premium) || premium < 0)
            {
                error = "Premium is missing or not a valid number.";
            }
            else if (!string.IsNullOrWhiteSpace(sumRaw) && (!ImportValueParser.TryParseMoney(sumRaw, out sumInsured) || sumInsured < 0))
            {
                error = "Sum insured is not a valid number.";
            }
            else if (!string.IsNullOrWhiteSpace(commissionPctRaw)
                     && (!ImportValueParser.TryParseMoney(commissionPctRaw, out commissionPct) || commissionPct < 0))
            {
                error = "Commission percentage is not a valid number.";
            }
            else if (!string.IsNullOrWhiteSpace(commissionAmtRaw)
                     && (!ImportValueParser.TryParseMoney(commissionAmtRaw, out commissionAmt) || commissionAmt < 0))
            {
                error = "Commission amount is not a valid number.";
            }
            else if (!ImportValueParser.TryParsePolicyStatus(statusRaw, out status))
            {
                error = "Status must be Active, Expired, Cancelled, or PendingRenewal.";
            }
            else if (!TryMatchInsurer(insurerRaw, context, out insurerId))
            {
                error = "Insurer was not found.";
            }
            else if (!TryMatchClient(matchBy, clientCode, clientName, phone, context, out clientId, out matchedClientName, out var matchError))
            {
                error = matchError;
            }

            if (string.IsNullOrWhiteSpace(commissionAmtRaw) && commissionPct > 0 && premium > 0)
            {
                commissionAmt = decimal.Round(premium * commissionPct / 100m, 2, MidpointRounding.AwayFromZero);
            }

            drafts.Add(new PolicyImportDraft
            {
                RowNumber = rowNumber,
                IsValid = error is null,
                Error = error,
                Values = new PolicyImportRowDto
                {
                    PolicyNumber = policyNumber,
                    ClientCode = clientCode,
                    ClientName = clientName,
                    Phone = phone,
                    Insurer = insurerRaw,
                    PolicyType = typeRaw,
                    StartDate = start == default ? startRaw : start.ToString("yyyy-MM-dd"),
                    ExpiryDate = expiry == default ? expiryRaw : expiry.ToString("yyyy-MM-dd"),
                    Premium = premiumRaw,
                    MatchedClientName = matchedClientName
                },
                PolicyNumber = policyNumber,
                ClientId = clientId,
                InsurerId = insurerId,
                PolicyType = policyType,
                StartDate = start,
                ExpiryDate = expiry,
                Premium = premium,
                SumInsured = sumInsured,
                CommissionPercentage = commissionPct,
                CommissionAmount = commissionAmt,
                Status = status,
                Notes = NullIfEmpty(SpreadsheetReader.Get(row, "notes"))
            });
        }

        return drafts;
    }

    private static bool TryMatchClient(
        ClientMatchStrategy matchBy,
        string clientCode,
        string clientName,
        string phone,
        PolicyMatchContext context,
        out long? clientId,
        out string? matchedName,
        out string error)
    {
        clientId = null;
        matchedName = null;
        error = "No matching client found.";

        if (matchBy == ClientMatchStrategy.ClientCode)
        {
            if (string.IsNullOrWhiteSpace(clientCode))
            {
                error = "Missing client code.";
                return false;
            }

            if (!context.ClientsByCode.TryGetValue(clientCode, out var client))
            {
                error = "No matching client found.";
                return false;
            }

            clientId = client.Id;
            matchedName = client.CompanyName;
            return true;
        }

        if (string.IsNullOrWhiteSpace(clientName) || string.IsNullOrWhiteSpace(phone))
        {
            error = "Missing client name or phone number.";
            return false;
        }

        var key = NamePhoneKey(clientName, phone);
        if (!context.ClientsByNamePhone.TryGetValue(key, out var matches) || matches.Count == 0)
        {
            error = "No matching client found.";
            return false;
        }

        if (matches.Count > 1)
        {
            error = "Multiple clients match this name and phone.";
            return false;
        }

        clientId = matches[0].Id;
        matchedName = matches[0].CompanyName;
        return true;
    }

    private static bool TryMatchInsurer(string raw, PolicyMatchContext context, out long? insurerId)
    {
        insurerId = null;
        if (string.IsNullOrWhiteSpace(raw))
        {
            return false;
        }

        if (context.InsurersByCode.TryGetValue(raw, out var byCode))
        {
            insurerId = byCode.Id;
            return true;
        }

        if (context.InsurersByName.TryGetValue(raw, out var byName) && byName.Count == 1)
        {
            insurerId = byName[0].Id;
            return true;
        }

        return false;
    }

    private Client MapClient(ClientImportDraft draft)
    {
        return new Client
        {
            OrganizationId = _currentUser.OrganizationId,
            ClientCode = draft.ClientCode,
            CompanyName = draft.CompanyName,
            ClientType = draft.ClientType,
            Industry = draft.Industry,
            Email = draft.Email,
            Phone = draft.Phone,
            AlternatePhone = draft.AlternatePhone,
            AddressLine1 = draft.AddressLine1,
            AddressLine2 = draft.AddressLine2,
            City = draft.City,
            State = draft.State,
            PostalCode = draft.PostalCode,
            Country = draft.Country,
            Notes = draft.Notes,
            IsActive = true
        };
    }

    private Policy MapPolicy(PolicyImportDraft draft)
    {
        return new Policy
        {
            OrganizationId = _currentUser.OrganizationId,
            ClientId = draft.ClientId!.Value,
            InsurerId = draft.InsurerId!.Value,
            PolicyNumber = draft.PolicyNumber,
            PolicyType = draft.PolicyType,
            StartDate = draft.StartDate,
            ExpiryDate = draft.ExpiryDate,
            Premium = draft.Premium,
            SumInsured = draft.SumInsured,
            CommissionPercentage = draft.CommissionPercentage,
            CommissionAmount = draft.CommissionAmount,
            Status = draft.Status,
            Notes = draft.Notes
        };
    }

    private Guid StoreSession(
        ImportKind kind,
        ClientMatchStrategy? matchStrategy,
        IReadOnlyList<ClientImportDraft> clients,
        IReadOnlyList<PolicyImportDraft> policies)
    {
        var token = Guid.NewGuid();
        _previewCache.Set(new ImportPreviewSession
        {
            Token = token,
            OrganizationId = _currentUser.OrganizationId,
            Kind = kind,
            MatchStrategy = matchStrategy,
            Clients = clients,
            Policies = policies
        });
        return token;
    }

    private ImportPreviewSession RequireSession(Guid token, ImportKind kind)
    {
        var session = _previewCache.Get(token);
        if (session is null || session.OrganizationId != _currentUser.OrganizationId || session.Kind != kind)
        {
            throw new BusinessRuleException("This preview has expired. Upload the file again.");
        }

        return session;
    }

    private static ImportPreviewDto<ClientImportRowDto> ToClientPreview(Guid token, IReadOnlyList<ClientImportDraft> drafts)
    {
        return new ImportPreviewDto<ClientImportRowDto>
        {
            PreviewToken = token,
            TotalRows = drafts.Count,
            ValidCount = drafts.Count(draft => draft.IsValid),
            InvalidCount = drafts.Count(draft => !draft.IsValid),
            Rows = drafts.Select(draft => new ImportPreviewRowDto<ClientImportRowDto>
            {
                RowNumber = draft.RowNumber,
                IsValid = draft.IsValid,
                Error = draft.Error,
                Values = draft.Values
            }).ToList()
        };
    }

    private static ImportPreviewDto<PolicyImportRowDto> ToPolicyPreview(
        Guid token,
        ClientMatchStrategy matchBy,
        IReadOnlyList<PolicyImportDraft> drafts)
    {
        return new ImportPreviewDto<PolicyImportRowDto>
        {
            PreviewToken = token,
            TotalRows = drafts.Count,
            ValidCount = drafts.Count(draft => draft.IsValid),
            InvalidCount = drafts.Count(draft => !draft.IsValid),
            MatchStrategy = matchBy.ToString(),
            Rows = drafts.Select(draft => new ImportPreviewRowDto<PolicyImportRowDto>
            {
                RowNumber = draft.RowNumber,
                IsValid = draft.IsValid,
                Error = draft.Error,
                Values = draft.Values
            }).ToList()
        };
    }

    private async Task<HashSet<string>> LoadExistingClientCodesAsync(CancellationToken cancellationToken)
    {
        var codes = await _dbContext.Clients.AsNoTracking()
            .Select(client => client.ClientCode)
            .ToListAsync(cancellationToken);
        return codes.ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    private async Task<PolicyMatchContext> LoadPolicyMatchContextAsync(CancellationToken cancellationToken)
    {
        var clients = await _dbContext.Clients.AsNoTracking()
            .Select(client => new ClientMatchRow(client.Id, client.ClientCode, client.CompanyName, client.Phone))
            .ToListAsync(cancellationToken);

        var insurers = await _dbContext.Insurers.AsNoTracking()
            .Select(insurer => new InsurerMatchRow(insurer.Id, insurer.Code, insurer.Name))
            .ToListAsync(cancellationToken);

        var policyNumbers = await _dbContext.Policies.AsNoTracking()
            .Select(policy => policy.PolicyNumber)
            .ToListAsync(cancellationToken);

        var byCode = clients
            .GroupBy(client => client.ClientCode, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);
        var byNamePhone = clients
            .GroupBy(client => NamePhoneKey(client.CompanyName, client.Phone), StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.ToList(), StringComparer.Ordinal);

        var insurersByCode = insurers
            .GroupBy(insurer => insurer.Code, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);
        var insurersByName = insurers
            .GroupBy(insurer => insurer.Name.Trim(), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.ToList(), StringComparer.OrdinalIgnoreCase);

        return new PolicyMatchContext(
            byCode,
            byNamePhone,
            insurersByCode,
            insurersByName,
            policyNumbers.ToHashSet(StringComparer.OrdinalIgnoreCase),
            clients.Select(client => client.Id).ToHashSet(),
            insurers.Select(insurer => insurer.Id).ToHashSet());
    }

    private static string NamePhoneKey(string name, string phone) =>
        $"{name.Trim().ToLowerInvariant()}|{ImportValueParser.DigitsOnly(phone)}";

    private static string? NullIfEmpty(string value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string DefaultIfEmpty(string value, string fallback) =>
        string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();

    private sealed record ClientMatchRow(long Id, string ClientCode, string CompanyName, string Phone);

    private sealed record InsurerMatchRow(long Id, string Code, string Name);

    private sealed record PolicyMatchContext(
        Dictionary<string, ClientMatchRow> ClientsByCode,
        Dictionary<string, List<ClientMatchRow>> ClientsByNamePhone,
        Dictionary<string, InsurerMatchRow> InsurersByCode,
        Dictionary<string, List<InsurerMatchRow>> InsurersByName,
        HashSet<string> PolicyNumbers,
        HashSet<long> ClientIds,
        HashSet<long> InsurerIds);
}
