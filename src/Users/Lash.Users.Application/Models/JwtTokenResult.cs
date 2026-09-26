namespace Lash.Users.Application.Models;

public sealed record JwtTokenResult(string AccessToken, Guid Jti);
