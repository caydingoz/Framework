using Framework.AuthServer.Enums;
using Framework.Domain.Entites;
using System.ComponentModel.DataAnnotations;

namespace Framework.AuthServer.Models;

public class User : Entity<Guid>
{
    [StringLength(100)]
    public required string Email { get; set; }
    [StringLength(100)]
    public required string FirstName { get; set; }
    [StringLength(100)]
    public required string LastName { get; set; }
    [StringLength(20)]
    public string PhoneNumber { get; set; } = string.Empty;
    public required string Password { get; set; }
    public bool EmailConfirmed { get; set; }
    public bool PhoneNumberConfirmed { get; set; }
    [StringLength(50)]
    public string Title { get; set; } = string.Empty;
    public UserStatusEnum Status { get; set; }
    [StringLength(50)]
    public string Image { get; set; } = string.Empty;
    public ICollection<Role> Roles { get; set; } = [];
}