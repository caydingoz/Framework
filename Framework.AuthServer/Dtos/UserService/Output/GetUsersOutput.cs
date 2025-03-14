﻿using Framework.AuthServer.Enums;
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
        public string PhoneNumber { get; set; } = string.Empty;
        public required string Email { get; set; }
        public required string Title { get; set; }
        public string? Image { get; set; }
        public UserStatus Status { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public IEnumerable<UserRoleOutput> Roles { get; set; } = [];
    }
    public class UserRoleOutput
    {
        public required int Id { get; set; }
        public required string Name { get; set; }
    }
}
