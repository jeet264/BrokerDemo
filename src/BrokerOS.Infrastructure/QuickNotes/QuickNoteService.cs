using BrokerOS.Application.Abstractions;
using BrokerOS.Application.QuickNotes;
using BrokerOS.Application.Security;
using BrokerOS.Domain.Activities;
using BrokerOS.Domain.Entities;
using BrokerOS.Domain.Enums;
using BrokerOS.Infrastructure.Persistence;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;

namespace BrokerOS.Infrastructure.QuickNotes;

/// <summary>
/// Saves a desk note as an <see cref="Activity"/> and, when asked, a follow-up <see cref="WorkTask"/>.
/// </summary>
/// <remarks>
/// Intentionally NOT doing AI/NLP parsing of the note in this version. Follow-up tasks are created
/// only when the broker ticks "Also create a follow-up task" — we do not infer intent from the text.
/// This is the natural place to add that later (same family as future AI document scanning): a later
/// sender could suggest the checkbox from the wording, but CreateAsync should stay the single write
/// path so the UI and API do not grow a second parser.
/// </remarks>
public sealed class QuickNoteService : IQuickNoteService
{
    private readonly BrokerOsDbContext _dbContext;
    private readonly ICurrentUserService _currentUser;
    private readonly IClock _clock;

    public QuickNoteService(BrokerOsDbContext dbContext, ICurrentUserService currentUser, IClock clock)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<QuickNoteDto> CreateAsync(CreateQuickNoteRequest request, CancellationToken cancellationToken)
    {
        var text = request.Text.Trim();
        var client = await ResolveClientAsync(request.ClientPublicId, cancellationToken);
        var renewal = await ResolveRenewalAsync(request.RenewalPublicId, cancellationToken);

        if (client is not null && renewal is not null && renewal.Policy.ClientId != client.Id)
        {
            throw new ValidationException([
                new ValidationFailure("RenewalPublicId", "Renewal does not belong to the selected client.")
            ]);
        }

        if (client is null && renewal is not null)
        {
            client = renewal.Policy.Client;
        }

        var activity = new Activity
        {
            OrganizationId = _currentUser.OrganizationId,
            ClientId = client?.Id,
            PolicyId = renewal?.PolicyId,
            RenewalId = renewal?.Id,
            UserId = _currentUser.UserId,
            ActivityType = ActivityType.Note,
            Description = text
        };
        _dbContext.Activities.Add(activity);

        WorkTask? task = null;
        if (request.CreateFollowUpTask)
        {
            var due = request.TaskDueDateUtc.HasValue
                ? DateTime.SpecifyKind(request.TaskDueDateUtc.Value, DateTimeKind.Utc)
                : _clock.UtcNow.AddDays(1);

            task = new WorkTask
            {
                OrganizationId = _currentUser.OrganizationId,
                RenewalId = renewal?.Id,
                ClientId = client?.Id,
                PolicyId = renewal?.PolicyId,
                AssignedUserId = _currentUser.UserId,
                Title = QuickNoteText.FollowUpTitle(text),
                Description = text,
                DueDateUtc = due,
                Priority = TaskPriority.Medium,
                Status = WorkTaskStatus.Pending
            };
            _dbContext.Tasks.Add(task);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new QuickNoteDto
        {
            ActivityPublicId = activity.PublicId,
            TaskPublicId = task?.PublicId,
            ClientPublicId = client?.PublicId,
            ClientName = client?.CompanyName,
            RenewalPublicId = renewal?.PublicId,
            PolicyNumber = renewal?.Policy.PolicyNumber,
            Text = text,
            FollowUpTaskCreated = task is not null,
            CreatedAtUtc = activity.CreatedAtUtc
        };
    }

    private async Task<Client?> ResolveClientAsync(Guid? publicId, CancellationToken cancellationToken)
    {
        if (!publicId.HasValue)
        {
            return null;
        }

        var client = await _dbContext.Clients
            .ForCurrentUser(_currentUser)
            .SingleOrDefaultAsync(entity => entity.PublicId == publicId.Value, cancellationToken);
        AssignmentScope.EnsureFound(client);
        return client;
    }

    private async Task<Renewal?> ResolveRenewalAsync(Guid? publicId, CancellationToken cancellationToken)
    {
        if (!publicId.HasValue)
        {
            return null;
        }

        var renewal = await _dbContext.Renewals
            .Include(entity => entity.Policy)
                .ThenInclude(policy => policy.Client)
            .ForCurrentUser(_currentUser)
            .SingleOrDefaultAsync(entity => entity.PublicId == publicId.Value, cancellationToken);
        AssignmentScope.EnsureFound(renewal);
        return renewal;
    }
}
