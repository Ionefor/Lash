using CSharpFunctionalExtensions;
using ErrorsFlow.Errors;
using ErrorsFlow.Models;

namespace Lash.Users.Domain;

public sealed class RefreshSession
{
    private RefreshSession()
    {
    }

    private RefreshSession(Guid userId, Guid jti, string tokenHash, DateTimeOffset createdAt, DateTimeOffset expiresAt)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        Jti = jti;
        TokenHash = tokenHash;
        CreatedAt = createdAt;
        ExpiresAt = expiresAt;
    }

    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Guid Jti { get; private set; }
    public string TokenHash { get; private set; } = string.Empty;
    public DateTimeOffset ExpiresAt { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? RevokedAt { get; private set; }

    public bool IsRevoked => RevokedAt is not null;

    public UnitResult<Error> EnsureActive(DateTimeOffset currentTime)
    {
        if (IsRevoked)
        {
            return UnitResult.Failure(AuthErrors.RefreshTokenInvalid());
        }

        if (ExpiresAt <= currentTime)
        {
            return UnitResult.Failure(AuthErrors.RefreshTokenExpired());
        }

        return UnitResult.Success<Error>();
    }

    public static Result<RefreshSession, Error> Create(
        Guid userId,
        Guid jti,
        string tokenHash,
        DateTimeOffset createdAt,
        DateTimeOffset expiresAt)
    {
        if (userId == Guid.Empty)
        {
            return Result.Failure<RefreshSession, Error>(GeneralErrors.ValueIsInvalid(nameof(UserId)));
        }

        if (jti == Guid.Empty)
        {
            return Result.Failure<RefreshSession, Error>(GeneralErrors.ValueIsInvalid(nameof(Jti)));
        }

        if (string.IsNullOrWhiteSpace(tokenHash))
        {
            return Result.Failure<RefreshSession, Error>(GeneralErrors.ValueIsRequired(nameof(TokenHash)));
        }

        if (expiresAt <= createdAt)
        {
            return Result.Failure<RefreshSession, Error>(GeneralErrors.ValueIsInvalid(nameof(ExpiresAt)));
        }

        return Result.Success<RefreshSession, Error>(new RefreshSession(userId, jti, tokenHash, createdAt, expiresAt));
    }

    public UnitResult<Error> Revoke(DateTimeOffset revokedAt)
    {
        if (IsRevoked)
        {
            return UnitResult.Success<Error>();
        }

        if (revokedAt < CreatedAt)
        {
            return UnitResult.Failure(GeneralErrors.ValueIsInvalid(nameof(revokedAt)));
        }

        RevokedAt = revokedAt;
        return UnitResult.Success<Error>();
    }
}
