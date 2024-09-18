namespace Framework.AuthServer.Dtos.UserService.Input
{
    public class DeleteUserInput
    {
        public required ICollection<Guid> Ids { get; set; }
    }
}
