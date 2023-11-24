using Framework.Shared.Consts;
using Framework.Shared.Enums;
using Microsoft.AspNetCore.Authorization;

namespace Framework.Shared.Helpers
{
    public static class PermissionHelper
    {
        public static Action<AuthorizationOptions> SetPolicies(ICollection<string> pages)
        {
            return options =>
            {
                foreach (var page in pages)
                {
                    options.AddPolicy($"{page}{PermissionAccessTypes.ReadAccess}",
                        policy => policy.RequireAssertion(x =>
                            x.User.HasClaim(y => y.Type == "permissions" && (y.Value == $"{page}:100" || y.Value == $"{page}:010" || y.Value == $"{page}:001")
                        ))
                    );
                    options.AddPolicy($"{page}{PermissionAccessTypes.WriteAccess}",
                        policy => policy.RequireAssertion(x =>
                            x.User.HasClaim(y => y.Type == "permissions" && (y.Value == $"{page}:100" || y.Value == $"{page}:010")
                        ))
                    );
                    options.AddPolicy($"{page}{PermissionAccessTypes.DeleteAccess}",
                        policy => policy.RequireAssertion(x =>
                            x.User.HasClaim(y => y.Type == "permissions" && y.Value == $"{page}:100")
                        )
                    );
                }
            };
        }
        public static bool HasPermission(PermissionTypes userPermissions, PermissionTypes requiredPermission)
        {
            return (userPermissions & requiredPermission) == requiredPermission;
        }

        public static PermissionTypes AddPermission(PermissionTypes userPermissions, PermissionTypes newPermission)
        {
            return userPermissions | newPermission;
        }

        public static PermissionTypes RemovePermission(PermissionTypes userPermissions, PermissionTypes permissionToRemove)
        {
            return userPermissions & ~permissionToRemove;
        }
    }
}
