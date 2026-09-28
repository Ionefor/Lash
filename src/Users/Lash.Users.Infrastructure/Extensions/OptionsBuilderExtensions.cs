using Lash.Users.Application.Constants;
using Lash.Users.Infrastructure.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Lash.Users.Infrastructure.Extensions;

public static class OptionsBuilderExtensions
{
    public static OptionsBuilder<AdminOptions> ValidateForSeed(
        this OptionsBuilder<AdminOptions> builder,
        bool isSeedEnabled) =>
        builder
            .Validate(
                options => !isSeedEnabled || HasCompleteAdminCredentials(options),
                "Admin:Email and Admin:Password must be specified together when database seed is enabled.")
            .ValidateOnStart();

    public static OptionsBuilder<RolePermissionOptions> ValidateForSeed(
        this OptionsBuilder<RolePermissionOptions> builder,
        bool isSeedEnabled) =>
        builder
            .Validate(
                options => !isSeedEnabled || HasValidRolePermissions(options),
                "RolePermissions must contain known roles and permission codes that are non-empty and unique when specified.")
            .ValidateOnStart();

    private static bool HasCompleteAdminCredentials(AdminOptions options) =>
        string.IsNullOrWhiteSpace(options.Email) == string.IsNullOrWhiteSpace(options.Password);

    private static bool HasValidRolePermissions(RolePermissionOptions options)
    {
        if (options.Permissions is null || options.Roles is null)
        {
            return false;
        }

        return options.Permissions.All(group =>
                   !string.IsNullOrWhiteSpace(group.Key) && HasValidCodes(group.Value)) &&
               options.Roles.All(role =>
                   IsKnownRole(role.Key) && HasValidCodes(role.Value));
    }

    private static bool HasValidCodes(IEnumerable<string>? codes) =>
        codes is not null &&
        codes.All(code => !string.IsNullOrWhiteSpace(code)) &&
        codes.Distinct(StringComparer.Ordinal).Count() == codes.Count();

    private static bool IsKnownRole(string roleName) =>
        roleName is AccountRoleNames.Client or AccountRoleNames.Master or AccountRoleNames.Admin;
}
