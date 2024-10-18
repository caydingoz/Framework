using System.ComponentModel.DataAnnotations;

namespace Framework.AuthServer.Dtos.UserService.Input
{
    public class UpdateUserInput
    {
        public required Guid Id { get; set; }
        [StringLength(100)]
        public required string Email { get; set; }
        [StringLength(100)]
        public required string FirstName { get; set; }
        [StringLength(100)]
        public required string LastName { get; set; }
        [StringLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;
        [StringLength(50)]
        public string Title { get; set; } = string.Empty;
        [StringLength(50)]
        public string Image { get; set; } = string.Empty;
        public ICollection<int> RoleIds { get; set; } = [];
    }
}
