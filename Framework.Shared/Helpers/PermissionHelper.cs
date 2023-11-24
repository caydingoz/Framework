using Framework.Shared.Enums;

namespace Framework.Shared.Helpers
{
    public static class PermissionHelper
    {
        public static bool HasPermission(Permissions userPermissions, Permissions requiredPermission)
        {
            return (userPermissions & requiredPermission) == requiredPermission;
        }

        public static Permissions AddPermission(Permissions userPermissions, Permissions newPermission)
        {
            return userPermissions | newPermission;
        }

        public static Permissions RemovePermission(Permissions userPermissions, Permissions permissionToRemove)
        {
            return userPermissions & ~permissionToRemove;
        }
    }
}
