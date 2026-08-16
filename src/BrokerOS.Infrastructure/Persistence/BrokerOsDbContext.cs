using BrokerOS.Application.Abstractions;
using BrokerOS.Domain.Common;
using BrokerOS.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BrokerOS.Infrastructure.Persistence;

/// <summary>
/// EF Core context for BrokerOS. Global query filters enforce tenant isolation and hide
/// soft-deleted rows. SaveChanges converts Deleted → IsDeleted for ISoftDeletable entities
/// so "delete" keeps history. CurrentOrganizationId comes from ITenantContext (JWT), never from the request body.
/// </summary>
public sealed class BrokerOsDbContext : DbContext
{
    private readonly ITenantContext _tenantContext;
    private readonly IClock _clock;

    public BrokerOsDbContext(
        DbContextOptions<BrokerOsDbContext> options,
        ITenantContext tenantContext,
        IClock clock)
        : base(options)
    {
        _tenantContext = tenantContext;
        _clock = clock;
    }

    /// <summary>
    /// Tenant id used inside query filters. 0 when unauthenticated so org-scoped filters match nothing
    /// (insurer filter also requires CurrentOrganizationId != 0).
    /// </summary>
    public long CurrentOrganizationId => _tenantContext.OrganizationId ?? 0;

    public DbSet<Organization> Organizations => Set<Organization>();

    public DbSet<User> Users => Set<User>();

    public DbSet<Client> Clients => Set<Client>();

    public DbSet<Contact> Contacts => Set<Contact>();

    public DbSet<Insurer> Insurers => Set<Insurer>();

    public DbSet<Policy> Policies => Set<Policy>();

    public DbSet<Renewal> Renewals => Set<Renewal>();

    public DbSet<WorkTask> Tasks => Set<WorkTask>();

    public DbSet<Activity> Activities => Set<Activity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BrokerOsDbContext).Assembly);
        ApplyTenantAndSoftDeleteFilters(modelBuilder);
    }

    public override int SaveChanges()
    {
        ApplyAuditAndSoftDelete();
        return base.SaveChanges();
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        ApplyAuditAndSoftDelete();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyAuditAndSoftDelete();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        ApplyAuditAndSoftDelete();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    /// <summary>
    /// Prompt 3 tenancy fence. Login/register must IgnoreQueryFilters because tenant context is empty
    /// before JWT middleware runs. Do not copy IgnoreQueryFilters into ordinary CRUD.
    /// Renewal and Activity filter by org only — they are not soft-deleted (workflow + append-only audit).
    /// Insurer allows OrganizationId == null (global panel) in addition to the current org.
    /// </summary>
    private void ApplyTenantAndSoftDeleteFilters(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Organization>().HasQueryFilter(entity => entity.Id == CurrentOrganizationId);

        modelBuilder.Entity<User>().HasQueryFilter(entity =>
            !entity.IsDeleted && entity.OrganizationId == CurrentOrganizationId);

        modelBuilder.Entity<Client>().HasQueryFilter(entity =>
            !entity.IsDeleted && entity.OrganizationId == CurrentOrganizationId);

        modelBuilder.Entity<Contact>().HasQueryFilter(entity =>
            !entity.IsDeleted && entity.OrganizationId == CurrentOrganizationId);

        // Global insurers (OrganizationId null) are visible to every authenticated org; anonymous (id 0) sees none.
        modelBuilder.Entity<Insurer>().HasQueryFilter(entity =>
            CurrentOrganizationId != 0
            && (entity.OrganizationId == null || entity.OrganizationId == CurrentOrganizationId));

        modelBuilder.Entity<Policy>().HasQueryFilter(entity =>
            !entity.IsDeleted && entity.OrganizationId == CurrentOrganizationId);

        modelBuilder.Entity<Renewal>().HasQueryFilter(entity =>
            entity.OrganizationId == CurrentOrganizationId);

        modelBuilder.Entity<WorkTask>().HasQueryFilter(entity =>
            !entity.IsDeleted && entity.OrganizationId == CurrentOrganizationId);

        modelBuilder.Entity<Activity>().HasQueryFilter(entity =>
            entity.OrganizationId == CurrentOrganizationId);
    }

    private void ApplyAuditAndSoftDelete()
    {
        var utcNow = _clock.UtcNow;
        var userIdentifier = string.IsNullOrWhiteSpace(_tenantContext.CurrentUserIdentifier)
            ? "system"
            : _tenantContext.CurrentUserIdentifier;

        foreach (var entry in ChangeTracker.Entries())
        {
            // Soft-delete instead of SQL DELETE so policy/client history remains attributable.
            if (entry.State == EntityState.Deleted && entry.Entity is ISoftDeletable softDeletable)
            {
                entry.State = EntityState.Modified;
                softDeletable.IsDeleted = true;
            }

            if (entry.Entity is Entity entity && entity.PublicId == Guid.Empty)
            {
                entity.PublicId = Guid.NewGuid();
            }

            switch (entry.State)
            {
                case EntityState.Added:
                    if (entry.Entity is IAudited addedAudited)
                    {
                        if (addedAudited.CreatedAtUtc == default)
                        {
                            addedAudited.CreatedAtUtc = utcNow;
                        }

                        addedAudited.CreatedBy ??= userIdentifier;
                    }
                    else if (entry.Entity is Organization organization && organization.CreatedAtUtc == default)
                    {
                        organization.CreatedAtUtc = utcNow;
                    }
                    else if (entry.Entity is Contact contact && contact.CreatedAtUtc == default)
                    {
                        contact.CreatedAtUtc = utcNow;
                    }
                    else if (entry.Entity is Insurer insurer && insurer.CreatedAtUtc == default)
                    {
                        insurer.CreatedAtUtc = utcNow;
                    }
                    else if (entry.Entity is Activity activity && activity.CreatedAtUtc == default)
                    {
                        activity.CreatedAtUtc = utcNow;
                    }

                    break;

                case EntityState.Modified:
                    if (entry.Entity is IAudited modifiedAudited)
                    {
                        modifiedAudited.ModifiedAtUtc = utcNow;
                        modifiedAudited.ModifiedBy = userIdentifier;
                    }
                    else if (entry.Entity is Organization modifiedOrganization)
                    {
                        modifiedOrganization.ModifiedAtUtc = utcNow;
                    }
                    else if (entry.Entity is Contact modifiedContact)
                    {
                        modifiedContact.ModifiedAtUtc = utcNow;
                    }
                    else if (entry.Entity is Insurer modifiedInsurer)
                    {
                        modifiedInsurer.ModifiedAtUtc = utcNow;
                    }

                    break;
            }
        }
    }
}
