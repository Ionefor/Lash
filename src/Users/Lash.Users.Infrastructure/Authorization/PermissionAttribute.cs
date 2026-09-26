using Microsoft.AspNetCore.Authorization;

namespace Lash.Users.Infrastructure.Authorization;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public sealed class PermissionAttribute(string code) : AuthorizeAttribute(code);
