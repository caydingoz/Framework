﻿namespace Framework.Shared.Enums
{
    [Flags]
    public enum Permissions
    {
        None = 0,
        Read = 1,
        Write = 2,
        Delete = 4,
        Admin = 8
    }
}
