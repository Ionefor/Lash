using Microsoft.AspNetCore.Authorization;

namespace Lash.Users.Infrastructure.Authorization;

public sealed record PermissionRequirement(string Code) : IAuthorizationRequirement;
