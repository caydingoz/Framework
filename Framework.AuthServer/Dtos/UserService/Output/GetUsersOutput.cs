using Framework.AuthServer.Enums;
using Framework.Shared.Dtos;

namespace Framework.AuthServer.Dtos.UserService.Output
{
    public class GetUsersOutput : PageOutput
    {
        public ICollection<UserOutput> Users { get; set; } = new List<UserOutput>();
    }
    public class UserOutput
    {
        public required Guid Id { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string Email { get; set; }
        public required string Title { get; set; }
        public string? Image { get; set; }
        public UserStatusEnum Status { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public IEnumerable<string> Roles { get; set; } = [];
    }
}
