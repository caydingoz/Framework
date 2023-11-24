namespace Framework.Shared.Enums
{
    [Flags]
    public enum PermissionTypes
    {
        None = 0,
        Read = 1,
        Write = 2,
        Delete = 4,
    }
}