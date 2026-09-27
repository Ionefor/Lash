using CSharpFunctionalExtensions;
using ErrorsFlow.Errors;
using ErrorsFlow.Models;

namespace Lash.Users.Domain;

public sealed class RefreshSession
{
    private RefreshSession()
    {
    }

    private RefreshSession(
        Guid userId,
        Guid jti,
        string tokenHash,
        DateTimeOffset createdAt,
        DateTimeOffset expiresAt,
        DateTimeOffset absoluteExpiresAt)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        Jti = jti;
        TokenHash = tokenHash;
        CreatedAt = createdAt;
        ExpiresAt = expiresAt;
        AbsoluteExpiresAt = absoluteExpiresAt;
    }

    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Guid Jti { get; private set; }
    public string TokenHash { get; private set; } = string.Empty;
    public DateTimeOffset ExpiresAt { get; private set; }
    public DateTimeOffset AbsoluteExpiresAt { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? RevokedAt { get; private set; }

    public bool IsRevoked => RevokedAt is not null;

    public UnitResult<Error> EnsureActive(DateTimeOffset currentTime)
    {
        if (IsRevoked)
        {
            return UnitResult.Failure(AuthErrors.RefreshTokenInvalid());
        }

        if (ExpiresAt <= currentTime || AbsoluteExpiresAt <= currentTime)
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
        DateTimeOffset expiresAt,
        DateTimeOffset? absoluteExpiresAt = null)
    {
        if (userId == Guid.Empty)
        {
            return Result.Failure<RefreshSession, Error>(GeneralErrors.ValueIsInvalid("userId"));
        }

        if (jti == Guid.Empty)
        {
            return Result.Failure<RefreshSession, Error>(GeneralErrors.ValueIsInvalid("jti"));
        }

        if (string.IsNullOrWhiteSpace(tokenHash))
        {
            return Result.Failure<RefreshSession, Error>(GeneralErrors.ValueIsRequired("tokenHash"));
        }

        if (expiresAt <= createdAt)
        {
            return Result.Failure<RefreshSession, Error>(GeneralErrors.ValueIsInvalid("expiresAt"));
        }

        var effectiveAbsoluteExpiresAt = absoluteExpiresAt ?? expiresAt;
        if (effectiveAbsoluteExpiresAt < expiresAt)
        {
            return Result.Failure<RefreshSession, Error>(GeneralErrors.ValueIsInvalid("absoluteExpiresAt"));
        }

        return Result.Success<RefreshSession, Error>(new RefreshSession(
            userId,
            jti,
            tokenHash,
            createdAt,
            expiresAt,
            effectiveAbsoluteExpiresAt));
    }

    public UnitResult<Error> Revoke(DateTimeOffset revokedAt)
    {
        if (IsRevoked)
        {
            return UnitResult.Success<Error>();
        }

        if (revokedAt < CreatedAt)
        {
            return UnitResult.Failure(GeneralErrors.ValueIsInvalid("revokedAt"));
        }

        RevokedAt = revokedAt;
        return UnitResult.Success<Error>();
    }
}
