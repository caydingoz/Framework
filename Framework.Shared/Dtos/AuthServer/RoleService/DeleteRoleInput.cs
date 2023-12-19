namespace Framework.Shared.Dtos.AuthServer.RoleService
{
    public class DeleteRoleInput
    {
        public required ICollection<string> Ids { get; set; }
    }
}
