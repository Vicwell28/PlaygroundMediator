using Microsoft.AspNetCore.Authorization;
using PlaygroundMediator.Constants;

namespace PlaygroundMediator.Extensions
{
    public static class AuthorizationPolicyBuilderExtensions
    {
        public static void AddCustomAuthorizationPolicies(this AuthorizationOptions options)
        {
            // Políticas basadas en permisos
            options.AddPolicy(PolicyNames.CanReadProducts, policy =>
                policy.RequireClaim(Permissions.Permission, Permissions.ReadProducts));

            options.AddPolicy(PolicyNames.CanCreateProducts, policy =>
                policy.RequireClaim(Permissions.Permission, Permissions.CreateProducts));

            options.AddPolicy(PolicyNames.CanUpdateProducts, policy =>
                policy.RequireClaim(Permissions.Permission, Permissions.UpdateProducts));

            options.AddPolicy(PolicyNames.CanDeleteProducts, policy =>
                policy.RequireClaim(Permissions.Permission, Permissions.DeleteProducts));

            // Políticas basadas en roles
            options.AddPolicy(PolicyNames.IsAdmin, policy =>
                policy.RequireRole(Roles.Admin));

            // Políticas combinadas (roles y permisos)
            options.AddPolicy(PolicyNames.AdminCanCreateProducts, policy =>
            {
                policy.RequireRole(Roles.Admin);
                policy.RequireClaim(Permissions.Permission, Permissions.CreateProducts);
            });

            options.AddPolicy(PolicyNames.AdminOrUserCanCreateProducts, policy =>
            {
                policy.RequireRole(Roles.Admin, Roles.User);
                policy.RequireClaim(Permissions.Permission, Permissions.CreateProducts);
            });
        }
    }
}