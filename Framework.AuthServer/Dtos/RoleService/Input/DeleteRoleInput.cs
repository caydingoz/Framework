namespace Framework.AuthServer.Dtos.RoleService.Input
{
    public class DeleteRoleInput
    {
        public required ICollection<int> Ids { get; set; }
    }
}
