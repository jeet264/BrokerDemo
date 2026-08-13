using BrokerOS.Application.Abstractions;
using BrokerOS.Application.Auth;
using BrokerOS.Domain.Entities;
using BrokerOS.Domain.Enums;
using BrokerOS.Domain.Exceptions;
using BrokerOS.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BrokerOS.Infrastructure.Auth;

public sealed class AuthService : IAuthService
{
    private readonly BrokerOsDbContext _dbContext;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ICurrentUserService _currentUser;
    private readonly IClock _clock;

    public AuthService(
        BrokerOsDbContext dbContext,
        IPasswordHasher<User> passwordHasher,
        IJwtTokenService jwtTokenService,
        ICurrentUserService currentUser,
        IClock clock)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var email = NormalizeEmail(request.Email);

        var user = await _dbContext.Users
            .IgnoreQueryFilters()
            .Include(entity => entity.Organization)
            .Where(entity => entity.Email == email && !entity.IsDeleted)
            .OrderBy(entity => entity.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (user is null || !user.IsActive || !user.Organization.IsActive)
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        var verification = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (verification == PasswordVerificationResult.Failed)
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        user.LastLoginAtUtc = _clock.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return CreateAuthResponse(user);
    }

    public async Task<AuthResponseDto> RegisterOrganizationAsync(
        RegisterOrganizationRequest request,
        CancellationToken cancellationToken)
    {
        var email = NormalizeEmail(request.AdminEmail);
        var code = request.OrganizationCode.Trim().ToUpperInvariant();

        var emailTaken = await _dbContext.Users
            .IgnoreQueryFilters()
            .AnyAsync(entity => entity.Email == email && !entity.IsDeleted, cancellationToken);

        if (emailTaken)
        {
            throw new ConflictException("An account with this email already exists.");
        }

        var codeTaken = await _dbContext.Organizations
            .IgnoreQueryFilters()
            .AnyAsync(entity => entity.Code == code, cancellationToken);

        if (codeTaken)
        {
            throw new ConflictException("An organization with this code already exists.");
        }

        var organization = new Organization
        {
            Name = request.OrganizationName.Trim(),
            Code = code,
            IsActive = true
        };

        var admin = new User
        {
            Organization = organization,
            Email = email,
            FullName = request.AdminFullName.Trim(),
            Role = UserRole.BrokerAdmin,
            IsActive = true,
            CreatedBy = email
        };
        admin.PasswordHash = _passwordHasher.HashPassword(admin, request.AdminPassword);

        _dbContext.Organizations.Add(organization);
        _dbContext.Users.Add(admin);
        await _dbContext.SaveChangesAsync(cancellationToken);

        admin.Organization = organization;
        return CreateAuthResponse(admin);
    }

    public async Task<CurrentUserDto> GetCurrentUserAsync(CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated)
        {
            throw new UnauthorizedAccessException("Authentication is required.");
        }

        var user = await _dbContext.Users
            .Include(entity => entity.Organization)
            .SingleOrDefaultAsync(entity => entity.Id == _currentUser.UserId, cancellationToken);

        if (user is null || !user.IsActive || !user.Organization.IsActive)
        {
            throw new UnauthorizedAccessException("The account is no longer active.");
        }

        return MapCurrentUser(user);
    }

    private AuthResponseDto CreateAuthResponse(User user)
    {
        var (token, expiresAtUtc) = _jwtTokenService.CreateAccessToken(user);

        return new AuthResponseDto
        {
            AccessToken = token,
            ExpiresAtUtc = expiresAtUtc,
            User = MapCurrentUser(user)
        };
    }

    private static CurrentUserDto MapCurrentUser(User user) =>
        new()
        {
            PublicUserId = user.PublicId,
            Email = user.Email,
            FullName = user.FullName,
            Role = user.Role.ToString(),
            OrganizationPublicId = user.Organization.PublicId,
            OrganizationName = user.Organization.Name,
            OrganizationCode = user.Organization.Code
        };

    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();
}
